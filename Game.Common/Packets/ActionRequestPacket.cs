using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using Game.Common.Extentions;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Game.Packets
{
    public struct ActionRequestPacket: IPacket
    {
        public PacketType PacketType => PacketType.ActionRequest;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.Unreliable;

        public bool IsBatched => false;

        public NetPeer? NetPeer { get; set; }

        public Vector2 CastDirection { get; set; }
         
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CastDirection);
        }
        public void Deserialize(NetDataReader reader)
        {
            CastDirection = reader.GetVector2();
        }

    }
}
