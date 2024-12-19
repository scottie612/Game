using Arch.Core;
using Arch.Core.Extensions;
using Game.Configuration;
using Game.EntityComponentSystem.Components;
using LiteNetLib;
using System;
using System.Numerics;

namespace Game.EntityComponentSystem
{
    public static class EntityFactory
    {
        public static Entity CreatePlayer(World world, NetPeer peer)
        {
            var playerEntity = world.Create(
                new NetworkConnectionComponent { Peer = peer },
                new EntityTypeComponent { Type = EntityType.Player },
                new PositionComponent { Value = new Vector2(0, 0) },
                new HitboxComponent { Width = 1f, Height = 1f },
                new PlayerInputComponent { InputVector = new Vector2(0, 0) },
                new VelocityComponent { Value = new Vector2(0, 0) },
                new MovementSpeedComponent { Value = 15f },
                new HealthComponent { MaxValue = 100, CurrentValue = 100 },
                new ManaComponent { MaxValue = 100, CurrentValue = 100 },
                new PhysicalDefenceComponent { Value = 25 },
                new MagicDefenceComponent { Value = 25 },
                new NewEntityTag { }
                );


            return playerEntity;
        }

        public static Entity CreateTestPlayer(World world)
        {
            var test = new Random();
            
            var playerEntity = world.Create(
                new EntityTypeComponent { Type = EntityType.Player },
                new PositionComponent { Value = new Vector2(0, 0) },
                new HitboxComponent { Width = 1f, Height = 1f },
                new PlayerInputComponent { InputVector = new Vector2(test.NextSingle() * 2f - 1f, test.NextSingle() * 2f - 1f) },
                new VelocityComponent { Value = new Vector2(0, 0) },
                new MovementSpeedComponent { Value = 3f },
                new HealthComponent { MaxValue = 100, CurrentValue = 100 },
                new ManaComponent { MaxValue = 100, CurrentValue = 100 },
                new PhysicalDefenceComponent { Value = 25 },
                new MagicDefenceComponent { Value = 25 },
                new NewEntityTag { }
                );


            return playerEntity;
        }

        public static Entity CreateFireball(World world, ref Entity castingEntity, Vector2 direction)
        {
            if (castingEntity.TryGet<PositionComponent>(out var startingPosition))
            {
                var playerEntity = world.Create(
                    new EntityTypeComponent { Type = EntityType.FireBall },
                    new CasterComponent { CastingEntity = castingEntity },
                    new PositionComponent { Value = startingPosition.Value },
                    new VelocityComponent { Value = direction },
                    new MovementSpeedComponent { Value = 25f },
                    new DestroyAfterDistanceComponent { Distance = 10, StartingPosition = startingPosition.Value },
                    new NewEntityTag { }
                    );
                return playerEntity;
            }

            return castingEntity;

        }

    }
}
