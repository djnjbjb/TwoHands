using UnityEngine;


/*
    FistState 有以下五种状态

    需要注意的是 GrabStuffEnv
        并不是抓着剑就是GrabStuffEnv。
        抓着剑，如果可以任意移动，就是Stuff。
        剑插在Env里时，才是GrabStuffEnv。
*/
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

        if (pre == FistState.GrabEnv && altPressed)
        {
            //上一帧是Env，则优先Env
            LayerMask LMEnv = LayerMask.GetMask("EnvRock", "EnvGround", "EnvRoundRock");
            Collider2D fistCollider = fist.GetComponent<Collider2D>();

            ContactFilter2D filterEnv = new ContactFilter2D();
            filterEnv.SetLayerMask(LMEnv);
            Collider2D[] result = new Collider2D[1];
            if (Physics2D.OverlapCollider(fistCollider, filterEnv, result) > 0)
            {
                state = FistState.GrabEnv;
                return;
            }
            else
            {
                goto NormalAltPressedState;
            }
        }

        if (pre == FistState.GrabStuff && altPressed)
        {
            if (grabedStuff != null)
            {
                return;
            }
            else
            {
                goto NormalAltPressedState;
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
                goto NormalAltPressedState;
            }
            
        }

    NormalAltPressedState:
        {
            LayerMask LMStuff = LayerMask.GetMask("Stuff");
            LayerMask LMEnv = LayerMask.GetMask("EnvRock", "EnvGround", "EnvRoundRock");

            Collider2D fistCollider = fist.GetComponent<Collider2D>();

            //Stuff
            {
                ContactFilter2D filterStuff = new ContactFilter2D();
                filterStuff.SetLayerMask(LMStuff);
                Collider2D[] result = new Collider2D[1];

                //目前的条件是，对colliderStuff来说。他自己，或者他的parrent，一定包含Stuff脚本
                if (Physics2D.OverlapCollider(fistCollider, filterStuff, result) > 0)
                {
                    Stuff.Stuff stuff = result[0].gameObject.GetComponent<Stuff.Stuff>();
                    if (stuff == null)
                    {
                        stuff = result[0].gameObject.transform.parent.GetComponent<Stuff.Stuff>();
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
            {
                ContactFilter2D filterEnv = new ContactFilter2D();
                filterEnv.SetLayerMask(LMEnv);
                Collider2D[] result = new Collider2D[1];
                if (Physics2D.OverlapCollider(fistCollider, filterEnv, result) > 0)
                {
                    state = FistState.GrabEnv;
                    return;
                }
            }
            
            //Nothing
            state = FistState.GrabNothing;
        }
    }

}