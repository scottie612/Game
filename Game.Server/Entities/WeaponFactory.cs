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
                new CooldownComponent { Cooldown = 1f },
                new DamageComponent { Damage = 10, DamageType = DamageType.Physical },
                new RangeComponent { Value = 20 },
                new MovementSpeedComponent { Value = 25f },
                new WeaponTag { }
                );

            return rifle;
        }
    }
}
