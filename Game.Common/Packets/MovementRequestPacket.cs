using Game.Common.Events;
using Game.Configuration;
using LiteNetLib.Utils;

namespace Game.Packets
{
    public struct MovementRequestPacket : INetSerializable, IEvent, IPacket
    {
        public Packet PacketType => Packet.MovementRequest;
        public float XComponent { get; set; }
        public float YComponent { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PacketType);
            writer.Put(XComponent);
            writer.Put(YComponent);
        }
        public void Deserialize(NetDataReader reader)
        {
            XComponent = reader.GetFloat();
            YComponent = reader.GetFloat();
        }
    }
}
