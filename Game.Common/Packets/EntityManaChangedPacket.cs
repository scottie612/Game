using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Common.Packets
{
    public struct EntityManaChangedPacket : IPacket
    {
        public PacketType PacketType => PacketType.EntityManaChanged;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.Unreliable;

        public bool IsBatched => true;

        public NetPeer? NetPeer { get; set; }

        public int EntityID { get; set; }

        public int MaxValue { get; set; }

        public int CurrentValue { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityID);
            writer.Put(MaxValue);
            writer.Put(CurrentValue);
        }
        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
            MaxValue = reader.GetInt();
            CurrentValue = reader.GetInt();
        }

    }
}
