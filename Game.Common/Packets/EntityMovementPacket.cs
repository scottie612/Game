using Game.Configuration;
using Game.Events;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Game.Packets
{
    public class EntityMovementPacket : INetSerializable, IDisposable
    {
        public List<(int EntityID, float X, float Y)> Data = new List<(int EntityID, float X, float Y)>();

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Packet.EntityMovement);
            writer.Put((ushort)Data.Count);
            for (int i = 0; i < Data.Count; i++) 
            {
                writer.Put(Data[i].EntityID);
                writer.Put(Data[i].X);
                writer.Put(Data[i].Y);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            var count = reader.GetUShort();
            for (int i = 0; i < count; i++)
            {
                var entityID = reader.GetInt();
                var x = reader.GetFloat();
                var y = reader.GetFloat();
                Data.Add((entityID, x, y));
            }
        }

        public void Dispose()
        {
            Data.Clear();
        }
    }

    public struct EntityMovementData : INetSerializable, IEntityEvent
    {
        public int EntityID { get; set; }
        public Vector2 Position;

        public void Deserialize(NetDataReader reader)
        {
            EntityID = reader.GetInt();
            Position.X = reader.GetFloat();
            Position.Y = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityID);
            writer.Put(Position.X);
            writer.Put(Position.Y);
        }
    }


}


