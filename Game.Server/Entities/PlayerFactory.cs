using Arch.Core;
using Game.Common.Enums;
using Game.Server.Components;
using LiteNetLib;
using System;
using System.Numerics;

namespace Game.Server.Entities
{
    public static class PlayerFactory
    {
        public static Entity CreatePlayer(World world, NetPeer peer, string username)
        {
            var playerEntity = world.Create(
                new NetworkConnectionComponent { Peer = peer },
                new EntityTypeComponent { Type = EntityType.Player },
                new NameComponent { Name = username },
                new PositionComponent { Value = new Vector2(0, 0) },
                new HitboxComponent { Width = 1f, Height = 1f },
                new PlayerInputComponent { InputVector = new Vector2(0, 0) },
                new VelocityComponent { Value = new Vector2(0, 0) },
                new MovementSpeedComponent { Value = 15f },
                new HealthComponent { MaxValue = 100, CurrentValue = 100 },
                new ManaComponent { MaxValue = 100, CurrentValue = 100 },
                new NewEntityTag { },
                new HealthDirtyTag { },
                new ManaDirtyTag { }
                );

            return playerEntity;
        }
    }
}
