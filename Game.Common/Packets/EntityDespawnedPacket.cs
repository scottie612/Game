using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Packets
{
    public struct EntityDespawnedPacket : IPacket
    {
        public PacketType PacketType => PacketType.EntityDespawned;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.ReliableOrdered;

        public bool IsBatched => false;

        public NetPeer? NetPeer { get; set; }

        public int EntityID { get; set; }

        public EntityType Type { get; set; }

        public void Serialize(NetDataWriter writer)
        {
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
