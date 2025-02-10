using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Common.Packets
{

    /// <summary>
    /// Client will send witch index in their Hotbar they would like to be their selected item.
    /// </summary>
    public struct ChangeSelectedHotbarIndexRequestPacket : IPacket
    {
        public PacketType PacketType => PacketType.ChangeSelectedHotbarIndexRequest;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.Unreliable;

        public bool IsBatched => false;

        public NetPeer? NetPeer { get; set; }

        public int Index { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Index);
        }
        public void Deserialize(NetDataReader reader)
        {
            Index = reader.GetInt();
        }

    }
}
