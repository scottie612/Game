using Arch.Core;
using Game.Common.Enums;
using Game.Server.Components;
using System.Numerics;

namespace Game.Server.Entities
{
    public static class OrbFactory
    {

        public static Entity CreateHealing(World world, int healAmount, Vector2 position)
        {
            var healingOrb = world.Create(
                new EntityTypeComponent { Type = EntityType.HealingOrb },
                new HitboxComponent { Radius = 2f },
                new PositionComponent { Value = position },
                new HealComponent { Value = healAmount },
                new NewEntityTag() { },
                new OrbTag { }
            );

            return healingOrb;
        }

    }
}
