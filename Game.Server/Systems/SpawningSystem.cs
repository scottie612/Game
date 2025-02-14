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
        private ILogger<SpawningSystem> _logger;
        public SpawningSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<SpawningSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
        }

        public override void Update(float deltaTime)
        {
            DestroyProjectilesAfterRange();  
            SendSpawnedEntities();
            SendDespawnedEntites();
            DeleteTaggedEntities();
        }

        public QueryDescription _destroyProjectilesAfterRange = new QueryDescription().WithAll<RangeComponent, PositionComponent, ProjectileTag>();
        private void DestroyProjectilesAfterRange()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _destroyProjectilesAfterRange, (Entity entity, ref PositionComponent pos, ref RangeComponent ran) =>
            {
                var distanceTraveled = (pos.Value - ran.StartingPosition).Length();
                if (distanceTraveled > ran.Range)
                {
                    buffer.Add<DeleteEntityTag>(entity);
                }
            });
            buffer.Playback(World.World);
        }


        public QueryDescription _newEntitiesQuery = new QueryDescription().WithAll<NewEntityTag, EntityTypeComponent, PositionComponent>();
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

                //if the entity has a name component, add it to the packet, otherwise use EntityID as the name.
                if (newEntity.TryGet<NameComponent>(out var nameComponent))
                {
                    packet.EntityName = nameComponent.Name;
                }
                else
                {
                    packet.EntityName = newEntity.Id.ToString();
                }

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

                        //if the existing entity has a name component, add it to the packet, otherwise use EntityID as the name.
                        if (existingEntity.TryGet<NameComponent>(out var nameComponent))
                        {
                            packet.EntityName = nameComponent.Name;
                        }
                        else
                        {
                            packet.EntityName = existingEntity.Id.ToString();
                        }
                        if(existingEntity.TryGet<HealthComponent>(out var healComponent))
                        {
                            buffer.Add<HealthDirtyTag>(existingEntity);
                        }
                        if (existingEntity.TryGet<ManaComponent>(out var manaComponent))
                        {
                            buffer.Add<ManaDirtyTag>(existingEntity);
                        }

                        PacketDispatcher.Enqueue(packet);
                    });
                }

                if (newEntity.TryGet<HealthComponent>(out var healComponent))
                {
                    buffer.Add<HealthDirtyTag>(newEntity);
                }
                if (newEntity.TryGet<ManaComponent>(out var manaComponent))
                {
                    buffer.Add<ManaDirtyTag>(newEntity);
                }

                buffer.Remove<NewEntityTag>(newEntity);

            });
            buffer.Playback(World.World);
        }

        public QueryDescription _despawnEntitiesQuery = new QueryDescription().WithAll<DeleteEntityTag, EntityTypeComponent>();
        private void SendDespawnedEntites()
        {
            World.World.Query(in _despawnEntitiesQuery, (Entity entity, ref EntityTypeComponent type) =>
            {
                var packet = new EntityDespawnedPacket();
                packet.EntityID = entity.Id;
                packet.Type = type.Type;
                PacketDispatcher.Enqueue(packet);
            });
        }

        public QueryDescription _deleteEntititesQuery= new QueryDescription().WithAll<DeleteEntityTag>();
        private void DeleteTaggedEntities()
        {
            World.World.Destroy(in _deleteEntititesQuery);
        }
    }
}
