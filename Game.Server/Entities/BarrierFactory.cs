using Arch.Core;
using Arch.Core.Extensions;
using Game.Common.Enums;
using Game.Server.Components;
using Game.Server.Components.Collisions;
using System.Numerics;

namespace Game.Server.Entities
{
    public static class BarrierFactory
    {
        public static Entity CreateBarrier(World world, Vector2 position, Shape shape)
        {
            var barrier = world.Create(
                new PositionComponent { Value = position },
                new ColliderComponent
                {
                    Shape = shape,
                    OnContinue = (self, other) =>
                    {
                        if (other.Entity.TryGet<EntityTypeComponent>(out var entityType))
                        {
                            if (entityType.Type == EntityType.Player)
                            {
                                var playerPosition = other.Entity.Get<PositionComponent>().Value;
                                var velocityComponent = other.Entity.Get<VelocityComponent>();
                                var vel = velocityComponent.Value;

                                // Get the shape and position of the barrier
                                var collider = other.Entity.Get<ColliderComponent>();
                                var barrierPosition = other.Entity.Get<PositionComponent>().Value;
                                var closestPoint = collider.Shape.ClosestPoint(barrierPosition, playerPosition);

                                // Compute normal vector
                                var normal = playerPosition - closestPoint;
                                if (normal.LengthSquared() > 0)
                                {
                                    normal = Vector2.Normalize(normal);
                                }

                                // Compute the velocity component along the normal
                                float velocityAlongNormal = Vector2.Dot(vel, normal);

                                // If moving into the wall, remove that component
                                if (velocityAlongNormal < 0)
                                {
                                    vel -= velocityAlongNormal * normal;
                                }

                                // Set new velocity
                                other.Entity.Set(new VelocityComponent { Value = vel });

                            }
                        }
                    },
                }
            );

            return barrier;
        }
    }
}
