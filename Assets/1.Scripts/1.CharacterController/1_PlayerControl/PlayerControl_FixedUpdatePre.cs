using Ludo.TwoHandsWar.PlayerInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerControl : MonoBehaviour
{
    void ___________________________()
    {

    }
    void FU_Pre()
    {
        /*
            State
                FistState
                FootState
         */
        leftFistState.FixedUpdateManually(leftFist, leftGrabedStuff, UniformInput.lGrab);
        rightFistState.FixedUpdateManually(rightFist, rightGrabedStuff, UniformInput.rGrab);
        footState.FixedUpdateManually(bottomLeftPoint: bottomLeftPoint, bottomRightPoint: bottomRightPoint);

        HandStuffFixedUpdate(rightFistState.pre, leftFistState.pre);

        YDebugPanelShow_FUPre();
    }

    void HandStuffFixedUpdate(FistState rightPre, FistState leftPre)
    {
        //需要处理Stuff和StuffEnv两种情况
        //这个函数，只是需要处理 handGrabedStuff 和 stuff.fist
        void HandStuffUpdate_One(ref GameObject handGrabedStuff, FistState state, FistState statePre, GameObject fist, Vector2 mvDir)
        {
            LayerMask LMStuff = LayerMask.GetMask("Stuff");

            if (!statePre.IsGrabing_Stuff_StuffEnv() && state.IsGrabing_Stuff_StuffEnv())
            {
                var collider = Physics2D.OverlapCircle(fist.transform.position,
                    fist.transform.lossyScale.x / 2, LMStuff);
                if (collider == null)
                    throw new System.Exception("collider == null");

                //找collider以及collider的父亲，直到有Component是stuff
                GameObject colliderObject = collider.gameObject;
                GameObject stuffObject = null;
                Stuff.Stuff stuff = null;
                while (true)
                {
                    stuff = colliderObject.GetComponent<Stuff.Stuff>();
                    if (stuff == null)
                    {
                        var transformParent = colliderObject.transform.parent;
                        if (transformParent == null)
                            break;
                        colliderObject = transformParent.gameObject;
                    }
                    else
                    {
                        stuffObject = stuff.gameObject;
                        break;
                    }
                }

                if (stuffObject == null)
                    throw new System.Exception("手的状态为GrabStuff，找不到stuff");

                handGrabedStuff = stuffObject;
                if (fist == leftFist)
                {
                    stuff.AddAssociatedFist("LeftFist");
                }
                else if (fist == rightFist)
                {
                    stuff.AddAssociatedFist("RightFist");
                }
                
            }
            if (statePre.IsGrabing_Stuff_StuffEnv() && !state.IsGrabing_Stuff_StuffEnv())
            {
                if (handGrabedStuff != null)
                {
                    GameObject stuffObject = handGrabedStuff;
                    handGrabedStuff = null;

                    var stuff = stuffObject.GetComponent<Stuff.Stuff>();
                    if (fist == leftFist)
                    {
                        stuff.RemoveAssociatedFist("LeftFist");
                    }
                    else if (fist == rightFist)
                    {
                        stuff.RemoveAssociatedFist("RightFist");
                    }

                    if (stuff is Stuff.Sword)
                    {
                        if (fist == leftFist)
                        {
                            ((Stuff.Sword)stuff).ThrowSword(fistVelocity.left.speed, fistOffset.leftDirOf9History);
                        }
                        else if (fist == rightFist)
                        {
                            ((Stuff.Sword)stuff).ThrowSword(fistVelocity.right.speed, fistOffset.rightDirOf9History);
                        }
                        
                    }
                }
                
            }
        }

        HandStuffUpdate_One(ref rightGrabedStuff, rightFistState, rightPre, rightFist, UniformInput.rMvDir);
        HandStuffUpdate_One(ref leftGrabedStuff, leftFistState, leftPre, leftFist, UniformInput.lMvDir);

    }

    void YDebugPanelShow_FUPre()
    {
        Y.DebugPanel.Log(_message: (FistState)leftFistState, _name: "LeftFistState", _category: "Player");
        Y.DebugPanel.Log(_message: (FistState)rightFistState, _name: "RightFistState", _category: "Player");
        Y.DebugPanel.Log(_message: (FootState)footState, _name: "FootState", _category: "Player");
    }
}
