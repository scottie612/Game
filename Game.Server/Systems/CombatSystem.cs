using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Game.Common;
using Game.Common.Enums;
using Game.Common.Random;
using Game.Server.Components;
using Game.Server.Components.Stats;
using Game.Server.Entities;
using Game.Server.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Numerics;

namespace Game.Server.Systems
{
    public class CombatSystem : SystemBase
    {
        private readonly ILogger<CombatSystem> _logger;
        private readonly ServerOptions _serverOptions;
        public CombatSystem(GameWorld world, PacketDispatcher packetDispatcher, IOptions<ServerOptions> serverOptions, ILogger<CombatSystem> logger) : base(world, packetDispatcher)
        {
            _serverOptions = serverOptions.Value;
            _logger = logger;
        }

        public override void Update(float deltaTime)
        {
            EnsureOrbs();
            CheckForDeath();
        }

        private QueryDescription _ensureOrbsQuery = new QueryDescription().WithAll<OrbTag>();
        private void EnsureOrbs()
        {
            var count = 0;
            World.World.Query(in _ensureOrbsQuery, (Entity entity) =>
            {
                count++;
            });
            var amountToAdd = 50 - count;
            for (int i = 0; i<= amountToAdd; i++)
            {
                var healAmount = RandomHelper.RandomInt(10, 100);
                var position = RandomHelper.RandomVector2(_serverOptions.MaxWorldSize.MinX, _serverOptions.MaxWorldSize.MaxX, _serverOptions.MaxWorldSize.MinY, _serverOptions.MaxWorldSize.MaxY);
                OrbFactory.CreateHealing(World.World, healAmount, position);
            }
        }

        private QueryDescription _deathQuery = new QueryDescription().WithAll<HealthComponent>();
        private void CheckForDeath()
        {
            var buffer = new CommandBuffer();

            World.World.Query(in _deathQuery, (Entity entity, ref HealthComponent hc) =>
            {
                if (hc.CurrentValue <= 0)
                {
                    _logger.LogTrace($"Entity {entity.Id} has died. Respawning...");

                    if (entity.TryGet<EntityTypeComponent>(out var entityType))
                    {
                        //If the entity is a player, respawn them
                        if (entityType.Type == EntityType.Player)
                        {
                            buffer.Set(entity, new PositionComponent { Value = RandomHelper.RandomVector2(_serverOptions.MaxWorldSize.MinX, _serverOptions.MaxWorldSize.MaxX, _serverOptions.MaxWorldSize.MinY, _serverOptions.MaxWorldSize.MaxY) });
                            buffer.Set(entity, new VelocityComponent { Value = new Vector2(0, 0) });
                            buffer.Set(entity, new HealthComponent(100));
                            buffer.Set(entity, new ManaComponent { CurrentValue = 100, MaxValue = 100 });
                            buffer.Add<PositionDiryTag>(entity);
                        }
                    }
                }
            });
            buffer.Playback(World.World);
        }

    }
}
