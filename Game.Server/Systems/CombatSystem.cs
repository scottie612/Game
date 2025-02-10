using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
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
using System.Linq;
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

        public override void Update(float deltaTime)
        {
            UpdateWeaponCooldowns(deltaTime);
            Attack();
            DamageOnCollision(deltaTime);
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
                        cooldownComponent.TimeLeft = cooldownComponent.Cooldown;
                        selectedItem.Set(cooldownComponent);

                        //SOMEHOW NOTE THAT THIS PLAYER IS ATTACKING
                        var attackedPacket = new EntityAttackedPacket() 
                        { 
                            EntityID = entity.Id,
                            AttackDirection = arc.MouseDirection,
                        };
                        PacketDispatcher.Enqueue(attackedPacket);

                        var bullet = ProjectileFactory.CreateBullet(World.World, ref entity,ref selectedItem, arc.MouseDirection);

                        
                    }
                    buffer.Remove<AttackRequestComponent>(entity);
                }
            });
            buffer.Playback(World.World);
        }


        private QueryDescription _damageOnCollisionQuery = new QueryDescription().WithAll<PositionComponent, DamageComponent>();
        private QueryDescription _hitboxQuery = new QueryDescription().WithAll<HitboxComponent, PositionComponent, HealthComponent>();
        private void DamageOnCollision(float deltaTime)
        {
            World.World.Query(in _damageOnCollisionQuery, (Entity projectileEntity, ref PositionComponent projectilePosition, ref DamageComponent projectileDamage) =>
            {
                var buffer = new CommandBuffer();
                var projectileDam = projectileDamage.Damage;
                var projectilePos = projectilePosition.Value;


                //If entity has a hitbox, caluclate if any other entities with a hitbox and health component are within the hitbox
                if (projectileEntity.TryGet<HitboxComponent>(out var projectileHitbox))
                {
                    World.World.Query(in _hitboxQuery, (Entity targetEntity, ref HitboxComponent targetHitbox, ref PositionComponent targetPosition, ref HealthComponent targetHealth) =>
                    {
                        if (projectileEntity == targetEntity)
                            return;
                        if (projectileEntity.Get<CasterComponent>().CastingEntity == targetEntity)
                            return;

                        float axMin = projectilePos.X + projectileHitbox.XOffset;
                        float axMax = axMin + projectileHitbox.Width;
                        float ayMin = projectilePos.Y + projectileHitbox.YOffset;
                        float ayMax = ayMin + projectileHitbox.Height;

                        // Calculate the bounding box for the second entity
                        float bxMin = targetPosition.Value.X + targetHitbox.XOffset;
                        float bxMax = bxMin + targetHitbox.Width;
                        float byMin = targetPosition.Value.Y + targetHitbox.YOffset;
                        float byMax = byMin + targetHitbox.Height;

                        var projectileVelocity = projectileEntity.Get<VelocityComponent>().Value;
                        var targetVelocity = targetEntity.Get<VelocityComponent>().Value;

                        var relativeVelocity = projectileVelocity - targetVelocity;

                        float txMin = (relativeVelocity.X > 0) ? (bxMin - axMax) / relativeVelocity.X : (bxMax - axMin) / relativeVelocity.X;
                        float txMax = (relativeVelocity.X > 0) ? (bxMax - axMin) / relativeVelocity.X : (bxMin - axMax) / relativeVelocity.X;

                        // Compute entry/exit times along Y axis
                        float tyMin = (relativeVelocity.Y > 0) ? (byMin - ayMax) / relativeVelocity.Y : (byMax - ayMin) / relativeVelocity.Y;
                        float tyMax = (relativeVelocity.Y > 0) ? (byMax - ayMin) / relativeVelocity.Y : (byMin - ayMax) / relativeVelocity.Y;

                        // Find the earliest entry and latest exit time
                        float tEnter = Math.Max(txMin, tyMin);
                        float tExit = Math.Min(txMax, tyMax);

                        // If tEnter > tExit, no collision; also limit check to this frame (deltaTime)
                        if (tEnter > tExit || tEnter < 0 || tEnter > deltaTime)
                            return;

                        targetHealth.CurrentValue -= projectileDam;
                        buffer.Add<HealthDirtyTag>(targetEntity);
                        buffer.Add<DeleteEntityTag>(projectileEntity);
                        _logger.LogTrace($"Entity {targetEntity.Id} hit by projectile {projectileEntity.Id}. Health: {targetHealth.CurrentValue}");


                    });
                }
                else
                {  //if not, caluclate if it's positon is within any other entities with a hitbox and health component.
                    World.World.Query(in _hitboxQuery, (Entity targetEntity, ref HitboxComponent targetHitbox, ref PositionComponent targetPosition, ref HealthComponent targetHealth) =>
                    {
                        if (projectileEntity == targetEntity)
                            return;
                        if (projectileEntity.Get<CasterComponent>().CastingEntity == targetEntity)
                            return;

                        // Calculate the bounding box for the target entity
                        float bxMin = targetPosition.Value.X + targetHitbox.XOffset;
                        float bxMax = bxMin + targetHitbox.Width;
                        float byMin = targetPosition.Value.Y + targetHitbox.YOffset;
                        float byMax = byMin + targetHitbox.Height;

                        var projectileVelocity = projectileEntity.Get<VelocityComponent>().Value;
                        var targetVelocity = targetEntity.Get<VelocityComponent>().Value;

                        var relativeVelocity = projectileVelocity - targetVelocity;

                        // Compute entry/exit times along X axis
                        float txMin = (relativeVelocity.X > 0) ? (bxMin - projectilePos.X) / relativeVelocity.X
                                                               : (bxMax - projectilePos.X) / relativeVelocity.X;
                        float txMax = (relativeVelocity.X > 0) ? (bxMax - projectilePos.X) / relativeVelocity.X
                                                               : (bxMin - projectilePos.X) / relativeVelocity.X;

                        // Compute entry/exit times along Y axis
                        float tyMin = (relativeVelocity.Y > 0) ? (byMin - projectilePos.Y) / relativeVelocity.Y
                                                               : (byMax - projectilePos.Y) / relativeVelocity.Y;
                        float tyMax = (relativeVelocity.Y > 0) ? (byMax - projectilePos.Y) / relativeVelocity.Y
                                                               : (byMin - projectilePos.Y) / relativeVelocity.Y;

                        // Find the earliest entry and latest exit time
                        float tEnter = Math.Max(txMin, tyMin);
                        float tExit = Math.Min(txMax, tyMax);

                        // If tEnter > tExit, no collision; also limit check to this frame (deltaTime)
                        if (tEnter > tExit || tEnter < 0 || tEnter > deltaTime)
                            return;

                        targetHealth.CurrentValue -= projectileDam;
                        buffer.Add<HealthDirtyTag>(targetEntity);
                        buffer.Add<DeleteEntityTag>(projectileEntity);
                        _logger.LogTrace($"Entity {targetEntity.Id} hit by projectile {projectileEntity.Id}. Health: {targetHealth.CurrentValue}");


                    });
                }
                buffer.Playback(World.World);

            });
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
                            buffer.Set(entity, new PlayerInputComponent { MovemenetVector = new Vector2(0, 0), Fire = false, MousePosition = new Vector2(0, 0) });
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
