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
            //Spawn 10 Healing Orbs at random Locations
            for (int i = 0; i < 10; i++)
            {
                var healAmount = RandomHelper.RandomInt(10, 100);
                var position = new Vector2(RandomHelper.RandomFloat(-100f, 100f), RandomHelper.RandomFloat(-100f, 100f));
                OrbFactory.CreateHealing(World.World, healAmount, position);
            }
        }

        public override void Update(float deltaTime)
        {
            EnsureOrbs();
            UpdateWeaponCooldowns(deltaTime);
            Attack();
            HandleCollisions();
            CheckForDeath();
        }

        private QueryDescription _ensureOrbsQuery = new QueryDescription().WithAll<OrbTag>();
        private void EnsureOrbs()
        {
            var count = 0;
            World.World.Query(in _ensureOrbsQuery, (Entity entity) =>
            {
                count++;
            });

            var amountToAdd = 30 - count;
            for (int i = 0; i < amountToAdd; i++)
            {
                var healAmount = RandomHelper.RandomInt(10, 100);
                var position = new Vector2(RandomHelper.RandomFloat(-100f, 100f), RandomHelper.RandomFloat(-100f, 100f));
                OrbFactory.CreateHealing(World.World, healAmount, position);
            }
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

        private QueryDescription _playerHitboxQuery = new QueryDescription().WithAll<HitboxComponent, PositionComponent, HealthComponent>();
        private QueryDescription _damageOnCollisionQuery = new QueryDescription().WithAll<PositionComponent, DamageComponent, HitboxComponent, ProjectileTag>();
        private QueryDescription _healOnCollisionQuery = new QueryDescription().WithAll<PositionComponent, HealComponent, HitboxComponent, OrbTag>();
        private void HandleCollisions()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _playerHitboxQuery, (Entity playerEntity, ref HitboxComponent playerHB, ref PositionComponent playerPosition, ref HealthComponent playerHP) =>
            {
                var playerHealth = playerHP;
                var playerPos = playerPosition;
                var playerHitBox = playerHB;
                World.World.Query(in _damageOnCollisionQuery, (Entity projectileEntity, ref PositionComponent projPos, ref DamageComponent projDamage, ref HitboxComponent projHitBox) =>
                {
                    if (projectileEntity == playerEntity)
                        return;
                    if (projectileEntity.TryGet<CasterComponent>(out var casterComponent))
                    {
                        if (casterComponent.CastingEntity == playerEntity)
                            return;
                    }
                    if (IsColliding(playerPos, projPos, playerHitBox, projHitBox))
                    {
                        playerHealth.CurrentValue -= projDamage.Damage;

                        buffer.Add<HealthDirtyTag>(playerEntity);
                        buffer.Set(playerEntity, playerHealth);
                        buffer.Add<DeleteEntityTag>(projectileEntity);
                        _logger.LogTrace($"Entity {playerEntity.Id} hit by projectile {projectileEntity.Id}. Health: {playerHealth.CurrentValue}");
                    }
                });
                World.World.Query(in _healOnCollisionQuery, (Entity orbEntity, ref PositionComponent orbPos, ref HealComponent orbHeal, ref HitboxComponent orbHitBox) =>
                {
                    if (orbEntity == playerEntity)
                        return;
                    if (orbEntity.TryGet<CasterComponent>(out var casterComponent))
                    {
                        if (casterComponent.CastingEntity == playerEntity)
                            return;
                    }
                    if (IsColliding(playerPos, orbPos, playerHitBox, orbHitBox))
                    {
                        playerHealth.CurrentValue += orbHeal.Value;
                        if(playerHealth.CurrentValue > playerHealth.MaxValue)
                        {
                            playerHealth.CurrentValue = playerHealth.MaxValue;
                        }

                        buffer.Add<HealthDirtyTag>(playerEntity);
                        buffer.Set(playerEntity, playerHealth);
                        buffer.Add<DeleteEntityTag>(orbEntity);
                        _logger.LogTrace($"Entity {playerEntity.Id} healed by orb {orbEntity.Id}. Health: {playerHealth.CurrentValue}");
                    }

                });
            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }

        private bool IsColliding(PositionComponent positionA, PositionComponent positionB, HitboxComponent hitboxA, HitboxComponent hitboxB)
        {
            // Get center positions of both circles
            Vector2 centerA = new Vector2(
                positionA.Value.X + hitboxA.XOffset,
                positionA.Value.Y + hitboxA.YOffset
            );

            Vector2 centerB = new Vector2(
                positionB.Value.X + hitboxB.XOffset,
                positionB.Value.Y + hitboxB.YOffset
            );

            // Check if circles overlap using distance check
            var distanceSquared = Vector2.DistanceSquared(centerA, centerB);
            var combinedRadius = hitboxA.Radius + hitboxB.Radius;

            if (distanceSquared <= combinedRadius * combinedRadius)
            {
                return true;
            }
            else
            {
                return false;
            }
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
