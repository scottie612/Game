using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Game.Configuration;
using Game.Console.Helpers;
using Game.Packets;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace Game.EntityComponentSystem.Systems
{
    public class SpawningSystem : BaseSystem<World, float>
    {
        public QueryDescription _identityQuery = new QueryDescription().WithAll<NewEntityTag, NetworkConnectionComponent>();
        public QueryDescription _newEntitiesQuery = new QueryDescription().WithAll<NewEntityTag, EntityTypeComponent, PositionComponent>();
        public QueryDescription _deleteEntitiesQuery = new QueryDescription().WithAll<DeleteEntityTag, EntityTypeComponent>();
        public QueryDescription _despawnAfterDistanceQuery = new QueryDescription().WithAll<DestroyAfterDistanceComponent, PositionComponent>();

        private NetManager _netManager;
        private NetDataWriter _netDataWriter;
        private BatchPacketProcessor _batchPacketProcessor;

        public SpawningSystem(World world, NetManager netManager) : base(world)
        {
            _netManager = netManager;
            _netDataWriter = new NetDataWriter();
            _batchPacketProcessor = new BatchPacketProcessor(Packet.EntitySpawned, DeliveryMethod.ReliableOrdered, _netDataWriter, _netManager);
        }

        public override void Update(in float deltaTime)
        {
            var time = deltaTime / 1000;
            DespawnAfterDistance();
            SendIdentity();
            SendDespawnedEntites();
            SendSpawnedEntities();
           
        }

        private void DespawnAfterDistance()
        {
            var buffer = new CommandBuffer();
            World.Query(in _despawnAfterDistanceQuery, (Entity entity, ref PositionComponent pos, ref DestroyAfterDistanceComponent dad) =>
            {
                var distanceTraveled = (pos.Value - dad.StartingPosition).Length();
                if (distanceTraveled > dad.Distance)
                {
                    buffer.Add<DeleteEntityTag>(entity);
                }
            });
            buffer.Playback(World);
            buffer.Dispose();
        }

        private void SendDespawnedEntites()
        {
            World.Query(in _deleteEntitiesQuery, (Entity entity, ref EntityTypeComponent type) =>
            {
                var packet = new EntityDespawnedPacket();
                packet.EntityID = entity.Id;
                packet.Type = type.Type;
                packet.Serialize(_netDataWriter);
                _netManager.SendToAll(_netDataWriter, DeliveryMethod.ReliableUnordered);
                _netDataWriter.Reset();
            });

            World.Destroy(in _deleteEntitiesQuery);
        }

        private void SendSpawnedEntities()
        {
            var buffer = new CommandBuffer();
            World.Query(in _newEntitiesQuery, (Entity newEntity, ref EntityTypeComponent newEntityType, ref PositionComponent newEntityPos) =>
            {
                // Let existing Players know about new Entity
                var packet = new EntitySpawnedPacket();
                packet.EntityID = newEntity.Id;
                packet.Type = newEntityType.Type;
                packet.StartingX = newEntityPos.Value.X;
                packet.StartingY = newEntityPos.Value.Y;
                packet.Serialize(_netDataWriter);
                _netManager.SendToAll(_netDataWriter, DeliveryMethod.ReliableOrdered);
                _netDataWriter.Reset();

                //If they are a player, Let them know about all other Existing Entities
                if (newEntityType.Type == EntityType.Player)
                {
                    if(newEntity.TryGet<NetworkConnectionComponent>(out var ncc))
                    {
                        var peer = ncc.Peer;

                        var existingEntitiesquery = new QueryDescription().WithAll<EntityTypeComponent, PositionComponent>().WithNone<NewEntityTag>();
                        World.Query(in existingEntitiesquery, (Entity existingEntity, ref EntityTypeComponent existingEntityType, ref PositionComponent existingEntityPos) =>
                        {
                            var packet = new EntitySpawnedPacket();
                            packet.EntityID = existingEntity.Id;
                            packet.Type = existingEntityType.Type;
                            packet.StartingX = existingEntityPos.Value.X;
                            packet.StartingY = existingEntityPos.Value.Y;
                            packet.Serialize(_netDataWriter);
                            peer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
                            _netDataWriter.Reset();
                        });
                    }

                }

                buffer.Remove<NewEntityTag>(newEntity);
               
            });
            buffer.Playback(World);
            buffer.Dispose();
        }
        private void SendIdentity()
        {
            World.Query(in _identityQuery, (Entity entity, ref NetworkConnectionComponent pc) =>
            {
                var packet = new IdentityPacket();
                packet.EntityID = entity.Id;
                packet.Serialize(_netDataWriter);
                pc.Peer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
                _netDataWriter.Reset();
            });
        }

    }
}
