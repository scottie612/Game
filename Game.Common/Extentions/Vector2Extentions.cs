using System.Numerics;
using System;

namespace Game.Common.Extentions
{
    public static class Vector2Extentions
    {

        /// <summary>
        /// Normalizes a vector, If the length is zero, return the zero vector rather than NaN
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2 NormalizeSafe(this Vector2 vector)
        {
            float length = vector.Length();
            if (length > 0)
            {
                return vector / length;
            }
            else
            {
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Returns the equivalent Angle in degrees of the Vector
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public static float Degrees(this Vector2 coordinates)
        {
            //Mathf.Atan2's output is from (-pi -> pi). We need to convert this to degrees (-180 -> 180)
            var angle = MathF.Atan2(coordinates.Y, coordinates.X).ToDegrees();

            // Now that we are in degrees, convert the domain to (0 -> 360)
            var convertedAngle = ((angle % 360) + 360) % 360;
            return convertedAngle;
        }

        /// <summary>
        /// Returns the equivalent Angle in radians of the Vector
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public static float Radians(this Vector2 coordinates)
        {
            //Mathf.Atan2's output is from (-pi -> pi).
            var angle = MathF.Atan2(coordinates.Y, coordinates.X);

            // Now that we are in degrees, convert the domain to (0 -> 360)
            var convertedAngle = ((angle % 360) + 360) % 360;
            return convertedAngle;
        }


        /// <summary>
        /// returns a rotated a vector by a given number of degrees
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static Vector2 Rotate(this Vector2 vector, float degrees)
        {
            float radians = degrees.ToRadians();

            float sin = MathF.Sin(radians);
            float cos = MathF.Cos(radians);

            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }
    }

}
