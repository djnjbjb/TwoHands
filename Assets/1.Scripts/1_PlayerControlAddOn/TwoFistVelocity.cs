using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludo;
using PlayerControlTool;

public class TwoFistVelocity
{
    public FistVelocity left;
    public FistVelocity right;
    public DirectionOf9History leftHistoryOfNineDir = new PlayerControlTool.DirectionOf9History();
    public DirectionOf9History rightHistoryOfNineDir = new PlayerControlTool.DirectionOf9History();

    public TwoFistVelocity(float length)
    {
        left = new FistVelocity(length);
        right = new FistVelocity(length);
    }

    public void RefreshSpeed(ParameterForFistVelocity leftParameter, ParameterForFistVelocity rightParameter)
    {
        /*
            如果双手都是GrabEnv，就要双手协同
            单手比较简单，用以前的方式。
        */
        if (leftParameter.handState == FistState.GrabEnv && rightParameter.handState == FistState.GrabEnv)
        {
            /*
                如果双手同向，则双手都按照RefreshSpeedTwoFist来加速。
                否则，有多种情况。
                    双手方向不同。
                    一只手有方向，一只手方向为0
                    双手方向都为0
                都根据一只手的情况计算就可以了。
             */
            if ( (leftParameter.moveDir == rightParameter.moveDir) && leftParameter.moveDir.magnitude != 0)
            {
                float speedBigger = Mathf.Max(left.speed, right.speed);
                left.AccelerateSpeedTwoFistGrabed(leftParameter, speedBigger);
                right.AccelerateSpeedTwoFistGrabed(rightParameter, speedBigger);
                return;
            }
            else
            {
                goto OneHandCase;
            }
        }
        OneHandCase:
        {
            left.RefreshSpeedOneFist(leftParameter);
            right.RefreshSpeedOneFist(rightParameter);
        }
    }

    public void OffsetReflectSpeed(ParameterForFistVelocity leftParameter, Vector2 leftOffset, 
                                   ParameterForFistVelocity rightParameter, Vector2 rightOffset)
    {
        /*
            双手加速，需考虑offset
            双手加速的情况：双手 + 同向移动 + 不为0
         */
        if (leftParameter.handState == FistState.GrabEnv && rightParameter.handState == FistState.GrabEnv)
        {
            if (  (leftParameter.moveDir == rightParameter.moveDir)
                  && leftParameter.moveDir.magnitude != 0            )
            {
                if (  Utility.FloatEqual_WithIn0p001(leftOffset.magnitude, 0f)
                      && Utility.FloatEqual_WithIn0p001(rightOffset.magnitude, 0f) )
                {
                    left.OffsetZeroTwoHand();
                    right.OffsetZeroTwoHand();
                    return;
                }
            }
            else
            {
                goto NormalState;
            }
        }

    /*
        单手加速，需考虑offset
        单手加速的情况：
            1. 只有一个或0个GrabEnv，mvDir不为0 -> 在下面有条件判断
            2. 两个都GrabEnv，但是非双手加速，MvDir不为0 -> 在下面有条件判断，由前面的goto跳转而来
        增加一条，速度必须小于单手最大速度，才加速
    */
    NormalState:
        if (Utility.FloatEqual_WithIn0p001(leftOffset.magnitude, 0f))
        {
            if (leftParameter.moveDir.magnitude != 0)
            {
                if (left.IsSpeedPreNoBiggerThanMaxOneFist())
                {
                    left.OffsetZeroOneHand();
                }
                
            }
        }

        if (Utility.FloatEqual_WithIn0p001(rightOffset.magnitude, 0f))
        {
            if (rightParameter.moveDir.magnitude != 0)
            {
                if (right.IsSpeedPreNoBiggerThanMaxOneFist())
                {
                    right.OffsetZeroOneHand();
                }
            }
        }
    }

    public void FixedUpdateHistoryManually()
    {
        leftHistoryOfNineDir.FixedUpdateManually(Time.fixedTime, left.direction.normalized);
        rightHistoryOfNineDir.FixedUpdateManually(Time.fixedTime, right.direction.normalized);
    }
}
