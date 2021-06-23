using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandControlTool
{
    public static class Tool
    {
        public class AABB
        {
            public float left;
            public float right;
            public float bottom;
            public float top;
        }
        static Tool()
        {
            nineDirections.Add(new Vector2(1, 0).normalized);
            nineDirections.Add(new Vector2(-1, 0).normalized);
            nineDirections.Add(new Vector2(0, 1).normalized);
            nineDirections.Add(new Vector2(0, -1).normalized);
            nineDirections.Add(new Vector2(1, 1).normalized);
            nineDirections.Add(new Vector2(1, -1).normalized);
            nineDirections.Add(new Vector2(-1, 1).normalized);
            nineDirections.Add(new Vector2(-1, -1).normalized);
            nineDirections.Add(new Vector2(0, 0).normalized);

            eightDirections.Add(new Vector2(1, 0).normalized);
            eightDirections.Add(new Vector2(-1, 0).normalized);
            eightDirections.Add(new Vector2(0, 1).normalized);
            eightDirections.Add(new Vector2(0, -1).normalized);
            eightDirections.Add(new Vector2(1, 1).normalized);
            eightDirections.Add(new Vector2(1, -1).normalized);
            eightDirections.Add(new Vector2(-1, 1).normalized);
            eightDirections.Add(new Vector2(-1, -1).normalized);
        }

        public class TryMoveRegionParams
        {
            public Vector2 fistPos;
            public Vector2 joint1Pos;
            public float length;
        }

        public static void TryMove(TryMoveRegionParams @params, Vector2 moveVector, out Vector2 offset)
        {
            float minX = @params.joint1Pos.x - @params.length;
            float maxX = @params.joint1Pos.x + @params.length;
            float minY = @params.joint1Pos.y - @params.length;
            float maxY = @params.joint1Pos.y + @params.length;

            Vector2 afterMove = @params.fistPos + moveVector;
            afterMove.x = Mathf.Clamp(afterMove.x, minX, maxX);
            afterMove.y = Mathf.Clamp(afterMove.y, minY, maxY);

            offset = afterMove - @params.fistPos;
        }

        /*
                九向相关
         */
        public static List<Vector2> nineDirections = new List<Vector2>();
        public static List<Vector2> eightDirections = new List<Vector2>();
        public static Vector2 ArbitraryDirectionToNineDirection(Vector2 direction)
        {
            float directionRightPart = 0;
            float rightDot = Vector2.Dot(direction, Vector2.right);
            directionRightPart = Mathf.Sign(rightDot) * (Ludo.Utility.FloatEqual_WithIn0p001(rightDot, 0) ? 0 : 1);
            float directionUpPart = 0;
            float upDot = Vector2.Dot(direction, Vector2.up);
            directionUpPart = Mathf.Sign(upDot) * (Ludo.Utility.FloatEqual_WithIn0p001(upDot, 0) ? 0 : 1);
            direction = (directionRightPart * Vector2.right + directionUpPart * Vector2.up).normalized;
            return direction;
        }

        public static Vector2 MostCloseNineDirection(Vector2 direction)
        {
            //先判断是否为0，然后再判断和8向的夹角
            if (Ludo.Utility.FloatEqual_WithIn0p001(direction.x, 0) && Ludo.Utility.FloatEqual_WithIn0p001(direction.y, 0))
            {
                return new Vector2(0, 0);
            }

            foreach (Vector2 oneDir in eightDirections)
            {
                if (Vector2.Angle(direction, oneDir) <= 22.5)
                {
                    return oneDir;
                }
            }

            throw new System.Exception("Direction should be found in previous code.");
        }
    }
}

