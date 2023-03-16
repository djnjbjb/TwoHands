using Ludo.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Ludo.TwoHandsWar.Circumstance.Logger;

public class WholeVelocityWhileJumping : Velocity
{
    public class Params
    {
        public FistStatePlus leftFistState { get; set; }
        public FistStatePlus rightFistState { get; set; }
        public FootStatePlus footState { get; set; }
    }
    public enum StartJump_Direction_Type
    {
        ZeroCut,
        SimpleHistory,
        ComplexHistory
    }

    private float gravity;
    private float friction;
    private float speedDownPartMax;
    private float wholeFistSpeedRatio;
    private float historyTime;
    private float historyZeroCutTime;
    private float historySimpleHistoryTime;
    private float historyComplexHistoryTime;
    private bool jumpSpeedAddedWhenSlanting;
    private string startJumpLog;
    public WholeVelocityWhileJumping(
        float gravity, float friction, float speedDownPartMax, float wholeFistSpeedRatio,
        float historyTime, float historyZeroCutTime, float historySimpleHistoryTime, float historyComplexHistoryTime,
        bool jumpSpeedAddedWhenSlanting)
    {
        this.gravity = gravity;
        this.friction = friction;
        this.speedDownPartMax = speedDownPartMax;
        this.wholeFistSpeedRatio = wholeFistSpeedRatio;

        this.historyTime = historyTime;
        this.historyZeroCutTime = historyZeroCutTime;
        this.historySimpleHistoryTime = historySimpleHistoryTime;
        this.historyComplexHistoryTime = historyComplexHistoryTime;
        this.jumpSpeedAddedWhenSlanting = jumpSpeedAddedWhenSlanting;
        
    }

    public void StartJumpWithLog(WholeVelocityBeforeJump velocityBeforeJump, WholeOffset offset)
    {
        startJumpLog = "--StartJump\n";

        StartJump(velocityBeforeJump, offset);
        
        startJumpLog += "    " + $"TimeNow: {Time.fixedTime}\n";
        startJumpLog += "    " + "------History------\n";
        for (int i = velocityBeforeJump.history.Count - 1; i >= 0; i--)
        {
            var history = velocityBeforeJump.history[i];
            startJumpLog += $"    Time: {history.time}, Dir:{history.direction}, Speed:{history.speed}\n";
        }
        startJumpLog += $"--StartJump End\n";
        Logger.Log(startJumpLog);
    }

    private void StartJump(WholeVelocityBeforeJump velocityBeforeJump, WholeOffset offset)
    {
        StartJump_Direction_Type type;
        Vector2 direction;
        (direction, type) = StartJump_GetDirection(velocityBeforeJump);
        float speed = StartJump_GetSpeed(velocityBeforeJump, type, out float lastNoZeroSpeed, out float deltaTime);
        this.speed = speed;
        this.direction = direction;
        StartJump_SpeedAddedWhenSlanting();

        if (type == StartJump_Direction_Type.ZeroCut)
        {
            startJumpLog += "    " + "JumpStyle: Zero\n";
        }
        else if (type == StartJump_Direction_Type.SimpleHistory)
        {
            startJumpLog += "    " + "JumpStyle: Simple History\n";
        }
        else if (type == StartJump_Direction_Type.ComplexHistory)
        {
            startJumpLog += "    " + "JumpStyle: Complex History\n";
        }
        startJumpLog += "    " + $"JumpDirection:{this.direction}\n";
        startJumpLog += "    " + $"JumpSpeed:{this.speed}\n";
        startJumpLog += "    " + $"LastNoZeroSpeed:{lastNoZeroSpeed}\n";
        startJumpLog += "    " + $"DeltaTime:{deltaTime}\n";
    }

