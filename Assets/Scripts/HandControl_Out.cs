using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HandControl : MonoBehaviour
{
    public class ValueForSword
    {
        public Vector2 offset;
    }
    public ValueForSword GetValueForSword(GameObject fist)
    {
        ValueForSword value = new ValueForSword();
        if (fist == leftFist)
        {
            value.offset = fistOffset.left;
        }
        if (fist == rightFist)
        {
            value.offset = fistOffset.right;
        }
        return value;
    }
}
