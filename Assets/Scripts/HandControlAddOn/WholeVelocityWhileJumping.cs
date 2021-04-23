using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeVelocityWhileJumping : Velocity
{
    public class Params
    {
        public FistStatePlus leftFistState { get; set; }
        public FistStatePlus rightFistState { get; set; }
        public FootStatePlus footState { get; set; }
    }

    private float gravity;
    private float friciton;
    private float speedDownPartMax;
    private float wholeFistSpeedRatio;
    private float speedCutTime;
    private float historyTime;
    private float fastHistoryTime;
    private string log;
    public WholeVelocityWhileJumping(
        float gravity, float friction, float speedDownPartMax, float wholeFistSpeedRatio,
        float speedCutTime, float historyTime, float fastHistoryTime)
    {
        this.gravity = gravity;
        this.friciton = friction;
        this.speedDownPartMax = speedDownPartMax;
        this.wholeFistSpeedRatio = wholeFistSpeedRatio;

        this.speedCutTime = speedCutTime;
        this.historyTime = historyTime;
        this.fastHistoryTime = fastHistoryTime;
        
    }

    public void StartJumpWithLog(WholeVelocityBeforeJump velocityBeforeJump, WholeOffset offset)
    {
        log = "--StartJump\n";

        StartJump(velocityBeforeJump, offset);
        
        log += "    " + $"TimeNow: {Time.fixedTime}\n";
        log += "    " + "------Fast History------\n";
        foreach (var text in velocityBeforeJump.history)
        {
            if (Time.fixedTime - text.time <= fastHistoryTime)
            {
                log += "    " + $"time: {text.time}, dir:{text.direction}\n";
            }
        }
        log += "    " + "------Other History------\n";
        foreach (var text in velocityBeforeJump.history)
        {
            if (Time.fixedTime - text.time > fastHistoryTime && Time.fixedTime - text.time <= historyTime)
            {
                log += "    " + $"time: {text.time}, dir:{text.direction}\n";
            }
        }
        log += $"--StartJump End\n";
        MyLog.Log(log);
    }

    private void StartJump(WholeVelocityBeforeJump velocityBeforeJump, WholeOffset offset)
    {
        bool allZeroInCutTime = true;
        foreach (WholeVelocityBeforeJump.DirectionHistory history in velocityBeforeJump.history)
        {
            if (Time.fixedTime - history.time <= speedCutTime)
            {
                if (history.direction.magnitude != 0)
                {
                    allZeroInCutTime = false;
                    break;
                }
            }
        }
        if (allZeroInCutTime)
        {
            speed = 0f;
            direction = new Vector2();
            log += "    " + "JumpStyle: Zero\n";
            return;
        }

        //-----------------------------------------------------------
        bool allSameAndNotZeroInFastHistory = false;
        Vector2 directionByFastHistory = new Vector2();
        bool directionFound = false;
        foreach (WholeVelocityBeforeJump.DirectionHistory history in velocityBeforeJump.history)
        {
            if (Time.fixedTime - history.time <= fastHistoryTime)
            {
                if (history.direction.magnitude != 0)
                {
                    if (!directionFound)
                    {
                        allSameAndNotZeroInFastHistory = true;
                        directionByFastHistory = history.direction;
                        directionFound = true;
                    }
                    else
                    {
                        if (history.direction != directionByFastHistory)
                        {
                            allSameAndNotZeroInFastHistory = false;
                        }
                    }
                }
            }
        }
        if (allSameAndNotZeroInFastHistory && directionByFastHistory.magnitude != 0f)
        {
            speed = velocityBeforeJump.speed * wholeFistSpeedRatio;
            direction = directionByFastHistory;
            log += "    " + "JumpStyle: Fast History\n";
            return;
        }

        //-----------------------------------------------------------
        //把8个方向放在这里，然后数count。用vector2 的 dictionary，但是我搜了搜，需要实现GetHashCode和Equals，就先不搞了。
        List<Vector2> possibleDirections = new List<Vector2>();
        possibleDirections.Add(new Vector2(1, 0).normalized);
        possibleDirections.Add(new Vector2(-1, 0).normalized);
        possibleDirections.Add(new Vector2(0, 1).normalized);
        possibleDirections.Add(new Vector2(0, -1).normalized);
        possibleDirections.Add(new Vector2(1, 1).normalized);
        possibleDirections.Add(new Vector2(1, -1).normalized);
        possibleDirections.Add(new Vector2(-1, 1).normalized);
        possibleDirections.Add(new Vector2(-1, -1).normalized);
        List<int> count = new List<int>();
        foreach (Vector2 dir in possibleDirections) count.Add(0);

        foreach (WholeVelocityBeforeJump.DirectionHistory history in velocityBeforeJump.history)
        {
            if (Time.fixedTime - history.time  <= historyTime)
            {
                for (int i = 0; i < possibleDirections.Count; i++)
                {
                    if (possibleDirections[i] == history.direction)
                    {
                        count[i]++;
                    }
                }
            }
        }

        int maxCount = 0;
        Vector2 maxCountDirection = new Vector2();
        for (int i = 0; i < count.Count; i++)
        {
            if (count[i] > maxCount)
            {
                maxCount = count[i];
                maxCountDirection = possibleDirections[i];
            }
        }
        
        //可能出现两个方向的Count相同。不处理。根据代码选到谁就是谁啦。

        if (maxCount > 0)
        {
            speed = velocityBeforeJump.speed * wholeFistSpeedRatio;
            direction = maxCountDirection;
            log += "    " + "JumpStyle: All History\n";
        }
        else
        {
            speed = 0f;
            direction = new Vector2();
            //因为前面已经检查过，在speedCutTime内，不都为0了。所以，这里检查historyTime,不应该出现全为0的情况。
            Debug.LogError("In WholeVelocitywhileJumping.StartJump, Search All History, all history should not be all zero.");
        }
    }
    public void FixedUpdateManually(Params @params)
    {
        FistStatePlus leftFistState = @params.leftFistState;
        FistStatePlus rightFistState = @params.rightFistState;
        FootStatePlus footState = @params.footState;
        if (leftFistState == FistState.GrabEnv || rightFistState == FistState.GrabEnv)
        {
            Debug.LogError("WholeVelocityWhileJumping should not be called when fist grabing");
            return;
        }

        //1.重力加速度
        float deltaSpeed = gravity * Time.fixedDeltaTime;
        Vector2 deltaVelocity = deltaSpeed * Vector2.down;
        Vector2 velocityNew = velocity + deltaVelocity;
        float velocityNewRightPart = Vector2.Dot(velocityNew, Vector2.right);
        float velocityNewDownPart = Vector2.Dot(velocityNew, Vector2.down);
        if (velocityNewDownPart >=0 && velocityNewDownPart > speedDownPartMax)
        {
            velocityNewDownPart = speedDownPartMax;
        }
        velocityNew = velocityNewRightPart * Vector2.right + velocityNewDownPart * Vector2.down;
        
        speed = velocityNew.magnitude;
        direction = velocityNew.normalized;


        //2 根据速度方向和footState来更改速度
        float velocityDownPart = Vector2.Dot(velocity, Vector2.down);
        //2.1 向上运动
        if (velocityDownPart < 0)
        {
            return;
        }
        //2.2 向下或水平运动
        if (footState == FootState.Surface || footState == FootState.EnvGround)
        {
            /*
            //变为水平运动
            float velocityRightPart = Vector2.Dot(velocity, Vector2.right);
            speed = Mathf.Abs(velocityRightPart);
            direction = velocityRightPart>=0 ? Vector2.right:Vector2.left;
            //水平方向摩擦力减速
            float speedNew = speed - friciton * Time.fixedDeltaTime;
            speed = Mathf.Clamp(speedNew, 0f, float.MaxValue);
            */

            //速度变为0
            speed = 0f;
            direction = new Vector2();

        }
        else if (footState == FootState.Air)
        {

        }
        else if (footState == FootState.EnvRock)
        {

        }
    }
}