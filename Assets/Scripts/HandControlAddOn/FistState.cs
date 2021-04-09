using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FistState
{
    Free = 0,
    GrabStuff = 1,
    GrabEnv = 2,
    GrabNothing = 3
}

public class FistStatePlus
{
    public FistState state { get; private set; }
    public FistState pre { get; private set; }

    public static implicit operator FistState(FistStatePlus handState) => handState.state;
    public static implicit operator FistStatePlus(FistState handState) => new FistStatePlus(handState);
    
    public bool IsFreeToMove()
    {
        return state == FistState.Free || state == FistState.GrabNothing || state == FistState.GrabStuff;
    }
    public bool IsGrabPressed()
    {
        return state == FistState.GrabNothing || state == FistState.GrabEnv || state == FistState.GrabStuff;
    }
    public bool IsGrabingThings()
    {
        return state == FistState.GrabEnv || state == FistState.GrabStuff;
    }

    public FistStatePlus(FistState handState)
    {
        state = handState;
        pre = handState;
    }

    public void FixedUpdateManually(GameObject fist, bool altPressed)
    {
        pre = state;

        if (!altPressed)
        {
            state = FistState.Free;
            return;
        }
        if (pre == FistState.GrabStuff && altPressed)
        {
            state = FistState.GrabStuff;
            return;
        }

        LayerMask LMStuff = LayerMask.GetMask("Stuff");
        LayerMask LMEnv = LayerMask.GetMask("EnvRock", "EnvGround");
        //GrabStuff的优先级较高，比Env之类的高。
        var colliderStuff = Physics2D.OverlapCircle
            (fist.transform.position, fist.transform.lossyScale.x / 2, LMStuff);
        if (colliderStuff)
        {
            state = FistState.GrabStuff;
            return;
        }
        var colliderEnv = Physics2D.OverlapCircle
            (fist.transform.position, fist.transform.lossyScale.x / 2, LMEnv);
        if (colliderEnv)
        {
            state  = FistState.GrabEnv;
        }
        state =  FistState.GrabNothing;
    }

}