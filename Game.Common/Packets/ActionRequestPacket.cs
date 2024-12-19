using Game.Common.Events;
using Game.Configuration;
using Game.Extentions;
using LiteNetLib.Utils;
using System.Numerics;

namespace Game.Packets
{

    public struct ActionRequestPacket: INetSerializable, IPacket, IEvent
    {
        public Packet PacketType => Packet.ActionRequest;
        public int AbilityIndex { get; set; }
        public Vector2 CastDirection { get; set; }
         
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PacketType);
            writer.Put(AbilityIndex);
            writer.Put(CastDirection);
        }
        public void Deserialize(NetDataReader reader)
        {
            AbilityIndex = reader.GetInt();
            CastDirection = reader.GetVector2();
        }

    }
}
