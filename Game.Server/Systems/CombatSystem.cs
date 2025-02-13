using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using CommunityToolkit.HighPerformance.Buffers;
using Game.Common;
using Game.Common.Enums;
using Game.Common.Packets;
using Game.Common.Random;
using Game.Packets;
using Game.Server.Components;
using Game.Server.Entities;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using System;
using System.Numerics;

namespace Game.Server.Systems
{
    public class CombatSystem : SystemBase
    {
        private ILogger<CombatSystem> _logger;
        public CombatSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<CombatSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
            PacketDispatcher.Subscribe<ActionRequestPacket>(HandleActionRequest);
            PacketDispatcher.Subscribe<ChangeSelectedHotbarIndexRequestPacket>(HandleChangeSelectedHotbarIndexRequest);
        }

        public override void Initialize()
        {
            //Create 50 orbs
            for (int i = 0; i < 50; i++)
            {
                var healAmount = RandomHelper.RandomInt(10, 100);
                var position = new Vector2(RandomHelper.RandomFloat(-100f, 100f), RandomHelper.RandomFloat(-100f, 100f));
                OrbFactory.CreateHealing(World.World, healAmount, position);
            }
        }
        public override void Update(float deltaTime)
        {
            UpdateWeaponCooldowns(deltaTime);
            Attack();
            CheckForDeath();
        }


        private QueryDescription _recieveActionRequestQuery = new QueryDescription().WithAll<NetworkConnectionComponent>();
        public void HandleActionRequest(NetPeer peer, ActionRequestPacket packet)
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _recieveActionRequestQuery, (Entity entity, ref NetworkConnectionComponent ncc) =>
            {
                if (peer.Id == ncc.Peer.Id)
                {
                    _logger.LogTrace($"Entity {entity.Id} wants to attack in the direction {packet.CastDirection}");
                    entity.Add(new AttackRequestComponent()
                    {
                        MouseDirection = packet.CastDirection,
                    });
                }
            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }

        private QueryDescription _recieveChangeSelectedHotbarIndexRequestQuery = new QueryDescription().WithAll<NetworkConnectionComponent, HotbarComponent>();
        private void HandleChangeSelectedHotbarIndexRequest(NetPeer peer, ChangeSelectedHotbarIndexRequestPacket packet)
        {
            World.World.Query(in _recieveChangeSelectedHotbarIndexRequestQuery, (Entity entity, ref NetworkConnectionComponent ncc, ref HotbarComponent hc) =>
            {
                if (peer.Id == ncc.Peer.Id)
                {
                    //Only update selected index if requested index is within the array
                    if (packet.Index < 0 || packet.Index > hc.Hotbar.Count - 1)
                        return;

                    hc.SelectedIndex = packet.Index;
                }
            });
        }

        private QueryDescription _cooldownQuery = new QueryDescription().WithAll<WeaponTag, CooldownComponent>();
        private void UpdateWeaponCooldowns(float deltaTime)
        {
            World.World.Query(in _cooldownQuery, (Entity entity, ref CooldownComponent cc) =>
            {
                cc.TimeLeft -= deltaTime;
            });
        }

        private QueryDescription _attackQuery = new QueryDescription().WithAll<HotbarComponent, AttackRequestComponent>();
        private void Attack()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _attackQuery, (Entity entity, ref HotbarComponent hc, ref AttackRequestComponent arc) =>
            {
                var selectedItem = hc.Hotbar[hc.SelectedIndex].Entity;
                if (selectedItem.TryGet<CooldownComponent>(out var cooldownComponent))
                {
                    if (cooldownComponent.TimeLeft > 0)
                    {
                        _logger.LogTrace($"Weapon on cooldown. Time left: {cooldownComponent.TimeLeft}");
                    }
                    else
                    {
                        _logger.LogTrace($"Weapon NOT on cooldown. Time left: {cooldownComponent.TimeLeft}");
                        cooldownComponent.TimeLeft = cooldownComponent.Duration;
                        selectedItem.Set(cooldownComponent);

                        //SOMEHOW NOTE THAT THIS PLAYER IS ATTACKING
                        var attackedPacket = new EntityAttackedPacket()
                        {
                            EntityID = entity.Id,
                            AttackDirection = arc.MouseDirection,
                        };
                        PacketDispatcher.Enqueue(attackedPacket);

                        var bullet = ProjectileFactory.CreateRifleBullet(World.World, ref entity, ref selectedItem, arc.MouseDirection);

                    }
                    buffer.Remove<AttackRequestComponent>(entity);
                }
            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }

        private QueryDescription _deathQuery = new QueryDescription().WithAll<HealthComponent>();
        private void CheckForDeath()
        {
            var buffer = new CommandBuffer();

            World.World.Query(in _deathQuery, (Entity entity, ref HealthComponent hc) =>
            {
                if (hc.CurrentValue <= 0)
                {
                    _logger.LogTrace($"Entity {entity.Id} has died. Respawning...");

                    if (entity.TryGet<EntityTypeComponent>(out var entityType))
                    {
                        //If the entity is a player, respawn them
                        if (entityType.Type == EntityType.Player)
                        {
                            buffer.Set(entity, new PositionComponent { Value = new Vector2(RandomHelper.RandomInt(-100, 100), RandomHelper.RandomInt(-100, 100)) });
                            buffer.Set(entity, new VelocityComponent { Value = new Vector2(0, 0) });
                            buffer.Set(entity, new HealthComponent { CurrentValue = 100, MaxValue = 100 });
                            buffer.Set(entity, new ManaComponent { CurrentValue = 100, MaxValue = 100 });
                            buffer.Add<PositionDiryTag>(entity);
                        }
                    }

                }
            });
            buffer.Playback(World.World);
        }

    }
}
