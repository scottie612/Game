using Arch.Core;
using Arch.Core.Extensions;
using Game.Common.Enums;
using Game.Server.Components;
using Game.Server.Components.Collisions;
using Game.Server.Components.Stats;
using System.Numerics;

namespace Game.Server.Entities
{
    public static class OrbFactory
    {
        public static Entity CreateHealing(World world, int healAmount, Vector2 position)
        {
            var healingOrb = world.Create(
                new EntityTypeComponent { Type = EntityType.HealingOrb },
                new PositionComponent { Value = position },
                new ColliderComponent
                {
                    Shape = Shape.Circle(0.5f),
                    OnStart = ( self, other) =>
                        {
                            if (other.TryGet<HealthComponent>(out var health))
                            {
                                health.CurrentValue += healAmount;
                                if (health.CurrentValue > health.MaxValue)
                                {
                                    health.CurrentValue = health.MaxValue;
                                }
                                other.Set<HealthComponent>(health);
                                other.Add<HealthDirtyTag>();
                                self.Add<DeleteEntityTag>();
                            }
                        }
                },
                new NewEntityTag() { },
                new OrbTag { }
            );

            return healingOrb;
        }

    }
}
