using Arch.Buffer;
using Arch.Core;
using Game.Common;
using Game.Common.Extentions;
using Game.Packets;
using Game.Server.Components;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Game.Server.Systems
{
    public class MovementSystem : SystemBase
    {
        private QueryDescription _recieveMovementRequestQuery = new QueryDescription().WithAll<NetworkConnectionComponent, PlayerInputComponent>();
        private QueryDescription _inputQuery = new QueryDescription().WithAll<VelocityComponent, PlayerInputComponent>();
        private QueryDescription _moveSpeedQuery = new QueryDescription().WithAll<VelocityComponent, MovementSpeedComponent>();
        private QueryDescription _movementQuery = new QueryDescription().WithAll<PositionComponent, VelocityComponent>();
        private QueryDescription _sendMovementQuery = new QueryDescription().WithAll<PositionComponent, PositionDiryTag>();

        private ILogger<MovementSystem> _logger;
        public MovementSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<MovementSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
            PacketDispatcher.Subscribe<MovementRequestPacket>(HandleMovementRequest);
        }

        public override void Update(float deltaTime)
        {
            UpdateVelocityWithPlayerInput();
            UpdateVelocityWithMovementSpeed();
            UpdatePositionWithVelocity(deltaTime);
            SendPositionUpdates();
        }

        public void HandleMovementRequest(NetPeer peer, MovementRequestPacket packet)
        {
            World.World.Query(in _recieveMovementRequestQuery, (Entity entity, ref NetworkConnectionComponent ncc, ref PlayerInputComponent pic) =>
            {
                if (peer.Id == ncc.Peer.Id)
                {
                    pic.MovemenetVector.X = packet.XComponent;
                    pic.MovemenetVector.Y = packet.YComponent;
                }
            });
        }

        private void UpdateVelocityWithPlayerInput()
        {
            World.World.Query(in _inputQuery, (Entity entity, ref VelocityComponent vel, ref PlayerInputComponent pic) =>
            {
                vel.Value = pic.MovemenetVector.NormalizeSafe();
            });
        }

        private void UpdateVelocityWithMovementSpeed()
        {
            World.World.Query(in _moveSpeedQuery, (Entity entity, ref VelocityComponent vel, ref MovementSpeedComponent ms) =>
            {
                vel.Value = vel.Value.NormalizeSafe();
                vel.Value.X *= ms.Value;
                vel.Value.Y *= ms.Value;
            });
        }

        private void UpdatePositionWithVelocity(float deltaTime)
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _movementQuery, (Entity entity, ref PositionComponent pos, ref VelocityComponent vel) =>
            {
                pos.Value += vel.Value * deltaTime;

                if (vel.Value.Length() > 0)
                {
                    buffer.Add<PositionDiryTag>(entity);
                }
            });

            buffer.Playback(World.World);
        }

        private void SendPositionUpdates()
        {

            var buffer = new CommandBuffer();
            World.World.Query(in _sendMovementQuery, (Entity entity, ref PositionComponent pos) =>
            {
                var packet = new EntityMovementPacket();
                packet.EntityID = entity.Id;
                packet.Position = pos.Value;

                PacketDispatcher.Enqueue(packet);

                buffer.Remove<PositionDiryTag>(entity);
            });
            buffer.Playback(World.World);
        }


    }
}
