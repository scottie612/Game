using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Packets
{
    public struct IdentityPacket : IPacket
    {
        public PacketType PacketType => PacketType.Identity;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.ReliableOrdered;

        public bool IsBatched => false;

        public NetPeer? NetPeer { get; set; }

        public int EntityID { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityID);
        }
        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
        }
    }
}
