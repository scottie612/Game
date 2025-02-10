using Game.Common.Enums;
using Game.Common.Extentions;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Game.Packets
{
    public struct MovementRequestPacket : IPacket
    {
        public PacketType PacketType => PacketType.MovementRequest;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.ReliableOrdered;

        public bool IsBatched => false;

        public NetPeer? NetPeer { get; set; }

        public Vector2 InputVector { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(InputVector);
        }
        public void Deserialize(NetDataReader reader)
        {
            InputVector = reader.GetVector2();
        }
    }
}
