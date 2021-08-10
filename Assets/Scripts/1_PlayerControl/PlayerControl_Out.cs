using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerControl : MonoBehaviour
{
    public class ValueForSword
    {
        public Vector2 handOffset;
        public Vector2 wholeOffset;
        public GameObject fistObject;
    }
    public ValueForSword ForSword_GiveValue(string fist)
    {
        if (fist != "LeftFist" && fist != "RightFist")
            throw new System.Exception("fist should be LeftFist or RightFist");
        ValueForSword value = new ValueForSword();
        if (fist == "LeftFist")
        {
            value.handOffset = fistOffset.left;
            value.fistObject = leftFist;
        }
        if (fist == "RightFist")
        {
            value.handOffset = fistOffset.right;
            value.fistObject = rightFist;
        }
        value.wholeOffset = wholeOffset.offset;
        return value;
    }

    public bool ForSword_CheckOverlap(string fist, GameObject stuffObject)
    {
        if (fist != "LeftFist" && fist != "RightFist")
            throw new System.Exception("fist should be LeftFist or RightFist");
        GameObject fistObj = null;
        if (fist == "LeftFist")
        {
            fistObj = leftFist;
        }
        if (fist == "RightFist")
        {
            fistObj = rightFist;
        }

        LayerMask LMStuff = LayerMask.GetMask("Stuff");
        Collider2D fistCollider = fistObj.GetComponent<Collider2D>();
        Collider2D[] result = new Collider2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LMStuff);
        filter.SetDepth(stuffObject.transform.position.z, stuffObject.transform.position.z);
        int count = Physics2D.OverlapCollider
                (fistCollider, filter, result);

        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public void ForSword_UnSetGrabedStuff(string fist)
    {
        if (fist != "LeftFist" && fist != "RightFist")
            throw new System.Exception("fist should be LeftFist or RightFist");
        if (fist == "LeftFist")
        {
            leftGrabedStuff = null;
        }
        if (fist == "RightFist")
        {
            rightGrabedStuff = null;
        }
    }

    public void Out_SetMoveDirectionReverseIfGrabEnv(int value)
    {
        moveDirectionReverseIfGrabEnv = value;
    }

    public int Out_GetMoveDirectionReverseIfGrabEnv()
    {
        return moveDirectionReverseIfGrabEnv;
    }
}
