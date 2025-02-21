using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Game.Common;
using Game.Common.Enums;
using Game.Server.Components;
using Game.Server.Components.Collisions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Game.Server.Systems
{
    public class CollisionSystem : SystemBase
    {
        private QueryDescription _collisionEventQuery = new QueryDescription().WithAll<CollisionEvent, CollisionData>();
        private QueryDescription _detectCollisionsQuery = new QueryDescription().WithAll<ColliderComponent, PositionComponent>();

        private List<(EntityReference e, ColliderComponent c, PositionComponent p)> _colliders = new List<(EntityReference, ColliderComponent, PositionComponent)>();
        private Dictionary<int, CollisionEvent> _currentCollisions = new Dictionary<int, CollisionEvent>();

        private ILogger<CollisionSystem> _logger;
        public CollisionSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<CollisionSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
        }

        public override void Update(float deltaTime)
        {
            DetectCollisions();
            UpdateCollisionState();
            ProcessCollisions();
        }

        private void DetectCollisions()
        {
            _colliders.Clear();
            _currentCollisions.Clear();

            //Loop through all colliders and added the collision event to current collisions
            World.World.Query(in _detectCollisionsQuery, (Entity entity, ref ColliderComponent collider, ref PositionComponent position) =>
            {
                for (int i = 0; i < _colliders.Count; i++)
                {
                    if (entity.Id != _colliders[i].e.Entity.Id)
                    {
                        if (IsColliding(collider, position, _colliders[i].c, _colliders[i].p))
                        {
                            var entity1 = entity;
                            var entity2 = _colliders[i].e.Entity;

                            var hash = entity1.Id > entity2.Id ? HashCode.Combine(entity2.Id, entity1.Id) : HashCode.Combine(entity1.Id, entity2.Id);
                            var collisionEvent = new CollisionEvent() { EntityA = entity1.Reference(), EntityB = entity2.Reference() };
                            _currentCollisions[hash] = collisionEvent;
                        }
                    }
                }

                _colliders.Add((entity.Reference(), collider, position));
            });
        }


        private void UpdateCollisionState()
        {
            var buffer = new CommandBuffer();

            //Loop through all existing collision events and update their state
            World.World.Query(in _collisionEventQuery, (Entity entity, ref CollisionEvent collisionEvent, ref CollisionData collisionData) =>
            {
                var entity1 = collisionEvent.EntityA;
                var entity2 = collisionEvent.EntityB;

                // collision exists
                if (_currentCollisions.ContainsKey(collisionData.Hash))
                {
                    entity1.Entity.Get<ColliderComponent>().ActiveCollisions.Add(entity2);
                    entity2.Entity.Get<ColliderComponent>().ActiveCollisions.Add(entity1);
                    //If it was previously starting, set it to contunuing
                    if (collisionData.State == CollisionState.Starting)
                    {
                        //_logger.LogTrace($"Collision Continuing... EntityA ID: {entity1.Entity.Id}, EntityB ID: {entity2.Entity.Id}");
                        collisionData.State = CollisionState.Continuing;
                    }
                    //If it was previously continuing, then it is still continuing.
                    else if (collisionData.State == CollisionState.Continuing)
                    {
                        //_logger.LogTrace($"Collision Continuing... EntityA ID: {entity1.Entity.Id}, EntityB ID: {entity2.Entity.Id}");
                        collisionData.State = CollisionState.Continuing;
                    }

                }
                // collision no longer exists
                else
                {
                    //_logger.LogTrace($"Collision Exiting... EntityA ID: {entity1.Entity.Id}, EntityB ID: {entity2.Entity.Id}");
                    collisionData.State = CollisionState.Exiting;
                }
                //removed the processed collisions from _currentCollisions
                _currentCollisions.Remove(collisionData.Hash);
            });

            // the collisions left over in _currentCollisions are new

            foreach (var collision in _currentCollisions)
            {
                var e1 = collision.Value.EntityA;
                var e2 = collision.Value.EntityB;

               //_logger.LogTrace($"Collision Starting... EntityA ID: {e1.Entity.Id}, EntityB ID: {e2.Entity.Id}");
                World.World.Create(new CollisionData { Hash = collision.Key, State = CollisionState.Starting }, collision.Value);

                e1.Entity.Get<ColliderComponent>().ActiveCollisions.Add(e2);
                e2.Entity.Get<ColliderComponent>().ActiveCollisions.Add(e1);

            }
            buffer.Playback(World.World);
        }

        private void ProcessCollisions()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _collisionEventQuery, (Entity entity, ref CollisionEvent collisionEvent, ref CollisionData collisionData) =>
            {
                var entity1 = collisionEvent.EntityA;
                var entity2 = collisionEvent.EntityB;

                if (!entity1.IsAlive() || !entity2.IsAlive())
                {
                    //_logger.LogTrace($"Skipping collision callbacks - entity no longer alive. EntityA: {entity1.Entity.Id}, EntityB: {entity2.Entity.Id}");

                    // Clean up ActiveCollisions
                    if (entity1.IsAlive() && entity1.Entity.Has<ColliderComponent>())
                        entity1.Entity.Get<ColliderComponent>().ActiveCollisions.Remove(entity2);
                    if (entity2.IsAlive() && entity2.Entity.Has<ColliderComponent>())
                        entity2.Entity.Get<ColliderComponent>().ActiveCollisions.Remove(entity1);

                    //Delete Event
                    buffer.Add<DeleteEntityTag>(entity);
                    return;
                }

                switch (collisionData.State)
                {
                    case CollisionState.Starting:
                        entity1.Entity.Get<ColliderComponent>().OnStart?.Invoke(entity1.Entity, entity2.Entity);
                        entity2.Entity.Get<ColliderComponent>().OnStart?.Invoke(entity2.Entity, entity1.Entity);
                        break;
                    case CollisionState.Continuing:
                        entity1.Entity.Get<ColliderComponent>().OnContinue?.Invoke(entity1.Entity, entity2.Entity);
                        entity2.Entity.Get<ColliderComponent>().OnContinue?.Invoke(entity2.Entity, entity1.Entity);
                        break;
                    case CollisionState.Exiting:
                        entity1.Entity.Get<ColliderComponent>().OnExit?.Invoke(entity1.Entity, entity2.Entity);
                        entity2.Entity.Get<ColliderComponent>().OnExit?.Invoke(entity2.Entity, entity1.Entity);
                        entity1.Entity.Get<ColliderComponent>().ActiveCollisions.Remove(entity2);
                        entity2.Entity.Get<ColliderComponent>().ActiveCollisions.Remove(entity1);

                        buffer.Add<DeleteEntityTag>(entity);
                        break;
                }
            });
            buffer.Playback(World.World);
        }

        private bool IsColliding(ColliderComponent colliderA, PositionComponent positionA, ColliderComponent colliderB, PositionComponent positionB)
        {
            // Get center positions of both circles
            Vector2 centerA = positionA.Value + colliderA.Offset;
            Vector2 centerB = positionB.Value + colliderB.Offset;

            switch (colliderA.Shape.Type)
            {
                case ShapeType.Circle:
                    switch (colliderB.Shape.Type)
                    {
                        case ShapeType.Circle:
                            return CheckCircleCircleCollision(centerA, colliderA.Shape.Radius, centerB, colliderB.Shape.Radius);
                        case ShapeType.Box:
                            return CheckCircleBoxCollision(centerA, colliderA.Shape.Radius, centerB, colliderB.Shape.Size);
                    }
                    break;
                case ShapeType.Box:
                    switch (colliderB.Shape.Type)
                    {
                        case ShapeType.Circle:
                            return CheckCircleBoxCollision(centerB, colliderB.Shape.Radius, centerA, colliderA.Shape.Size);
                        case ShapeType.Box:
                            return CheckBoxBoxCollision(centerA, colliderA.Shape.Size, centerB, colliderB.Shape.Size);
                    }
                    break;
            }
            return false;
        }

        private bool CheckCircleCircleCollision(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            var distanceSquared = Vector2.DistanceSquared(centerA, centerB);
            var combinedRadius = radiusA + radiusB;

            return distanceSquared <= combinedRadius * combinedRadius;
        }

        private bool CheckBoxBoxCollision(Vector2 centerA, Vector2 sizeA, Vector2 centerB, Vector2 sizeB)
        {
            // Calculate half-sizes for both boxes
            Vector2 halfSizeA = sizeA * 0.5f;
            Vector2 halfSizeB = sizeB * 0.5f;

            // Calculate min/max points for both boxes
            Vector2 minA = centerA - halfSizeA;
            Vector2 maxA = centerA + halfSizeA;
            Vector2 minB = centerB - halfSizeB;
            Vector2 maxB = centerB + halfSizeB;

            // Check for overlap on both axes (AABB collision)
            return (minA.X <= maxB.X && maxA.X >= minB.X) &&
                   (minA.Y <= maxB.Y && maxA.Y >= minB.Y);
        }

        private bool CheckCircleBoxCollision(Vector2 circleCenter, float radius, Vector2 boxCenter, Vector2 boxSize)
        {
            // Calculate half-size of box
            Vector2 halfSize = boxSize * 0.5f;

            // Calculate box min/max points
            Vector2 boxMin = boxCenter - halfSize;
            Vector2 boxMax = boxCenter + halfSize;

            // Find the closest point on the box to the circle center
            float closestX = MathF.Max(boxMin.X, MathF.Min(circleCenter.X, boxMax.X));
            float closestY = MathF.Max(boxMin.Y, MathF.Min(circleCenter.Y, boxMax.Y));
            Vector2 closestPoint = new Vector2(closestX, closestY);

            // Calculate distance from closest point to circle center
            float distanceSquared = Vector2.DistanceSquared(circleCenter, closestPoint);

            // Check if distance is less than or equal to radius
            return distanceSquared <= radius * radius;
        }
    }
}
