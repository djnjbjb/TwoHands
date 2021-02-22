using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTool
{
    public static Vector2 Vec2Rotate(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
}


