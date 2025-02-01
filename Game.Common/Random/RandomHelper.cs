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
    }
}
