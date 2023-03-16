using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerControlTool;
using Ludo.Utility;

public class WholeVelocityBeforeJump: Velocity
{
    public struct History
    {
        public float time;
        public Vector2 direction;
        public float speed;
    }

    public class Params
    {
        public FistStatePlus leftFistState { get; set; }
        public FistStatePlus rightFistState { get; set; }
        public FistStatePlus leftFistState_GrabStuffEnvProcessed { get; set; }
        public FistStatePlus rightFistState_GrabStuffEnvProcessed { get; set; }
        public Velocity leftFistVelocity { get; set; }
        public Velocity rightFistVelocity { get; set; }
        public Vector2 leftFistOffset { get; set; }
        public Vector2 rightFistOffset { get; set; }
        public Matrix4x4 wholeMatrix { get; set; }
    }

    private float historyTime;
    public List<History> history;

    public WholeVelocityBeforeJump(float historyTime)
    {
        speed = 0f;
        direction = Vector2.zero;
        this.historyTime = historyTime;
        history = new List<History>();
    }

    public void FixedUpdateManually(Params @params)
    {
        /*
            加入GrabStuffEnv后的处理
            
            首先，params有了GrabStuffEnvProcessed。
            
            先把计算速度的分类讨论中，所有的state换成 state_GrabStuffEnvProcessed。
            这样，有GrabEnv的情况应该就没有问题了。
            接下来处理无GrabEnv的情况。
                处理时，需要看看速度的代码，看速度的定义是什么。
                ~~~~
                看了一下，可以为零。那就置为零了。 
        */

        /*
            身体的速度 - 大小，和fist一致
            身体的速度 - 方向，和offset一致。
                但是，只有9向，而不是所有方向。
                这涉及到宏观还是微观看身体移动方向的问题。
            (9向 = 8个方向 + 没有方向)
        */

        FistStatePlus leftFistState = @params.leftFistState;
        FistStatePlus rightFistState = @params.rightFistState;
        FistStatePlus leftFistState_GrabStuffEnvProcessed = @params.leftFistState_GrabStuffEnvProcessed;
        FistStatePlus rightFistState_GrabStuffEnvProcessed = @params.rightFistState_GrabStuffEnvProcessed;
        Velocity leftFistVelocity = @params.leftFistVelocity;
        Velocity rightFistVelocity = @params.rightFistVelocity;
        Vector2 leftFistOffset = @params.leftFistOffset;
        Vector2 rightFistOffset = @params.rightFistOffset;
        Matrix4x4 wholeMatrix = @params.wholeMatrix;

        if (leftFistState_GrabStuffEnvProcessed != FistState.GrabEnv && rightFistState_GrabStuffEnvProcessed != FistState.GrabEnv)
        {
            //这里一定是，1只手或2只手GrabStuffEnv、但等效于Stuff。
            speed = 0f;
            direction = new Vector2();

            if (leftFistState == FistState.GrabStuff && rightFistState == FistState.GrabStuff)
            {
                Debug.LogError("WholeVelocityBeforeJump should not be called while jumping");
            }
        }
        if (leftFistState_GrabStuffEnvProcessed == FistState.GrabEnv && rightFistState_GrabStuffEnvProcessed != FistState.GrabEnv)
        {
            speed = leftFistVelocity.speed;
            direction = Tool.ArbitraryDirectionToNineDirection(leftFistOffset);
            direction = wholeMatrix * -direction;
        }
        if (leftFistState_GrabStuffEnvProcessed != FistState.GrabEnv && rightFistState_GrabStuffEnvProcessed == FistState.GrabEnv)
        {
            speed = rightFistVelocity.speed;
            direction = Tool.ArbitraryDirectionToNineDirection(rightFistOffset);
            direction = wholeMatrix * -direction;
        }

        if (rightFistState_GrabStuffEnvProcessed == FistState.GrabEnv && leftFistState_GrabStuffEnvProcessed == FistState.GrabEnv)
        {
            /*
                在双手GrabEnv的情况下，有几种情况。
                1. 双手都有方向且同向
                    这是最典型的情况。也是用到主要算法：根据两只手的offset来计算。
                2. 有一只手有方向，一只手没方向。
                    这时，如果有方向的手的offset不为0，根据那只手的offset来计算即可。
                    但是，如果两只手的offset都为0，就需要选有方向那只手，需要补充算法那。
                3. 两只手都有方向，但方向不同
                    速度为0。
            */
            if (leftFistVelocity.direction.magnitude != 0
                && rightFistVelocity.direction.magnitude != 0
                && (leftFistVelocity.direction != rightFistVelocity.direction))
            {
                speed = 0f;
                direction = new Vector2();
            }

            else if (leftFistVelocity.direction.magnitude != 0
                     && rightFistVelocity.direction.magnitude == 0)
            {
                speed = leftFistVelocity.speed;
                direction = Tool.ArbitraryDirectionToNineDirection(leftFistOffset);
                direction = wholeMatrix * -direction;
            }
            else if (leftFistVelocity.direction.magnitude == 0
                     && rightFistVelocity.direction.magnitude != 0)
            {
                speed = rightFistVelocity.speed;
                direction = Tool.ArbitraryDirectionToNineDirection(rightFistOffset);
                direction = wholeMatrix * -direction;
            }
            else 
            {
                float rhDis = rightFistOffset.magnitude;
                float lhDis = leftFistOffset.magnitude;
                float smallerDis = 0;
                float biggerDis = 0;
                RightOrLeftFist biggerFist = RightOrLeftFist.Left;
                if (rhDis > lhDis)
                {
                    smallerDis = lhDis;
                    biggerDis = rhDis;
                    biggerFist = RightOrLeftFist.Right;
                }
                else
                {
                    smallerDis = rhDis;
                    biggerDis = lhDis;
                    biggerFist = RightOrLeftFist.Left;
                }
                if (Geometry.FloatEqual_WithIn0p001(biggerDis, 0))
                {
                    //如果双手移动都是0
                    speed = leftFistVelocity.speed;
                    direction = Tool.ArbitraryDirectionToNineDirection(leftFistOffset);
                    direction = wholeMatrix * -direction;
                }
                else if (Geometry.FloatEqual_WithIn0p001(smallerDis, 0) && !Geometry.FloatEqual_WithIn0p001(biggerDis, 0))
                {
                    //如果一只手的移动是0
                    //身体和移动的那只手，等效于单手移动
                    if (biggerFist == RightOrLeftFist.Left)
                    {
                        speed = leftFistVelocity.speed;
                        direction = Tool.ArbitraryDirectionToNineDirection(leftFistOffset);
                        direction = wholeMatrix * -direction;
                    }
                    else if (biggerFist == RightOrLeftFist.Right)
                    {
                        speed = rightFistVelocity.speed;
                        direction = Tool.ArbitraryDirectionToNineDirection(rightFistOffset);
                        direction = wholeMatrix * -direction;
                    }
                }
                else
                {
                    /*  
                        双手移动都不是0时

                        速度，大小和fist一致
                        方向，和offset一致。
                            但是，只有8向，而不是所有方向。
                            这涉及到宏观还是微观看身体移动方向的问题。
                    */
                    speed = leftFistVelocity.speed;

                    float directionRightPart = 0;
                    float rightBigDot = 0;
                    if (Mathf.Abs(Vector2.Dot(rightFistOffset, Vector2.right))
                        >= Mathf.Abs(Vector2.Dot(leftFistOffset, Vector2.right))
                        )
                        rightBigDot = Vector2.Dot(rightFistOffset, Vector2.right);
                    else
                        rightBigDot = Vector2.Dot(leftFistOffset, Vector2.right);
                    directionRightPart = Mathf.Sign(rightBigDot) * (Geometry.FloatEqual_WithIn0p001(rightBigDot, 0) ? 0 : 1);
                    float directionUpPart = 0;
                    float upBigDot = 0;
                    if (Mathf.Abs(Vector2.Dot(rightFistOffset, Vector2.up))
                        >= Mathf.Abs(Vector2.Dot(leftFistOffset, Vector2.up))
                        )
                        upBigDot = Vector2.Dot(rightFistOffset, Vector2.up);
                    else
                        upBigDot = Vector2.Dot(leftFistOffset, Vector2.up);
                    directionUpPart = Mathf.Sign(upBigDot) * (Geometry.FloatEqual_WithIn0p001(upBigDot, 0) ? 0 : 1);
                    direction = (directionRightPart * Vector2.right + directionUpPart * Vector2.up).normalized;
                    direction = wholeMatrix * -direction;
                }
            }
            
        }

        History newDirection = new History {time = Time.fixedTime, direction = this.direction, speed = this.speed };
        history.Add(newDirection);
        while ( (Time.fixedTime - history[0].time) > historyTime )
        {
            history.RemoveAt(0);
        }
    }

}
