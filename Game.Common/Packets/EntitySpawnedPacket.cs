using Game.Configuration;
using Game.Events;
using LiteNetLib.Utils;
using System.Collections.Generic;

namespace Game.Packets
{
    public struct EntitySpawnedPacket : INetSerializable, IPacket, IEntityEvent
    {
       
        public Packet PacketType => Packet.EntitySpawned;
        public int EntityID { get; set; }
        public EntityType Type { get; set; }
        public float StartingX { get; set; }
        public float StartingY { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PacketType);
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
