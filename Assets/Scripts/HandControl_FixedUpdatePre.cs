using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HandControl : MonoBehaviour
{
    void ___________________________()
    {

    }
    void FU_Pre()
    {
        /*
            FistState
            FootState
         
         */
        rightFistState.FixedUpdateManually(rightFist, HKey.rAlt);
        leftFistState.FixedUpdateManually(leftFist, HKey.lAlt);
        footState.FixedUpdateManually(bottomLeftPoint: bottomLeftPoint, bottomRightPoint: bottomRightPoint);

        HandStuffFixedUpdate(rightFistState.pre, leftFistState.pre);

        Y.DebugPanel.Log("LeftFistState", "HandControl", (FistState)leftFistState);
        Y.DebugPanel.Log("RightFistState", "HandControl", (FistState)rightFistState);
    }

    void HandStuffFixedUpdate(FistState rightPre, FistState leftPre)
    {
        void HandStuffUpdate_One(ref GameObject handGrabedStuff, FistState state, FistState statePre, GameObject fist, Vector2 mvDir)
        {
            LayerMask LMStuff = LayerMask.GetMask("Stuff");
            if (statePre != FistState.GrabStuff && state == FistState.GrabStuff)
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
                stuff.fist = fist;
            }
            if (statePre == FistState.GrabStuff && state != FistState.GrabStuff)
            {
                GameObject stuffObject = handGrabedStuff;
                handGrabedStuff = null;

                var stuff = stuffObject.GetComponent<Stuff.Stuff>();
                stuff.fist = null;
            }
        }

        HandStuffUpdate_One(ref rightGrabedStuff, rightFistState, rightPre, rightFist, HKey.rMvDir);
        HandStuffUpdate_One(ref leftGrabedStuff, leftFistState, leftPre, leftFist, HKey.lMvDir);

    }
}
