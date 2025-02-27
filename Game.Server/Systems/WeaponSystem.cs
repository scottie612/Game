using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Game.Common;
using Game.Common.Packets;
using Game.Packets;
using Game.Server.Components;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Game.Server.Systems
{
    public class WeaponSystem : SystemBase
    {
        private readonly ILogger<WeaponSystem> _logger;

        public WeaponSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<WeaponSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
            PacketDispatcher.Subscribe<ActionRequestPacket>(HandleActionRequest);
            PacketDispatcher.Subscribe<ChangeSelectedHotbarIndexRequestPacket>(HandleChangeSelectedHotbarIndexRequest);
        }
        public override void Update(float deltaTime)
        {
            UpdateWeaponCooldowns(deltaTime);
            Attack();
        }

        private QueryDescription _recieveActionRequestQuery = new QueryDescription().WithAll<NetworkConnectionComponent>();
        public void HandleActionRequest(NetPeer peer, ActionRequestPacket packet)
        {
            World.World.Query(in _recieveActionRequestQuery, (Entity entity, ref NetworkConnectionComponent ncc) =>
            {
                if (peer.Id == ncc.Peer.Id)
                {
                    _logger.LogTrace($"Entity {entity.Id} wants to attack in the direction {packet.CastDirection}");

                    World.World.Create(new AttackRequestEventComponent()
                    {
                        CastingEntity = entity.Reference(),
                        MouseDirection = packet.CastDirection,
                    });
                }
            });
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

        private QueryDescription _attackQuery = new QueryDescription().WithAll<AttackRequestEventComponent>();
        private void Attack()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _attackQuery, (Entity entity, ref AttackRequestEventComponent arc) =>
            {
                if (!arc.CastingEntity.Entity.IsAlive())
                {
                    _logger.LogError($"Entity Reference Invalid and cannot attack");
                    buffer.Add<DeleteEntityTag>(entity);
                    return;
                }

                if (!arc.CastingEntity.Entity.Has<HotbarComponent>())
                {
                    _logger.LogError($"Entity Reference does not have a hotbar and cannot attack");
                    buffer.Add<DeleteEntityTag>(entity);
                    return;
                }
                
                var hotbarComponent = arc.CastingEntity.Entity.Get<HotbarComponent>();
                var selectedItem = hotbarComponent.Hotbar[hotbarComponent.SelectedIndex].Entity;

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
                            EntityID = arc.CastingEntity.Entity.Id,
                            AttackDirection = arc.MouseDirection,
                        };
                        PacketDispatcher.Enqueue(attackedPacket);

                        selectedItem.Get<OnAttackComponent>().OnAttack?.Invoke(arc.CastingEntity, arc.MouseDirection);

                    }

                    buffer.Add<DeleteEntityTag>(entity);
                }
            });
            buffer.Playback(World.World);
        }
    }
}
