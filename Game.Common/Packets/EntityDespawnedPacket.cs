using Game.Configuration;
using Game.Events;
using LiteNetLib.Utils;

namespace Game.Packets
{
    public struct EntityDespawnedPacket : INetSerializable, IPacket, IEntityEvent
    {
        public Packet PacketType => Packet.EntityDespawned;
        public int EntityID { get; set; }
        public EntityType Type { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PacketType);
            writer.Put(EntityID);
            writer.Put((ushort)Type);
        }

        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
            Type = (EntityType)reader.GetUShort();
        }
    }
}
