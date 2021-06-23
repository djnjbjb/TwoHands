using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FistState
{
    Free = 0,
    GrabStuff = 1,
    GrabEnv = 2,
    GrabNothing = 3,
    GrabStuffEnv = 4
}

public static class FistStateExtension
{
    public static bool IsFreeToMove(this FistState state)
    {
        return state == FistState.Free || state == FistState.GrabNothing || state == FistState.GrabStuff;
    }
    public static bool IsGrabPressed(this FistState state)
    {
        return state == FistState.GrabNothing || state == FistState.GrabEnv || state == FistState.GrabStuff || state == FistState.GrabStuffEnv;
    }
    public static bool IsGrabingThings(this FistState state)
    {
        return state == FistState.GrabEnv || state == FistState.GrabStuff || state == FistState.GrabStuffEnv;
    }
    public static bool IsGrabing_Env_StuffEnv(this FistState state)
    {
        return state == FistState.GrabEnv || state == FistState.GrabStuffEnv;
    }

    public static bool IsGrabing_Stuff_StuffEnv(this FistState state)
    {
        return state == FistState.GrabStuff || state == FistState.GrabStuffEnv;
    }
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
        return state == FistState.GrabNothing || state == FistState.GrabEnv || state == FistState.GrabStuff || state == FistState.GrabStuffEnv;
    }
    public bool IsGrabingThings()
    {
        return state == FistState.GrabEnv || state == FistState.GrabStuff || state == FistState.GrabStuffEnv;
    }
    public bool IsGrabing_Env_StuffEnv()
    {
        return state == FistState.GrabEnv || state == FistState.GrabStuffEnv;
    }

    public bool IsGrabing_Stuff_StuffEnv()
    {
        return state == FistState.GrabStuff || state == FistState.GrabStuffEnv;
    }

    public FistStatePlus(FistState handState)
    {
        state = handState;
        pre = handState;
    }

    public void FixedUpdateManually(GameObject fist, GameObject grabedStuff, bool altPressed)
    {
        pre = state;

        if (!altPressed)
        {
            state = FistState.Free;
            return;
        }

        if (pre == FistState.GrabStuff && altPressed)
        {
            if (grabedStuff != null)
            {
                return;
            }
            else
            {
                goto NormalPressedState;
            }
            
        }

        if (pre == FistState.GrabStuffEnv && altPressed)
        {
            if (grabedStuff != null)
            {
                Stuff.Sword sword = grabedStuff.GetComponent<Stuff.Sword>();
                var value = sword.GetValueForFist_AtFUPre();
                if (value.overlapWithEnv)
                {
                    state = FistState.GrabStuffEnv;
                }
                else
                {
                    state = FistState.GrabStuff;
                }
            }
            else
            {
                goto NormalPressedState;
            }
            
        }

    NormalPressedState:
        {
            LayerMask LMStuff = LayerMask.GetMask("Stuff");
            LayerMask LMEnv = LayerMask.GetMask("EnvRock", "EnvGround", "EnvRoundRock");

            //Stuff
            {
                //目前的条件是，对colliderStuff来说。他自己，或者他的parrent，一定包含Stuff脚本
                var collider = Physics2D.OverlapCircle
                (fist.transform.position, fist.transform.lossyScale.x / 2, LMStuff);
                if (collider)
                {
                    Stuff.Stuff stuff = collider.gameObject.GetComponent<Stuff.Stuff>();
                    if (stuff == null)
                    {
                        stuff = collider.gameObject.transform.parent.GetComponent<Stuff.Stuff>();
                    }
                    if (stuff == null)
                    {
                        throw new System.Exception("Sutff和collider的结构，不符合预期");
                    }
                    
                    if (!(stuff is Stuff.Sword))
                    {
                        state = FistState.GrabStuff;
                    }
                    if (stuff is Stuff.Sword)
                    {
                        Stuff.Sword sword = stuff as Stuff.Sword;
                        var value = sword.GetValueForFist_AtFUPre();
                        if (value.overlapWithEnv)
                        {
                            state = FistState.GrabStuffEnv;
                        }
                        else
                        {
                            state = FistState.GrabStuff;
                        }
                    }
                    return;
                }
            }

            //Env
            var colliderEnv = Physics2D.OverlapCircle
                (fist.transform.position, fist.transform.lossyScale.x / 2, LMEnv);
            if (colliderEnv)
            {
                state = FistState.GrabEnv;
                return;
            }

            //Nothing
            state = FistState.GrabNothing;
        }
    }

}