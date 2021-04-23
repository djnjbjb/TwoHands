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
    }

    void HandStuffFixedUpdate(FistState rightPre, FistState leftPre)
    {
        void HandStuffUpdate_One(ref GameObject handGrabedStuff, FistState state, FistState statePre, GameObject fist, Vector2 mvDir)
        {
            /*
                把stuff变成hand的子节点，这样可以省事
                对stuff的collider进行处理 - 添加/删除。暂时先这么做。
            */
            LayerMask LMStuff = LayerMask.GetMask("Stuff");
            if (statePre != FistState.GrabStuff && state == FistState.GrabStuff)
            {
                var colliderStuff = Physics2D.OverlapCircle(fist.transform.position,
                    fist.transform.lossyScale.x / 2, LMStuff);
                //这个collider，按理说一定存在
                if (!colliderStuff)
                    throw new System.Exception("collider == null");

                GameObject stuff = colliderStuff.gameObject;
                Destroy(colliderStuff);

                stuff.transform.SetParent(fist.transform);
                handGrabedStuff = stuff;
            }
            if (statePre == FistState.GrabStuff && state != FistState.GrabStuff)
            {
                GameObject stuff = handGrabedStuff;
                handGrabedStuff = null;
                stuff.transform.parent = null;
                stuff.AddComponent<PolygonCollider2D>();

                stuff.GetComponent<Stuff>().speed = 12f;
                stuff.GetComponent<Stuff>().direction = mvDir;
            }
        }

        HandStuffUpdate_One(ref rightGrabedStuff, rightFistState, rightPre, rightFist, HKey.rMvDir);
        HandStuffUpdate_One(ref leftGrabedStuff, leftFistState, leftPre, leftFist, HKey.lMvDir);

    }
}
