using Game.Common.Enums;
using Game.Common.Extentions;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Game.Common.Packets
{
    public struct EntityAttackedPacket : IPacket
    {
        public PacketType PacketType => PacketType.EntityAttacked;

        public DeliveryMethod DeliveryMethod => DeliveryMethod.Unreliable;

        public bool IsBatched => true;

        public NetPeer? NetPeer { get; set; }

        public int EntityID { get; set; }

        public Vector2 AttackDirection {  get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityID);
            writer.Put(AttackDirection);
        }
        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
            AttackDirection = reader.GetVector2();
        }
    }
}
