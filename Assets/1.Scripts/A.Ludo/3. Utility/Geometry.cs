﻿using UnityEngine;

namespace Ludo.Utility
{
    public class Geometry
    {
        /// <summary>
        /// 旋转向量
        /// </summary>
        /// <param name="v"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static Vector2 Vec2Rotate(Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        public static Vector2 FindNearestPointOnLine(Vector2 lineOrigin, Vector2 lineDirection, Vector2 point)
        {
            lineDirection.Normalize();
            Vector2 lhs = point - lineOrigin;

            float dotP = Vector2.Dot(lhs, lineDirection);
            return lineOrigin + lineDirection * dotP;
        }

        public static (Vector3 position, Quaternion rotation) RotateRoundPoint2D(Vector3 objectPosition, Quaternion objectRotation, Vector2 rotatePoint, float angle)
        {
            Vector3 v1 = (Vector2)objectPosition - rotatePoint;
            Vector3 v2 = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * v1;
            Vector3 delta = v2 - v1;

            Vector3 position = objectPosition + delta;
            Quaternion rotation = objectRotation * Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));

            return (position, rotation);
        }

    }
}

