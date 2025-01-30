using Arch.Core;
using Arch.Core.Extensions;
using Game.Common.Enums;
using Game.Server.Components;
using LiteNetLib;
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
                new HitboxComponent { Width = 1f, Height = 2f },
                new PlayerInputComponent { MovemenetVector = new Vector2(0, 0), MousePosition = new Vector2(0 ,0), Fire = false },
                new VelocityComponent { Value = new Vector2(0, 0) },
                new MovementSpeedComponent { Value = 10f },
                new HealthComponent { MaxValue = 100, CurrentValue = 100 },
                new ManaComponent { MaxValue = 100, CurrentValue = 100 },
                new SelectedWeaponComponent { Weapon = WeaponFactory.CreateRifle(world).Reference() },
                new NewEntityTag { }
                );

            return playerEntity;
        }
    }
}
