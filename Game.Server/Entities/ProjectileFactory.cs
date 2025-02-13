using Arch.Core;
using Arch.Core.Extensions;
using Game.Common.Enums;
using Game.Server.Components;
using Game.Server.Systems;
using System.Numerics;

namespace Game.Server.Entities
{
    public static class ProjectileFactory
    {

        public static Entity CreateRifleBullet(World world, ref Entity castingEntity, ref Entity weapon, Vector2 direction)
        {
            var startingPosition = castingEntity.Get<PositionComponent>().Value;

            var bullet = world.Create(
                new EntityTypeComponent { Type = EntityType.Bullet },
                new CasterComponent { CastingEntity = castingEntity },
                new PositionComponent { Value = startingPosition },
                new VelocityComponent { Value = direction },
                new MovementSpeedComponent { Value = 20f },
                new DamageComponent { Damage = 20, DamageType = DamageType.Physical },
                new RangeComponent { Range = 20, StartingPosition = startingPosition },
                new HitboxComponent {Radius = 0.2f },
                new ColliderComponent { Radius = .2f,
                OnStart = (self, other) =>
                {
                    if (self.Get<CasterComponent>().CastingEntity == other)
                        return;
                    if (other.TryGet<HealthComponent>(out var health))
                    {
                        health.CurrentValue -= 10;
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

        public static Entity CreateArrow(World world, ref Entity castingEntity, ref Entity weapon, Vector2 direction)
        {
            var startingPosition = castingEntity.Get<PositionComponent>().Value;

            var bullet = world.Create(
                new EntityTypeComponent { Type = EntityType.Bullet },
                new CasterComponent { CastingEntity = castingEntity },
                new PositionComponent { Value = startingPosition },
                new VelocityComponent { Value = direction },
                new MovementSpeedComponent { Value = 20f },
                new DamageComponent { Damage = 25, DamageType = DamageType.Physical },
                new RangeComponent { Range = 15, StartingPosition = startingPosition },
                new HitboxComponent {Radius = 0.5f },
                new ProjectileTag { },
                new NewEntityTag { }
                );
            return bullet;
        }
    }
}
