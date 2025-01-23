//using Arch.Core;
//using Game.Common.Enums;
//using System.Numerics;

//namespace Game.Server.Entities
//{
//    public static class ProjectileFactory
//    {
//        public static Entity CreateFireball(World world, ref Entity castingEntity, Vector2 direction)
//        {
//            if (castingEntity.TryGet<PositionComponent>(out var startingPosition))
//            {
//                var playerEntity = world.Create(
//                    new EntityTypeComponent { Type = EntityType.FireBall },
//                    new CasterComponent { CastingEntity = castingEntity },
//                    new PositionComponent { Value = startingPosition.Value },
//                    new VelocityComponent { Value = direction },
//                    new MovementSpeedComponent { Value = 25f },
//                    new DestroyAfterDistanceComponent { Distance = 10, StartingPosition = startingPosition.Value },
//                    new NewEntityTag { }
//                    );
//                return playerEntity;
//            }

//            return castingEntity;
//        }
//    }
//}
