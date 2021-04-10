using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeVelocityBeforeJump: Velocity
{
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

    public WholeVelocityBeforeJump()
    {

    }

    public void FixedUpdateManually(Params @params)
    {
        /*
            身体的速度，大小和fist一致
            方向，和offset一致。
                但是，只有8向，而不是所有方向。
                这涉及到宏观还是微观看身体移动方向的问题。
        */

        FistStatePlus leftFistState = @params.leftFistState;
        FistStatePlus rightFistState = @params.leftFistState;
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
            Vector2 velocity = leftFistVelocity.speed * leftFistVelocity.direction;
            velocity = @params.wholeMatrix * -velocity;
            speed = velocity.magnitude;
            direction = velocity.normalized;
        }
        if (leftFistState != FistState.GrabEnv && rightFistState == FistState.GrabEnv)
        {
            Vector2 velocity = rightFistVelocity.speed * rightFistVelocity.direction;
            velocity = @params.wholeMatrix * -velocity;
            speed = velocity.magnitude;
            direction = velocity.normalized;
        }

        if (rightFistState == FistState.GrabEnv && leftFistState == FistState.GrabEnv)
        {
            float rhDis = rightFistOffset.magnitude;
            float lhDis = leftFistOffset.magnitude;
            float smallerDis = 0;
            float biggerDis = 0;
            RightOrLeftFist smallerFist = RightOrLeftFist.Left;
            RightOrLeftFist biggerFist = RightOrLeftFist.Left;
            if (rhDis > lhDis)
            {
                smallerDis = lhDis;
                smallerFist = RightOrLeftFist.Left;
                biggerDis = rhDis;
                biggerFist = RightOrLeftFist.Right;
            }
            else
            {
                smallerDis = rhDis;
                smallerFist = RightOrLeftFist.Right;
                biggerDis = lhDis;
                biggerFist = RightOrLeftFist.Left;
            }
            if (MyTool.FloatEqual0p001(biggerDis, 0))
            {
                //如果双手移动都是0
                speed = leftFistVelocity.speed;
                direction = wholeMatrix * (-leftFistVelocity.direction);
            }
            else if (MyTool.FloatEqual0p001(smallerDis, 0) && !MyTool.FloatEqual0p001(biggerDis, 0))
            {
                //如果一只手的移动是0
                //身体和移动的那只手，等效于单手移动
                if (biggerFist == RightOrLeftFist.Left)
                {
                    speed = leftFistVelocity.speed;
                    direction = wholeMatrix * (-leftFistVelocity.direction);
                }
                else if (biggerFist == RightOrLeftFist.Right)
                {
                    speed = rightFistVelocity.speed;
                    direction = wholeMatrix * (-rightFistVelocity.direction);
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
                directionRightPart = Mathf.Sign(rightBigDot) * (MyTool.FloatEqual0p001(rightBigDot, 0) ? 0 : 1);
                float directionUpPart = 0;
                float upBigDot = 0;
                if (Mathf.Abs(Vector2.Dot(rightFistOffset, Vector2.up))
                    >= Mathf.Abs(Vector2.Dot(leftFistOffset, Vector2.up))
                   )
                    upBigDot = Vector2.Dot(rightFistOffset, Vector2.up);
                else
                    upBigDot = Vector2.Dot(leftFistOffset, Vector2.up);
                directionUpPart = Mathf.Sign(upBigDot) * (MyTool.FloatEqual0p001(upBigDot, 0) ? 0 : 1);
                direction = (directionRightPart * Vector2.right + directionUpPart * Vector2.up).normalized;
            }
        }
    }
}
