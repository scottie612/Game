using Arch.Core;
using Arch.Core.Extensions;
using Game.Common.Enums;
using Game.Server.Components;
using System.Numerics;

namespace Game.Server.Entities
{
    public static class ProjectileFactory
    {

        public static Entity CreateBullet(World world, ref Entity castingEntity, ref Entity weapon, Vector2 direction)
        {
            if (castingEntity.TryGet<PositionComponent>(out var startingPosition))
            {
                var playerEntity = world.Create(
                    new EntityTypeComponent { Type = EntityType.Bullet },
                    new CasterComponent { CastingEntity = castingEntity },
                    new PositionComponent { Value = startingPosition.Value },
                    new VelocityComponent { Value = direction },
                    new MovementSpeedComponent { Value = weapon.Get<MovementSpeedComponent>().Value },
                    new DamageComponent { Damage = weapon.Get<DamageComponent>().Damage, DamageType = weapon.Get<DamageComponent>().DamageType },
                    new HitboxComponent { Width = 0.5f, Height = 0.5f },
                    new DestroyAfterDistanceComponent { Distance = weapon.Get<RangeComponent>().Value, StartingPosition = startingPosition.Value },
                    new ProjectileTag { },
                    new NewEntityTag { }
                    );
                return playerEntity;
            }

            return castingEntity;
        }

    }
}
