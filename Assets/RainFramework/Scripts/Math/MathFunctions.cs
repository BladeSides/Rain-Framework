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


        // Functions for Smooth Lerping Below
        public static float LerpSmooth(float a, float b, float dt, float h)
        {
            return b + (a - b) * Mathf.Pow(2, -dt / h);
        }

        public static float LerpSmooth(float a, float b, float dt, float t, float precision)
        {
            return b + (a - b) * Mathf.Pow(2, -dt / GetHalfLife(t, precision));
        }

        public static Vector3 LerpSmooth(Vector3 a, Vector3 b, float dt, float h)
        {
            return b + (a - b) * Mathf.Pow(2, -dt / h);
        }

        public static Vector3 LerpSmooth(Vector3 a, Vector3 b, float dt, float t, float precision)
        {
            return b + (a - b) * Mathf.Pow(2, -dt / GetHalfLife(t, precision));
        }

        public static Quaternion SlerpSmooth(Quaternion a, Quaternion b, float dt, float h)
        {
            return (Quaternion.Slerp(a, b, 1 - Mathf.Pow(2, -dt / h)));
        }

        public static Quaternion SlerpSmooth(Quaternion a, Quaternion b, float dt, float t, float precision)
        {
            return (Quaternion.Slerp(a, b, 1 - Mathf.Pow(2, -dt / GetHalfLife(t, precision))));
        }

        public static Quaternion LerpSmooth(Quaternion a, Quaternion b, float dt, float h)
        {
            return (Quaternion.Lerp(a, b, 1 - Mathf.Pow(2, -dt / h)));
        }

        public static Quaternion LerpSmooth(Quaternion a, Quaternion b, float dt, float t, float precision)
        {
            return (Quaternion.Lerp(a, b, 1 - Mathf.Pow(2, -dt / GetHalfLife(t, precision))));
        }

        public static float GetHalfLife(float t, float precision)
        {
            return -t / Mathf.Log(precision, 2);
        }

    }

}