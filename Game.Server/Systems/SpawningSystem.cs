using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Game.Common;
using Game.Packets;
using Game.Server.Components;
using Microsoft.Extensions.Logging;

namespace Game.Server.Systems
{
    public class SpawningSystem : SystemBase
    {
        public QueryDescription _identityQuery = new QueryDescription().WithAll<NewEntityTag, NetworkConnectionComponent>();
        public QueryDescription _newEntitiesQuery = new QueryDescription().WithAll<NewEntityTag, EntityTypeComponent, PositionComponent>();
        public QueryDescription _despawnEntitiesQuery = new QueryDescription().WithAll<DeleteEntityTag, EntityTypeComponent>();
        public QueryDescription _despawnAfterDistanceQuery = new QueryDescription().WithAll<DestroyAfterDistanceComponent, PositionComponent>();

        private ILogger<SpawningSystem> _logger;
        public SpawningSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<SpawningSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
        }

        public override void Update(float deltaTime)
        {
            SendIdentity();
            DespawnAfterDistance();  
            SendSpawnedEntities();
            SendDespawnedEntites();
        }

        private void SendIdentity()
        {
            World.World.Query(in _identityQuery, (Entity entity, ref NetworkConnectionComponent pc) =>
            {
                var packet = new IdentityPacket();
                packet.EntityID = entity.Id;
                packet.NetPeer = pc.Peer;
                PacketDispatcher.Enqueue(packet);
            });
        }
        private void DespawnAfterDistance()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _despawnAfterDistanceQuery, (Entity entity, ref PositionComponent pos, ref DestroyAfterDistanceComponent dad) =>
            {
                var distanceTraveled = (pos.Value - dad.StartingPosition).Length();
                if (distanceTraveled > dad.Distance)
                {
                    buffer.Add<DeleteEntityTag>(entity);
                }
            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }

        private void SendSpawnedEntities()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _newEntitiesQuery, (Entity newEntity, ref EntityTypeComponent newEntityType, ref PositionComponent newEntityPos) =>
            {
                // Let existing Players know about new Entity
                var packet = new EntitySpawnedPacket();
                packet.EntityID = newEntity.Id;
                packet.Type = newEntityType.Type;
                packet.StartingX = newEntityPos.Value.X;
                packet.StartingY = newEntityPos.Value.Y;
                PacketDispatcher.Enqueue(packet);

                //If they are a player, Let them know about all other Existing Entities
                if (newEntity.TryGet<NetworkConnectionComponent>(out var ncc))
                {
                    var existingEntitiesquery = new QueryDescription().WithAll<EntityTypeComponent, PositionComponent>().WithNone<NewEntityTag>();
                    World.World.Query(in existingEntitiesquery, (Entity existingEntity, ref EntityTypeComponent existingEntityType, ref PositionComponent existingEntityPos) =>
                    {
                        var packet = new EntitySpawnedPacket();
                        packet.EntityID = existingEntity.Id;
                        packet.Type = existingEntityType.Type;
                        packet.StartingX = existingEntityPos.Value.X;
                        packet.StartingY = existingEntityPos.Value.Y;
                        packet.NetPeer = ncc.Peer;
                        PacketDispatcher.Enqueue(packet);
                    });
                }


                buffer.Remove<NewEntityTag>(newEntity);

            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }
        private void SendDespawnedEntites()
        {
            World.World.Query(in _despawnEntitiesQuery, (Entity entity, ref EntityTypeComponent type) =>
            {
                var packet = new EntityDespawnedPacket();
                packet.EntityID = entity.Id;
                packet.Type = type.Type;
                PacketDispatcher.Enqueue(packet);
            });

            World.World.Destroy(in _despawnEntitiesQuery);
        }
    }
}
