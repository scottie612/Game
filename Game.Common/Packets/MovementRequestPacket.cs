using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Packets
{
    public struct MovementRequestPacket : IPacket
    {
        public PacketType PacketType => PacketType.MovementRequest;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.ReliableOrdered;

        public bool IsBatched => false;

        public NetPeer? NetPeer { get; set; }

        public float XComponent { get; set; }

        public float YComponent { get; set; }

        public void Serialize(NetDataWriter writer)
        {
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
