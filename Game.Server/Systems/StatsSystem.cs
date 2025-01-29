using Arch.Buffer;
using Arch.Core;
using Game.Common;
using Game.Common.Packets;
using Game.Server.Components;
using Microsoft.Extensions.Logging;

namespace Game.Server.Systems
{
    public class StatsSystem : SystemBase
    {
        public QueryDescription _healthDirtyQuery = new QueryDescription().WithAll<HealthDirtyTag, HealthComponent>();
        public QueryDescription _manaDirtyQuery = new QueryDescription().WithAll<ManaDirtyTag, ManaComponent>();

        private ILogger<StatsSystem> _logger;
        public StatsSystem(GameWorld world, PacketDispatcher packetDispatcher, ILogger<StatsSystem> logger) : base(world, packetDispatcher)
        {
            _logger = logger;
        }

        public override void Update(float deltaTime)
        {
            SendHeathUpdates();
            SendManaUpdates();
        }

        private void SendHeathUpdates()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _healthDirtyQuery, (Entity entity, ref HealthComponent hc) =>
            {
                var packet = new EntityHealthChangedPacket();
                packet.EntityID = entity.Id;
                packet.MaxValue = hc.MaxValue;
                packet.CurrentValue = hc.CurrentValue;

                PacketDispatcher.Enqueue(packet);

                buffer.Remove<HealthDirtyTag>(entity);
            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }

        private void SendManaUpdates()
        {
            var buffer = new CommandBuffer();
            World.World.Query(in _manaDirtyQuery, (Entity entity, ref ManaComponent mc) =>
            {
                var packet = new EntityManaChangedPacket();
                packet.EntityID = entity.Id;
                packet.MaxValue = mc.MaxValue;
                packet.CurrentValue = mc.CurrentValue;

                PacketDispatcher.Enqueue(packet);

                buffer.Remove<ManaDirtyTag>(entity);
            });
            buffer.Playback(World.World);
            buffer.Dispose();
        }


    }
}
