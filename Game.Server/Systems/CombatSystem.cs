using Arch.Core;
using Arch.Core.Extensions;
using Game.Common;
using Game.Packets;
using Game.Server.Components;
using Game.Server.Entities;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Game.Server.Systems
{
    public class CombatSystem : SystemBase
    {
        private QueryDescription _recieveActionRequestQuery = new QueryDescription().WithAll<NetworkConnectionComponent, PlayerInputComponent>();
        private QueryDescription _cooldownQuery = new QueryDescription().WithAll<WeaponTag, CooldownComponent>();
        private QueryDescription _fireQuery = new QueryDescription().WithAll<SelectedWeaponComponent, PlayerInputComponent>();
        

        private ILogger<CombatSystem> _logger;
        public CombatSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<CombatSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
            PacketDispatcher.Subscribe<ActionRequestPacket>(HandleActionRequest);
        }

        public override void Update(float deltaTime)
        {
            UpdateWeaponCooldowns(deltaTime);
            FireWeapon();
        }

        public void HandleActionRequest(NetPeer peer, ActionRequestPacket packet)
        {
            World.World.Query(in _recieveActionRequestQuery, (Entity entity, ref NetworkConnectionComponent ncc, ref PlayerInputComponent pic) =>
            {
                if (peer.Id == ncc.Peer.Id)
                {
                    pic.MousePosition = packet.CastDirection;
                    pic.Fire = true;
                }
            });
        }

        private void UpdateWeaponCooldowns(float deltaTime)
        {
            World.World.Query(in _cooldownQuery, (Entity entity, ref CooldownComponent cc) =>
            {
                cc.TimeLeft -= deltaTime;
                
            });
        }

        private void FireWeapon()
        {
            World.World.Query(in _fireQuery, (Entity entity, ref SelectedWeaponComponent swc, ref PlayerInputComponent pic) =>
            {
                if (!pic.Fire)
                    return;
                
                //Check if current weapon is on cooldown
                if(swc.Weapon.Entity.TryGet<CooldownComponent>(out var cooldownComponent))
                {
                    if(cooldownComponent.TimeLeft > 0)
                    {
                        _logger.LogTrace($"Weapon on cooldown. Time left: {cooldownComponent.TimeLeft}");
                        pic.Fire = false;
                        return;
                    }
                    else
                    {
                        _logger.LogTrace($"Weapon NOT on cooldown. Time left: {cooldownComponent.TimeLeft}");
                        cooldownComponent.TimeLeft = cooldownComponent.Cooldown;
                        swc.Weapon.Entity.Set(cooldownComponent);
                    }

                }                
                   
                //Fire weapon
                ProjectileFactory.CreateBullet(World.World, ref entity, ref swc.Weapon, pic.MousePosition);
                pic.Fire = false;
            });
 

        }




    }
}
