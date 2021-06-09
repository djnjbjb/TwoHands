using UnityEngine;

namespace Ludo
{
    public class Utility
    {
        public static Vector2 Vec2Rotate(Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        public static bool FloatEqual_WithIn0p001(float a, float b)
        {
            float threshold = 0.001f;
            if (Mathf.Abs(a - b) <= threshold)
                return true;
            else
                return false;
        }

        public static Vector2 FindNearestPointOnLine(Vector2 lineOrigin, Vector2 lineDirection, Vector2 point)
        {
            lineDirection.Normalize();
            Vector2 lhs = point - lineOrigin;

            float dotP = Vector2.Dot(lhs, lineDirection);
            return lineOrigin + lineDirection * dotP;
        }
    }
}

