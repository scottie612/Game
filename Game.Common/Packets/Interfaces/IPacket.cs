using Game.Common.Enums;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Common.Packets.Interfaces
{
    public interface IPacket : INetSerializable
    {
        PacketType PacketType { get; }
        DeliveryMethod DeliveryMethod { get; }
        bool IsBatched { get; }

        /// <summary>
        /// If NetPeer is null, the packet will send to all
        /// </summary>
        NetPeer? NetPeer { get; set; }
    }
}
