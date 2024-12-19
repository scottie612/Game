//using Arch.Core;
//using Arch.System;
//using System;
//using Game.Extentions;
//using System.Numerics;
//using Game.EntityComponentSystem;

//namespace Game.EntityComponentSystem
//{
//    public class UpdateVelocityOnCollisionSystem : BaseSystem<World, float>
//    {
//        public UpdateVelocityOnCollisionSystem(World world) : base(world)
//        {
//        }

//        private QueryDescription _query = new QueryDescription().WithAll<VelocityComponent, PositionComponent, CircleColliderComponent>();

//        public override void Update(in float deltaTime)
//        {
//            World.Query(in _query, (Entity entity, ref VelocityComponent vel, ref PositionComponent pos, ref CircleColliderComponent col) =>
//            {

//                if (vel.Value.Length() > 0)
//                {
//                    foreach (var collision in col.Collisions)
//                    {

//                        var velocityAngle = vel.Value.Degrees();

//                        var collisionVector = new Vector2(collision.X - pos.Value.X, collision.Y - pos.Value.Y);
//                        var collisionAngle = collisionVector.Degrees();
//                        var veloctyWithRespectToCollision = ((velocityAngle - collisionAngle) + 360f) % 360f;

//                        //Console.WriteLine($"Collision Angle {collisionAngle} Degrees");

//                        float resultingAngle = 0;
//                        if (MathF.Abs(velocityAngle - collisionAngle) < 10)
//                        {
//                            vel.Value = Vector2.Zero;
//                        }
//                        else if (veloctyWithRespectToCollision < 90)
//                        {
//                            resultingAngle = collisionAngle + 90;
//                            vel.Value.X = MathF.Cos(resultingAngle.ToRadians());
//                            vel.Value.Y = MathF.Sin(resultingAngle.ToRadians());
//                        }
//                        else if (veloctyWithRespectToCollision > 270)
//                        {
//                            resultingAngle = collisionAngle - 90;
//                            vel.Value.X = MathF.Cos(resultingAngle.ToRadians());
//                            vel.Value.Y = MathF.Sin(resultingAngle.ToRadians());
//                        }
//                        else
//                        {
//                            resultingAngle = velocityAngle;
//                            vel.Value.X = MathF.Cos(resultingAngle.ToRadians());
//                            vel.Value.Y = MathF.Sin(resultingAngle.ToRadians());
//                        }
//                    }
//                    col.Collisions.Clear();
//                }
//            });
//        }

//    }
//}
