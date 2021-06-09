using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeVelocityBeforeJump: Velocity
{
    public struct DirectionHistory
    {
        public float time;
        public Vector2 direction;
    }
    private float historyTime;
    public Queue<DirectionHistory> history;
    public class Params
    {
        public FistStatePlus leftFistState { get; set; }
        public FistStatePlus rightFistState { get; set; }
        public Velocity leftFistVelocity { get; set; }
        public Velocity rightFistVelocity { get; set; }
        public Vector2 leftFistOffset { get; set; }
        public Vector2 rightFistOffset { get; set; }
        public Matrix4x4 wholeMatrix { get; set; }
    }

    public WholeVelocityBeforeJump(float historyTime)
    {
        speed = 0f;
        direction = new Vector2();
        this.historyTime = historyTime;
        history = new Queue<DirectionHistory>();
    }

    public void FixedUpdateManually(Params @params)
    {
        /*
            身体的速度，大小和fist一致
            方向，和offset一致。
                但是，只有9向，而不是所有方向。
                这涉及到宏观还是微观看身体移动方向的问题。
            9向，是8个方向 + 没有方向。
        */

        FistStatePlus leftFistState = @params.leftFistState;
        FistStatePlus rightFistState = @params.rightFistState;
        Velocity leftFistVelocity = @params.leftFistVelocity;
        Velocity rightFistVelocity = @params.rightFistVelocity;
        Vector2 leftFistOffset = @params.leftFistOffset;
        Vector2 rightFistOffset = @params.rightFistOffset;
        Matrix4x4 wholeMatrix = @params.wholeMatrix;

        if (leftFistState != FistState.GrabEnv && rightFistState != FistState.GrabEnv)
        {
            Debug.LogError("WholeVelocityBeforeJump should not be called while jumping");
        }
        if (leftFistState == FistState.GrabEnv && rightFistState != FistState.GrabEnv)
        {
            speed = leftFistVelocity.speed;
            direction = ArbitraryDirectionToNineDirection(leftFistOffset);
            direction = wholeMatrix * -direction;
        }
        if (leftFistState != FistState.GrabEnv && rightFistState == FistState.GrabEnv)
        {
            speed = rightFistVelocity.speed;
            direction = ArbitraryDirectionToNineDirection(rightFistOffset);
            direction = wholeMatrix * -direction;
        }

        if (rightFistState == FistState.GrabEnv && leftFistState == FistState.GrabEnv)
        {
            /*
                在双手GrabEnv的情况下，有几种情况。
                1. 双手都有方向且同向
                    这是最典型的情况。也是用到主要算法：根据两只手的offset来计算。
                2. 有一只手有方向，一只手没方向。
                    这时，如果有方向的手的offset不为0，根据两只手的offset来计算即可。
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
                direction = ArbitraryDirectionToNineDirection(leftFistOffset);
                direction = wholeMatrix * -direction;
            }
            else if (leftFistVelocity.direction.magnitude == 0
                     && rightFistVelocity.direction.magnitude != 0)
            {
                speed = rightFistVelocity.speed;
                direction = ArbitraryDirectionToNineDirection(rightFistOffset);
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
                if (Ludo.Utility.FloatEqual_WithIn0p001(biggerDis, 0))
                {
                    //如果双手移动都是0
                    speed = leftFistVelocity.speed;
                    direction = ArbitraryDirectionToNineDirection(leftFistOffset);
                    direction = wholeMatrix * -direction;
                }
                else if (Ludo.Utility.FloatEqual_WithIn0p001(smallerDis, 0) && !Ludo.Utility.FloatEqual_WithIn0p001(biggerDis, 0))
                {
                    //如果一只手的移动是0
                    //身体和移动的那只手，等效于单手移动
                    if (biggerFist == RightOrLeftFist.Left)
                    {
                        speed = leftFistVelocity.speed;
                        direction = ArbitraryDirectionToNineDirection(leftFistOffset);
                        direction = wholeMatrix * -direction;
                    }
                    else if (biggerFist == RightOrLeftFist.Right)
                    {
                        speed = rightFistVelocity.speed;
                        direction = ArbitraryDirectionToNineDirection(rightFistOffset);
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
                    directionRightPart = Mathf.Sign(rightBigDot) * (Ludo.Utility.FloatEqual_WithIn0p001(rightBigDot, 0) ? 0 : 1);
                    float directionUpPart = 0;
                    float upBigDot = 0;
                    if (Mathf.Abs(Vector2.Dot(rightFistOffset, Vector2.up))
                        >= Mathf.Abs(Vector2.Dot(leftFistOffset, Vector2.up))
                        )
                        upBigDot = Vector2.Dot(rightFistOffset, Vector2.up);
                    else
                        upBigDot = Vector2.Dot(leftFistOffset, Vector2.up);
                    directionUpPart = Mathf.Sign(upBigDot) * (Ludo.Utility.FloatEqual_WithIn0p001(upBigDot, 0) ? 0 : 1);
                    direction = (directionRightPart * Vector2.right + directionUpPart * Vector2.up).normalized;
                    direction = wholeMatrix * -direction;
                }
            }
            
        }

        DirectionHistory newDirection = new DirectionHistory {time = Time.fixedTime, direction = this.direction};
        history.Enqueue(newDirection);
        while ( (Time.fixedTime - history.Peek().time) > historyTime )
        {
            history.Dequeue();
        }
    }

    private Vector2 ArbitraryDirectionToNineDirection(Vector2 direction)
    {
        float directionRightPart = 0;
        float rightDot = Vector2.Dot(direction, Vector2.right);
        directionRightPart = Mathf.Sign(rightDot) * (Ludo.Utility.FloatEqual_WithIn0p001(rightDot, 0) ? 0 : 1);
        float directionUpPart = 0;
        float upDot = Vector2.Dot(direction, Vector2.up);
        directionUpPart = Mathf.Sign(upDot) * (Ludo.Utility.FloatEqual_WithIn0p001(upDot, 0) ? 0 : 1);
        direction = (directionRightPart * Vector2.right + directionUpPart * Vector2.up).normalized;
        return direction;
    }
}
