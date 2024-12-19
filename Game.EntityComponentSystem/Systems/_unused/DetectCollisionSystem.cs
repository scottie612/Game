//using Arch.Core;
//using Arch.System;
//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;

//namespace Game.EntityComponentSystem
//{
//    public partial class DetectCollisionSystem : BaseSystem<World,float>
//    {

//        private List<(Entity e, CircleColliderComponent c, PositionComponent p)> _colliders = new List<(Entity e, CircleColliderComponent c, PositionComponent p)>();

//        private QueryDescription _query = new QueryDescription().WithAll<PositionComponent, CircleColliderComponent>();
//        public DetectCollisionSystem(World world) : base(world)
//        {
//        }

//        public override void Update(in float t)
//        {
//            _colliders.Clear();

//            World.Query(in _query, (Entity entity, ref CircleColliderComponent cirCollider, ref PositionComponent pos) =>
//            {
//                _colliders.Add((entity, cirCollider, pos));
//            });


//            World.Query(in _query, (Entity entity, ref CircleColliderComponent cirCollider, ref PositionComponent pos) =>
//            {
//                cirCollider.Collisions = new List<CollisionPoint>();
                
//                foreach (var collider in _colliders)
//                {
//                    if (collider.e == entity) continue;

//                    if (DetectCircleCircleCollision(collider.p, collider.c, pos, cirCollider, out CollisionPoint collisionPoint))
//                    {
//                        cirCollider.Collisions.Add(collisionPoint);
//                    }
  
//                }
//            });
//        }

//        private bool DetectCircleCircleCollision(PositionComponent aPos, CircleColliderComponent aCol, PositionComponent bPos, CircleColliderComponent bCol, out CollisionPoint point)
//        {
//            var xDelta = (bPos.Value.X + bCol.XOffset) - (aPos.Value.X + aCol.XOffset);
//            var yDelta = (bPos.Value.Y + bCol.YOffset) - (aPos.Value.Y + aCol.YOffset);

//            var distance = MathF.Sqrt((xDelta * xDelta) + (yDelta * yDelta));

//            if(distance < aCol.Radius + bCol.Radius)
//            {
//                // Calculate the collision point (midpoint for simplicity)
//                float collisionX = (aPos.Value.X + aCol.XOffset + bPos.Value.X + bCol.XOffset) / 2;
//                float collisionY = (aPos.Value.Y + aCol.YOffset + bPos.Value.Y + bCol.YOffset) / 2;
//                point = new CollisionPoint { X = collisionX, Y = collisionY };
//                return true;
//            }
//            point = new CollisionPoint { };
//            return false;
         
//        }
//    }
//}
