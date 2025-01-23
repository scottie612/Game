using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using Game.Common.Extentions;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Game.Packets
{
    public struct EntityMovementPacket : IPacket
    {
        public PacketType PacketType => PacketType.EntityMovement;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.Unreliable;

        public bool IsBatched => true;

        public NetPeer? NetPeer { get; set; }

        public int EntityID { get; set; }

        public Vector2 Position { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityID);
            writer.Put(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
            Position = reader.GetVector2();
        }
    }
}


