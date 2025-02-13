using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Game.Common;
using Game.Server.Components;
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
        private Dictionary<int, CollisionEvent> _bufferedCollisionEvents = new Dictionary<int, CollisionEvent>();

        private ILogger<CollisionSystem> _logger;
        public CollisionSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<CollisionSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
        }

        public override void Update(float deltaTime)
        {
            ProcessCollisions();
            DetectCollisions();
            UpdateCollisionState();
            StartNewCollisions();
        }

        private void ProcessCollisions()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _collisionEventQuery, (Entity entity, ref CollisionEvent collisionEvent, ref CollisionData collisionData) =>
            {
                var entity1 = collisionEvent.EntityA;
                var entity2 = collisionEvent.EntityB;
                if (entity1.IsAlive() && entity2.IsAlive())
                {
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
                            buffer.Add<DeleteEntityTag>(entity);
                            break;
                    }
                }
                else
                {
                    collisionData.State = CollisionState.None;
                }
            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }

        private void DetectCollisions()
        {
            _colliders.Clear();
            _bufferedCollisionEvents.Clear();

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
                            _bufferedCollisionEvents[hash] = collisionEvent;
                        }
                    }
                }

                _colliders.Add((entity.Reference(), collider, position));
            });
        }


        private void UpdateCollisionState()
        {
            // Create a set of processed hashes to track which collisions we've handled
            var processedHashes = new HashSet<int>();

            World.World.Query(in _collisionEventQuery, (Entity entity, ref CollisionEvent collisionEvent, ref CollisionData collisionData) =>
            {
                var entity1 = collisionEvent.EntityA;
                var entity2 = collisionEvent.EntityB;

                
                if (entity1.IsAlive() && entity2.IsAlive() && collisionData.State != CollisionState.None)
                {
                    if (_bufferedCollisionEvents.ContainsKey(collisionData.Hash))
                    {
                        // Still colliding
                        if (collisionData.State == CollisionState.Starting || collisionData.State == CollisionState.Continuing)
                        {
                            collisionData.State = CollisionState.Continuing;
                        }

                        // Update active collisions sets
                        entity1.Entity.Get<ColliderComponent>().ActiveCollisions.Add(entity2);
                        entity2.Entity.Get<ColliderComponent>().ActiveCollisions.Add(entity1);

                        processedHashes.Add(collisionData.Hash);
                        _logger.LogTrace($"Collision Continuing... EntityA ID: {entity1.Entity.Id}, EntityB ID: {entity2.Entity.Id}");
                    }
                    else if (collisionData.State != CollisionState.Exiting)
                    {
                        // No longer colliding
                        _logger.LogTrace($"Collision Exiting... EntityA ID: {entity1.Entity.Id}, EntityB ID: {entity2.Entity.Id}");
                        collisionData.State = CollisionState.Exiting;

                        // Clear from active collisions
                        entity1.Entity.Get<ColliderComponent>().ActiveCollisions.Remove(entity2);
                        entity2.Entity.Get<ColliderComponent>().ActiveCollisions.Remove(entity1);

                    }
                }
            });

            // Remove processed collisions from buffer
            foreach (var hash in processedHashes)
            {
                _bufferedCollisionEvents.Remove(hash);
            }
        }

        private void StartNewCollisions()
        {
            foreach (var ent in _bufferedCollisionEvents)
            {
                var e1 = ent.Value.EntityA;
                var e2 = ent.Value.EntityB;

                e1.Entity.Get<ColliderComponent>().ActiveCollisions.Add(e2);
                e2.Entity.Get<ColliderComponent>().ActiveCollisions.Add(e1);
                _logger.LogTrace($"Collision Starting... EntityA ID: {e1.Entity.Id}, EntityB ID: {e2.Entity.Id}");
                World.World.Create(new CollisionData { Hash = ent.Key, State = CollisionState.Starting }, ent.Value);
            }
        }

        private bool IsColliding(ColliderComponent colliderA, PositionComponent positionA, ColliderComponent colliderB, PositionComponent positionB)
        {
            // Get center positions of both circles
            Vector2 centerA = positionA.Value + colliderA.Offset;
            Vector2 centerB = positionB.Value + colliderB.Offset;

            // Check if circles overlap using distance check
            var distanceSquared = Vector2.DistanceSquared(centerA, centerB);
            var combinedRadius = colliderA.Radius + colliderB.Radius;

            if (distanceSquared <= combinedRadius * combinedRadius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public enum CollisionState
    {
        Starting,
        Continuing,
        Exiting,
        None,
    }

    public struct CollisionEvent
    {
        public EntityReference EntityA;
        public EntityReference EntityB;
    }

    public struct CollisionData
    {
        public int Hash;
        public CollisionState State;
    }

    public struct ColliderComponent
    {
        public ColliderComponent()
        {
            ActiveCollisions = new HashSet<EntityReference>();
        }

        public float Radius;
        public Vector2 Offset;
        public HashSet<EntityReference> ActiveCollisions;
        public Action<Entity, Entity>? OnStart;
        public Action<Entity, Entity>? OnContinue;
        public Action<Entity, Entity>? OnExit;
    }
}
