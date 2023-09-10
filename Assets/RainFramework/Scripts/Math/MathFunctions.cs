using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RainFramework.Math
{
    // Class for holding math related functions
    // Written for RainFramework by BladeSides

    public static class MathFunctions
    {

        /// <summary>
        /// Returns the direction vector from an angle vector, takes values in radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector2 AngleToDirection(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>
        /// Returns the angle in radians from a direction vector. Returns a radian angle.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float DirectionToAngle(Vector2 direction)
        {
            return Mathf.Atan2(direction.y,direction.x);
        }
    }

}