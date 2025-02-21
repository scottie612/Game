using Arch.Core;
using Arch.Core.Extensions;
using Game.Common.Enums;
using Game.Server.Components;
using Game.Server.Components.Collisions;
using Game.Server.Components.Stats;
using Game.Server.Systems;
using System.Numerics;

namespace Game.Server.Entities
{
    public static class ProjectileFactory
    {

        public static Entity CreateArrow(World world, ref Entity castingEntity, ref Entity weapon, Vector2 direction)
        {
            var startingPosition = castingEntity.Get<PositionComponent>().Value;

            var bullet = world.Create(
                new EntityTypeComponent { Type = EntityType.Arrow },
                new CasterComponent { CastingEntity = castingEntity },
                new PositionComponent { Value = startingPosition },
                new VelocityComponent { Value = direction },
                new MovementSpeedComponent { Value = 20f },
                new RangeComponent { Range = 20, StartingPosition = startingPosition },
                new ColliderComponent
                {
                    Shape = Shape.Circle(0.1f),
                    OnStart = (self, other) =>
                    {
                        if (self.Get<CasterComponent>().CastingEntity == other)
                            return;
                        if (other.TryGet<HealthComponent>(out var health))
                        {
                            health.CurrentValue -= 15;
                            other.Set<HealthComponent>(health);
                            other.Add<HealthDirtyTag>();
                            self.Add<DeleteEntityTag>();
                        }
                    }
                },
                new ProjectileTag { },
                new NewEntityTag { }
                );
            return bullet;
        }
    }
}
