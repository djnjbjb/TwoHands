using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HandControl : MonoBehaviour
{
    void FU_Pre()
    {
        VariableUpdate();

        HandState rightPre = rightFistState;
        HandState leftPre = leftFistState;

        HandStateUpdate(rightPre, leftPre);
        HandStuffUpdate(rightPre, leftPre);

        fistVelocity.RefreshSpeed();

        Yurowm.DebugTools.DebugPanel.Log("rightSpeed", "Speed", fistVelocity.right.speed);
        Yurowm.DebugTools.DebugPanel.Log("leftSpeed", "Speed", fistVelocity.left.speed);
    }

    void VariableUpdate()
    {
        wholeMatrix = Matrix4x4.TRS(whole.transform.position, whole.transform.localRotation, whole.transform.localScale);
    }

    void HandStateUpdate(HandState rightPre, HandState leftPre)
    {
        LayerMask LMStuff = LayerMask.GetMask("Stuff");
        LayerMask LMEnv = LayerMask.GetMask("EnvRock", "EnvGround");
        HandState GetHandState(GameObject fist, bool altPressed, HandState statePre)
        {
            if (!altPressed)
                return HandState.Free;

            //GrabStuff的优先级较高。比Env之类的高。
            if (statePre == HandState.GrabStuff && altPressed)
                return HandState.GrabStuff;
            var colliderStuff = Physics2D.OverlapCircle(fist.transform.position,
                fist.transform.lossyScale.x / 2, LMStuff);
            if (colliderStuff)
                return HandState.GrabStuff;

            var colliderEnv = Physics2D.OverlapCircle(fist.transform.position,
               fist.transform.lossyScale.x / 2, LMEnv);
            if (colliderEnv)
                return HandState.GrabEnv;

            return HandState.GrabNothing;
        }
        rightFistState = GetHandState(rightFist, HKey.rAlt, rightPre);
        leftFistState = GetHandState(leftFist, HKey.lAlt, leftPre);
    }

    void HandStuffUpdate(HandState rightPre, HandState leftPre)
    {
        void HandStuffUpdate_One(ref GameObject handGrabedStuff, HandState state, HandState statePre, GameObject fist, Vector2 mvDir)
        {
            /*
                把stuff变成hand的子节点，这样可以省事
                对stuff的collider进行处理 - 添加/删除。暂时先这么做。
            */
            LayerMask LMStuff = LayerMask.GetMask("Stuff");
            if (statePre != HandState.GrabStuff && state == HandState.GrabStuff)
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
            if (statePre == HandState.GrabStuff && state != HandState.GrabStuff)
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
