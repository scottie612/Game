using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Common.Packets
{
    public struct EntityDiedPacket : IPacket
    {
        public PacketType PacketType => PacketType.EntityDied;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.Unreliable;

        public bool IsBatched => true;

        public NetPeer? NetPeer { get; set; }

        public int EntityID { get; set; }

        public float RespawnTime { get; set; }


        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityID);
            writer.Put(RespawnTime);
        }
        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
            RespawnTime = reader.GetFloat();
        }
    }
}
