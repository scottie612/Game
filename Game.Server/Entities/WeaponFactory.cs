using Arch.Core;
using Game.Common.Enums;
using Game.Server.Components;

namespace Game.Server.Entities
{
    public static class WeaponFactory
    {
        public static Entity CreateRifle(World world)
        {
            var rifle = world.Create(
                new CooldownComponent { Duration = .5f },
                new WeaponTag { }
                );

            return rifle;
        }

        public static Entity CreateBow(World world)
        {
            var bow = world.Create(
                new CooldownComponent { Duration = .5f },
                new WeaponTag { }
                );
            return bow;
        }

        public static Entity CreateSword(World world)
        {
            var bow = world.Create(
                new CooldownComponent { Duration = .5f },
                new WeaponTag { }
                );
            return bow;
        }
    }

}
