using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Game.Common.Events;
using Game.Configuration;
using Game.Console.Helpers;
using Game.EntityComponentSystem.Components.Tags;
using Game.Extentions;
using Game.Packets;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.EntityComponentSystem.Systems
{
    public partial class MovementSystem : BaseSystem<World, float>
    {
        private QueryDescription _recieveMovementRequestQuery = new QueryDescription().WithAll<NetworkConnectionComponent, PlayerInputComponent>();
        private QueryDescription _inputQuery = new QueryDescription().WithAll<VelocityComponent, PlayerInputComponent>();
        private QueryDescription _moveSpeedQuery = new QueryDescription().WithAll<VelocityComponent, MovementSpeedComponent>();
        private QueryDescription _movementQuery = new QueryDescription().WithAll<PositionComponent, VelocityComponent>();
        private QueryDescription _sendMovementQuery = new QueryDescription().WithAll<PositionComponent, PositionDiryTag>();


        private NetManager _netManager;
        private NetDataWriter _netDataWriter;
        private BatchPacketProcessor _batchPacketProcessor;
        public MovementSystem(World world, NetManager netManager) : base(world)
        {
            _netManager = netManager;

            _netDataWriter = new NetDataWriter();

            _batchPacketProcessor = new BatchPacketProcessor(Packet.EntityMovement, DeliveryMethod.Unreliable, _netDataWriter, _netManager);
        }

        public override void Update(in float deltaTime)
        {
            var time = deltaTime / 1000;

            UpdateVelocityWithPlayerInput();
            UpdateVelocityWithMovementSpeed();
            UpdatePositionWithVelocity(time);
            SendPositionUpdates();
        }


        public void HandleMovementRequest(NetDataReader data, NetPeer peer)
        {
            World.Query(in _recieveMovementRequestQuery, (Entity entity, ref NetworkConnectionComponent ncc, ref PlayerInputComponent pic) =>
            {
                if (peer.Id == ncc.Peer.Id)
                {
                    pic.InputVector.X = data.GetFloat();
                    pic.InputVector.Y = data.GetFloat();
                }
            });
        }

        private void UpdateVelocityWithPlayerInput()
        {
            World.Query(in _inputQuery, (Entity entity, ref VelocityComponent vel, ref PlayerInputComponent pic) =>
            {
                vel.Value = pic.InputVector.NormalizeSafe();
            });
        }

        private void UpdateVelocityWithMovementSpeed()
        {
            World.Query(in _moveSpeedQuery, (Entity entity, ref VelocityComponent vel, ref MovementSpeedComponent ms) =>
            {
                vel.Value = vel.Value.NormalizeSafe();
                vel.Value.X *= ms.Value;
                vel.Value.Y *= ms.Value;
            });
        }

        private void UpdatePositionWithVelocity(float deltaTime)
        {
            var buffer = new CommandBuffer();
            World.Query(in _movementQuery, (Entity entity, ref PositionComponent pos, ref VelocityComponent vel) =>
            {
                pos.Value += vel.Value * deltaTime;

                if (vel.Value.Length() > 0)
                {
                    buffer.Add<PositionDiryTag>(entity);
                }
            });

            buffer.Playback(World);
        }

        private void SendPositionUpdates()
        {
            var totalNumberOfDirtyEntities = World.CountEntities(in _sendMovementQuery);

            _batchPacketProcessor.Reset(totalNumberOfDirtyEntities);

            var buffer = new CommandBuffer();
            World.Query(in _sendMovementQuery, (Entity entity, ref PositionComponent pos) =>
            {
                var data = new EntityMovementData { EntityID = entity.Id, Position = pos.Value };
                _batchPacketProcessor.ProcessEntity(data);

                buffer.Remove<PositionDiryTag>(entity);
            });
            buffer.Playback(World);
        }

    }

}
