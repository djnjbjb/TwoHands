using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum RightOrLeftFist
{
    Right = 1,
    Left = -1
}

public enum Joint2State
{
    Singularity = 0x00,
    Line = 0x01,
    TriangleLeft = 0x10,
    TriangleRight = 0x11
}
public static class Joint2StateExtension
{
    public static bool IsTriangle(this Joint2State state)
    {
        return ( ((int)state & 0x10)  == 0x10);
    }
}
