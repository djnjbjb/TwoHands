using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludo;

public class WholeOffset
{
    public Vector2 offset 
    { 
        get
        {
            return _offset;
        }
        set
        {
            _offset = value;
            if (Utility.FloatEqual_WithIn0p001(_offset.magnitude, 0f))
            {
                offsetZeroTime += Time.fixedDeltaTime;
            }
            else
            {
                offsetZeroTime = 0f;
            }
        }
    }
    private Vector2 _offset;
    public float offsetZeroTime { get; private set; }

    public WholeOffset()
    {
        offset = new Vector2();
        offsetZeroTime = 0f;
    }
}
