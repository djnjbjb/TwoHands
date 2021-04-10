using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HandControlTool
{
    public class TryMoveRegionParams
    {
        public Vector2 joint1Pos;
        public float length;
        public Vector2 fistPos;
    }
    public static void TryMove(TryMoveRegionParams @params, Vector2 moveVector, out Vector2 offset)
    {
        float minY = @params.joint1Pos.y - @params.length;
        float maxY = @params.joint1Pos.y + @params.length;
        float minX = @params.joint1Pos.x - @params.length;
        float maxX = @params.joint1Pos.x + @params.length;

        Vector2 afterMove = @params.fistPos + moveVector;
        afterMove.x = Mathf.Clamp(afterMove.x, minX, maxX);
        afterMove.y = Mathf.Clamp(afterMove.y, minY, maxY);

        offset = afterMove - @params.fistPos;
    }
}
