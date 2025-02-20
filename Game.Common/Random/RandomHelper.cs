using System.Numerics;

namespace Game.Common.Random
{
    public static class RandomHelper
    {
        private static System.Random _random = new System.Random();

        public static int RandomInt(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static float RandomFloat(float min, float max)
        {
            return (float)_random.NextDouble() * (max - min) + min;
        }

        public static Vector2 RandomVector2(float min, float max)
        {
            return new Vector2()
            {
                X = RandomFloat(min, max),
                Y = RandomFloat(min, max)
            };
        }
        public static Vector2 RandomVector2(float minX, float maxX, float minY, float maxY)
        {
            return new Vector2()
            {
                X = RandomFloat(minX, maxX),
                Y = RandomFloat(minY, maxY)
            };
        }
    }
}
