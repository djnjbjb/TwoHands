using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HandControl : MonoBehaviour
{
    public class ValueForSword
    {
        public Vector2 handOffset;
        public Vector2 wholeOffset;
    }
    public ValueForSword GetValueForSword(GameObject fist)
    {
        ValueForSword value = new ValueForSword();
        if (fist == leftFist)
        {
            value.handOffset = fistOffset.left;
        }
        if (fist == rightFist)
        {
            value.handOffset = fistOffset.right;
        }
        value.wholeOffset = wholeOffset.offset;
        return value;
    }
}
