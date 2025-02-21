using Arch.Core;
using Arch.Core.Extensions;
using Game.Common.Enums;
using Game.Server.Components;
using Game.Server.Components.Collisions;
using Game.Server.Components.Stats;
using LiteNetLib;
using System.Collections.Generic;
using System.Numerics;

namespace Game.Server.Entities
{

    /// <summary>
    /// Creates a player in the world
    /// TODO Update this to be extention method of World and use builder pattern.
    /// </summary>
    public static class PlayerFactory
    {
        public static Entity CreatePlayer(World world, NetPeer peer, string username)
        {
            var weapon = WeaponFactory.CreateBow(world).Reference();

            var playerEntity = world.Create(
                new NetworkConnectionComponent { Peer = peer },
                new EntityTypeComponent { Type = EntityType.Player },
                new NameComponent { Name = username },
                new PositionComponent { Value = new Vector2(0, 0) },
                new VelocityComponent { Value = new Vector2(0, 0) },
                new MovementSpeedComponent { Value = 10f },
                new HealthComponent { MaxValue = 100, CurrentValue = 100 },
                new ManaComponent { MaxValue = 100, CurrentValue = 100 },
                new HotbarComponent { SelectedIndex = 0, Hotbar = new List<EntityReference>() { weapon } },
                new ColliderComponent { Shape = Shape.Box(2f, 1f) },
                new NewEntityTag { }
            );

            return playerEntity;
        }
    }
}
