using System;

namespace Game.Extentions
{
    public static class FloatExtentions
    {
        public static float ToRadians(this float angle)
        {
            return (MathF.PI / 180) * angle;
        }

        public static float ToDegrees(this float angle)
        {
            return (angle * 180) / MathF.PI;
        }

        public static float SafeDivide(this float numerator, float denominator)
        {
            return (denominator == 0) ? 0 : numerator / denominator;
        }
    }
}
