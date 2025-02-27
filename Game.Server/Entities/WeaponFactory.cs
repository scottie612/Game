using Arch.Core;
using Game.Server.Components;
using Game.Common.Extentions;

namespace Game.Server.Entities
{
    public static class WeaponFactory
    {
        public static Entity CreateRifle(World world)
        {
            var rifle = world.Create(
                new CooldownComponent { Duration = .5f },
                new OnAttackComponent { OnAttack = (castingEntity, mouseDirection) => 
                    {
                        // spawn bullet
                    }
                },
                new WeaponTag { }
                );

            return rifle;
        }

        public static Entity CreateBow(World world)
        {
            var bow = world.Create(
                new CooldownComponent { Duration = .5f },
                new OnAttackComponent
                {
                    OnAttack = (castingEntity, mouseDirection) =>
                    {
                        // spawn Arrow
                        var arrow = ProjectileFactory.CreateArrow(world, castingEntity, mouseDirection);
                        
                    }
                },
                new WeaponTag { }
                );
            return bow;
        }

        public static Entity CreateShotgun(World world)
        {
            var bow = world.Create(
                new CooldownComponent { Duration = .5f },
                new OnAttackComponent
                {
                    OnAttack = (castingEntity, mouseDirection) =>
                    {
                        
                        // spawn shotgun projectiles in a cone
                        var shell1 = ProjectileFactory.CreateShotgunProjectile(world, castingEntity, mouseDirection.Rotate(15));
                        var shell2 = ProjectileFactory.CreateShotgunProjectile(world, castingEntity, mouseDirection);
                        var shell3 = ProjectileFactory.CreateShotgunProjectile(world, castingEntity, mouseDirection.Rotate(-15));
                    }
                },
                new WeaponTag { }
                );
            return bow;
        }

        public static Entity CreateSword(World world)
        {
            var bow = world.Create(
                new CooldownComponent { Duration = .5f },
                new OnAttackComponent
                {
                    OnAttack = (castingEntity, mouseDirection) =>
                    {
                        // swing sword
                    }
                },
                new WeaponTag { }
                );
            return bow;
        }
    }

}
