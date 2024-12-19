using Game.Configuration;
using Game.Events;
using LiteNetLib.Utils;

namespace Game.Packets
{
    public struct IdentityPacket : INetSerializable, IPacket, IEntityEvent
    {
        public Packet PacketType => Packet.Identity;
        public int EntityID { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PacketType);
            writer.Put(EntityID);
        }
        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
        }
    }
}
