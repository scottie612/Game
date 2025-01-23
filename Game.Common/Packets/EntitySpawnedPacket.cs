using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Packets
{
    public struct EntitySpawnedPacket : IPacket
    {
        public PacketType PacketType => PacketType.EntitySpawned;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.ReliableOrdered;

        public bool IsBatched => false;

        public int EntityID { get; set; }

        public NetPeer? NetPeer { get; set; }

        public EntityType Type { get; set; }

        public float StartingX { get; set; }

        public float StartingY { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityID);
            writer.Put((ushort)Type);
            writer.Put(StartingX);
            writer.Put(StartingY);
        }

        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
            Type = (EntityType)reader.GetUShort();
            StartingX = reader.GetFloat();
            StartingY = reader.GetFloat();
        }
    }
}
