﻿#define TEST_RIGHT_LINE
#undef TEST_RIGHT_LINE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HKey
{
    public class DebugMod
    {
        public bool lAltAlways = false;
        public bool rAltAlways = false;
    }

    static public bool debugModOn = false;
    static public DebugMod debugMod = new DebugMod();

    static public bool rUp;
    static public bool rDown;
    static public bool rLeft;
    static public bool rRight;
    static public bool rAlt;

    static public bool lUp;
    static public bool lDown;
    static public bool lLeft;
    static public bool lRight;
    static public bool lAlt;

    static public int rMvRight;
    static public int rMvUp;
    static public Vector2 rMvDir;
    static public int lMvRight;
    static public int lMvUp;
    static public Vector2 lMvDir;

    static public void UpdateRefresh()
    {
        if (debugModOn)
        {
            DebugModUpdateRefresh();
            return;
        }

        Key();
        Direction();
    }

    static private void DebugModUpdateRefresh()
    {
        Key();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            debugMod.lAltAlways = !debugMod.lAltAlways;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            debugMod.rAltAlways = !debugMod.rAltAlways;
        }
        if (debugMod.lAltAlways)
            lAlt = true;
        if (debugMod.rAltAlways)
            rAlt = true;
        Direction();
    }

    static private void Key()
    {
        rUp = Input.GetKey(KeyCode.O);
        rDown = Input.GetKey(KeyCode.L);
        rLeft = Input.GetKey(KeyCode.K);
        rRight = Input.GetKey(KeyCode.Semicolon);
        rAlt = Input.GetKey(KeyCode.RightAlt);

        lUp = Input.GetKey(KeyCode.W);
        lDown = Input.GetKey(KeyCode.S);
        lLeft = Input.GetKey(KeyCode.A);
        lRight = Input.GetKey(KeyCode.D);
        lAlt = Input.GetKey(KeyCode.LeftAlt);

    }

    static private void Direction()
    {
        rMvRight = (rRight ? 1 : 0) - (rLeft ? 1 : 0);
        rMvUp = (rUp ? 1 : 0) - (rDown ? 1 : 0);
        rMvDir = new Vector2(rMvRight, rMvUp).normalized;
        lMvRight = (lRight ? 1 : 0) - (lLeft ? 1 : 0);
        lMvUp = (lUp ? 1 : 0) - (lDown ? 1 : 0);
        lMvDir = new Vector2(lMvRight, lMvUp).normalized;
    }



}