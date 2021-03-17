using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*                                       *
 *          Right Or Left Hand           *
 *                                       */
enum RightOrLeftHand
{
    Right = 1,
    Left = -1
}


/*                              *
 *          HandState           *
 *                              */
public enum HandState
{
    Free = 0,
    GrabStuff = 1,
    GrabEnv = 2,
    GrabNothing = 3
}

public static class HandStateExendsion
{
    public static bool FreeToMove(this HandState state)
    {
        return state == HandState.Free || state == HandState.GrabNothing || state == HandState.GrabStuff;
    }

    public static bool GrabPressed(this HandState state)
    {
        return state == HandState.GrabNothing || state == HandState.GrabEnv || state == HandState.GrabStuff;
    }
    public static bool GrabedSomething(this HandState state)
    {
        return state == HandState.GrabEnv || state == HandState.GrabStuff;
    }
}

/*                              *
 *          FootState           *
 *                              */
public enum FootState
{
    Air,
    Surface,
    EnvGround,
    EnvRock
}