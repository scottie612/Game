using Game.Common.Enums;
using LiteNetLib.Utils;
using System.Numerics;

namespace Game.Common.Extentions
{
    public static class SerializingExtentions
    {
        public static void Put(this NetDataWriter writer, Vector2 vector)
        {
            writer.Put(vector.X);
            writer.Put(vector.Y);
        }

        public static Vector2 GetVector2(this NetDataReader reader)
        {
            return new Vector2(reader.GetFloat(), reader.GetFloat());
        }

        public static void Put(this NetDataWriter writer, EntityType entityType)
        {
            writer.Put((ushort)entityType);
        }

        public static EntityType GetEntityType(this NetDataReader reader)
        {
            return (EntityType)reader.GetUShort();
        }
    }
}
