using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerControlStaticSetting
{
    static int moveDirectionReverseIfGrabEnv = 1;

    public static int GetMoveDirectionReverseIfGrabEnv()
    {
        return moveDirectionReverseIfGrabEnv;
    }

    public static bool ForUI_GetMoveDirectionReverseIfGrabEnv()
    {
        if (moveDirectionReverseIfGrabEnv == 1)
        {
            return false;
        }
        else if (moveDirectionReverseIfGrabEnv == -1)
        {
            return true;
        }
        else
        {
            throw new System.Exception("Error");
        }
    }

    public static void SetMoveDirectionReverseIfGrabEnv(bool value)
    {
        if (value)
        {
            moveDirectionReverseIfGrabEnv = -1;
        }
        else
        {
            moveDirectionReverseIfGrabEnv = 1;
        }

        if (PlayerControl.playerControl != null)
        {
            PlayerControl.playerControl.Out_SetMoveDirectionReverseIfGrabEnv(moveDirectionReverseIfGrabEnv);
        }
    }
}