    private (Vector2 direction, StartJump_Direction_Type type) StartJump_GetDirection(WholeVelocityBeforeJump velocityBeforeJump)
    {

        StartJump_Direction_Type type = StartJump_Direction_Type.ZeroCut;
        Vector2 directionResult = new Vector2();

        //-------------------------ZeroCut-------------------------//
        int LastNoZeroDirectionIndex = 0;
        float lastNoZeroTime = 0f;
        {
            bool allZeroInCutTime = true;
            for (int i = velocityBeforeJump.history.Count - 1; i >= 0; i--)
            {
                var history = velocityBeforeJump.history[i];
                if (Time.fixedTime - history.time <= historyZeroCutTime)
                {
                    if (history.direction.magnitude != 0)
                    {
                        allZeroInCutTime = false;
                        LastNoZeroDirectionIndex = i;
                        lastNoZeroTime = history.time;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (allZeroInCutTime)
            {
                type = StartJump_Direction_Type.ZeroCut;
                directionResult = Vector2.zero;
                goto End;
            }
        }
        /*
            之后LastNoZeroDirectionIndex、一定有效。
            因为，如果不存在，前面已经goto最后了。
        */

        //----------------------------SimpleHistory-------------------------------//
        bool allSameAndNotZeroInSimpleHistory = false;
        Vector2 directionBySimpleHistory = Vector2.zero;
        bool directionFound = false;
        for (int i = LastNoZeroDirectionIndex; i >= 0; i--)
        {
            var history = velocityBeforeJump.history[i];
            if (lastNoZeroTime - history.time <= historySimpleHistoryTime)
            {
                if (history.direction.magnitude != 0)
                {
                    if (!directionFound)
                    {
                        directionFound = true;

                        allSameAndNotZeroInSimpleHistory = true;

                        directionBySimpleHistory = history.direction;
                        
                    }
                    else
                    {
                        if (history.direction != directionBySimpleHistory)
                        {
                            allSameAndNotZeroInSimpleHistory = false;
                        }
                    }
                }
            }
            else
            {
                break;
            }
        }
        if (allSameAndNotZeroInSimpleHistory)
        {
            type = StartJump_Direction_Type.SimpleHistory;
            directionResult = directionBySimpleHistory;
            goto End;
        }

        //----------------------------ComplexHistory-------------------------------//
        //把8个方向放在这里，然后数count。可以用 vector2 的 dictionary，但是我搜了搜，需要实现GetHashCode和Equals，就先不搞了。
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

        for (int i = LastNoZeroDirectionIndex; i >= 0; i--)
        {
            var history = velocityBeforeJump.history[i];
            if (lastNoZeroTime - history.time <= historyComplexHistoryTime)
            {
                for (int j = 0; j < possibleDirections.Count; j++)
                {
                    if (possibleDirections[j] == history.direction)
                    {
                        count[j]++;
                    }
                }
            }
            else
            {
                break;
            }
        }

        int maxCount = 0;
        Vector2 maxCountDirection = Vector2.zero;
        for (int i = 0; i < count.Count; i++)
        {
            if (count[i] > maxCount)
            {
                maxCount = count[i];
                maxCountDirection = possibleDirections[i];
            }
        }

        //可能出现两个方向的Count相同。不处理。代码选到谁就是谁啦。
        if (maxCount > 0)
        {
            type = StartJump_Direction_Type.ComplexHistory;
            directionResult = maxCountDirection;
            goto End;
        }
        else
        {
            speed = 0f;
            directionResult = Vector2.zero;
            //因为前面已经检查过，有LastNoZeroDirectionIndex，所以，不可能出现directon结果为0的情况
            Debug.LogError("In WholeVelocitywhileJumping.StartJump, Search All History, all history should not be all zero.");
        }

        //---------------------------End---------------------------//
        End:
        return (direction: directionResult, type: type);
    }

    private float StartJump_GetSpeed(WholeVelocityBeforeJump velocityBeforeJump, StartJump_Direction_Type type, out float lastNoZeroSpeedOut, out float deltaTimeOut)
    {
        float speedEqualToBeforeJump = StartJump_GetSpeed_EqualToBeforeJump(velocityBeforeJump, type, out lastNoZeroSpeedOut, out deltaTimeOut);
        float speedReturn = StartJump_GetSpeed_FromEqualToBeforeJump_ToSpeed(speedEqualToBeforeJump, type);
        return speedReturn;
    }

    private float StartJump_GetSpeed_EqualToBeforeJump(WholeVelocityBeforeJump velocityBeforeJump, StartJump_Direction_Type type,
                                                      out float lastNoZeroSpeedOut, out float deltaTimeOut)
    {
        lastNoZeroSpeedOut = 0f;
        deltaTimeOut = 0f;


        float speedReturn = 0f;
        if (type == StartJump_Direction_Type.ZeroCut)
        {
            speedReturn = 0f;

            lastNoZeroSpeedOut = 0f;
            deltaTimeOut = 0f;
        }
        else if (type == StartJump_Direction_Type.SimpleHistory || type == StartJump_Direction_Type.ComplexHistory)
        {
            //1，获取  velocityBeforeJump，或其减速了的形式
            int LastNoZeroDirectionIndex = 0;
            float lastNoZeroTime = 0f;
            {
                bool allZeroInCutTime = true;
                for (int i = velocityBeforeJump.history.Count - 1; i >= 0; i--)
                {
                    var history = velocityBeforeJump.history[i];
                    if (Time.fixedTime - history.time <= historyZeroCutTime)
                    {
                        if (history.direction.magnitude != 0)
                        {
                            allZeroInCutTime = false;
                            LastNoZeroDirectionIndex = i;
                            lastNoZeroTime = history.time;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (allZeroInCutTime)
                {
                    throw new System.Exception("找不到LastNoZeroDirectionIndex，肯定有问题呀");
                }
            }
            float lastNoZeroSpeed = velocityBeforeJump.history[LastNoZeroDirectionIndex].speed;
            float deltaTime = Time.fixedTime - lastNoZeroTime;
            deltaTime = Mathf.Max(0, deltaTime - Time.fixedDeltaTime);  //因为跳跃总是比History晚一帧，而这一帧应该忽略。
            speedReturn = lastNoZeroSpeed * Mathf.Pow((1 - 1 / 0.16f * Mathf.Pow(deltaTime, 2)), 0.5f);
            /*
                不考虑代码环境。这里可能出现   1 - 1/0.16f * Mathf.Pow(deltaTime,2) 小于零的情况。
                就会出exception。
                就让它出exception。
            */


            lastNoZeroSpeedOut = lastNoZeroSpeed;
            deltaTimeOut = deltaTime;
        }
        return speedReturn;
    }

    private float StartJump_GetSpeed_FromEqualToBeforeJump_ToSpeed(float speedEqualToBeforeJump, StartJump_Direction_Type type)
    {
        float speedReturn = 0f;
        if (type == StartJump_Direction_Type.ZeroCut)
        {
            speedReturn = 0f;
        }
        else if (type == StartJump_Direction_Type.SimpleHistory || type == StartJump_Direction_Type.ComplexHistory)
        {
            speedReturn = speedEqualToBeforeJump * wholeFistSpeedRatio;
            
        }
        return speedReturn;
    }

    private void StartJump_SpeedAddedWhenSlanting()
    {
        if (jumpSpeedAddedWhenSlanting)
        {
            //斜向时，速度乘以 √2
            if (!Geometry.FloatEqual_WithIn0p001(direction.x, 0)
                 && !Geometry.FloatEqual_WithIn0p001(direction.y, 0))
            {
                Vector2 velocity = speed * direction;
                velocity.y = velocity.y * Mathf.Pow(2, 0.5f);
                speed = velocity.magnitude;
                direction = velocity.normalized;
            }
        }

    }

    public (Vector2 direction, StartJump_Direction_Type type) Out_GetDirection(WholeVelocityBeforeJump velocityBeforeJump)
    {
        return StartJump_GetDirection(velocityBeforeJump);
    }

    public float Out_GetSpeed_EqualToBeforeJump(WholeVelocityBeforeJump velocityBeforeJump, StartJump_Direction_Type type)
    {
        return StartJump_GetSpeed_EqualToBeforeJump(velocityBeforeJump, type, out _, out _);
    }

    public void FixedUpdateManually(Params @params)
    {
        FistStatePlus leftFistState = @params.leftFistState;
        FistStatePlus rightFistState = @params.rightFistState;
        FootStatePlus footState = @params.footState;
        if (leftFistState.IsGrabing_Env_StuffEnv() || rightFistState.IsGrabing_Env_StuffEnv())
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


        //2.根据速度方向和footState来更改速度
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