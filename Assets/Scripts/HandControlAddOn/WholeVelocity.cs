using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeVelocity: Velocity
{
    public class WholeVelocityParams
    {
        public FistStatePlus leftFistState { get; set; }
        public FistStatePlus rightFistState { get; set; }
        public Velocity leftFistVelocity { get; set; }
        public Velocity rightFistVelocity { get; set; }
        public Vector2 leftFistOffset { get; set; }
        public Vector2 rightFistOffset { get; set; }
        public Matrix4x4 wholeMatrix { get; set; }
        public HandControl handControl { get; set; }
    }

    public WholeVelocity()
    {

    }

    public void FixedUpdateManually(WholeVelocityParams param)
    {
        /*
            身体的速度，大小和fist一致
            方向，和offset一致。
                但是，只有8向，而不是所有方向。
                这涉及到宏观还是微观看身体移动方向的问题。
        */

        FistStatePlus leftFistState = param.leftFistState;
        FistStatePlus rightFistState = param.leftFistState;
        Velocity leftFistVelocity = param.leftFistVelocity;
        Velocity rightFistVelocity = param.rightFistVelocity;
        Vector2 leftFistOffset = param.leftFistOffset;
        Vector2 rightFistOffset = param.rightFistOffset;
        Matrix4x4 wholeMatrix = param.wholeMatrix;
        HandControl handControl = param.handControl;

        if (leftFistState != FistState.GrabEnv && rightFistState != FistState.GrabEnv)
        {
            Vector2 wholeVelocityNew;
            float wholeVelNew_DownPart;
            {
                float deltaSpeed = gravityAcceleration * Time.fixedDeltaTime;
                Vector2 deltaVelocity = deltaSpeed * Vector2.down;

                wholeVelocityNew = wholeVelocity + deltaVelocity;
                if (wholeVelocityNew.magnitude > wholeSpeedMax)
                {
                    wholeVelocityNew = wholeVelocityNew.normalized * wholeSpeedMax;
                }
                wholeVelNew_DownPart = Vector2.Dot(wholeVelocityNew, Vector2.down);
            }
        }
        if (leftFistState == FistState.GrabEnv && rightFistState != FistState.GrabEnv)
        {
            Vector2 velocity = leftFistVelocity.speed * leftFistVelocity.direction;
            velocity = param.wholeMatrix * -velocity;
            speed = velocity.magnitude;
            direction = velocity.normalized;
        }
        if (leftFistState != FistState.GrabEnv && rightFistState == FistState.GrabEnv)
        {
            Vector2 velocity = rightFistVelocity.speed * rightFistVelocity.direction;
            velocity = param.wholeMatrix * -velocity;
            speed = velocity.magnitude;
            direction = velocity.normalized;
        }

        if (rightFistState == FistState.GrabEnv && leftFistState == FistState.GrabEnv)
        {
            float rhDis = rightFistOffset.magnitude;
            float lhDis = leftFistOffset.magnitude;
            float smallerDis = 0;
            float biggerDis = 0;
            GameObject smallerFist = handControl.leftFist;
            GameObject biggerFist = handControl.leftFist;
            if (rhDis > lhDis)
            {
                smallerDis = lhDis;
                smallerFist = handControl.leftFist;
                biggerDis = rhDis;
                biggerFist = handControl.rightFist;
            }
            else
            {
                smallerDis = rhDis;
                smallerFist = handControl.rightFist;
                biggerDis = lhDis;
                biggerFist = handControl.leftFist;
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
                if (biggerFist == handControl.leftFist)
                {
                    speed = leftFistVelocity.speed;
                    direction = wholeMatrix * (-leftFistVelocity.direction);
                }
                else if (biggerFist == handControl.rightFist)
                {
                    speed = rightFistVelocity.speed;
                    direction = wholeMatrix * (-rightFistVelocity.direction);
                }
            }
            else
            {
                /*  
                    双手移动都不是0
                        其实分2种情况，一种是都可以移动到dis，一种是没有，但在计算时没有区别

                    如果双手的移动方向一致：
                    双手都移动到对应的dis。身体的offset是biggerDis。
                    双手速度都增加到speed。身体速度则是-speed。

                    注意，offset的方向，可能和fistVelocity不同。
                    这时应如何计算呢？
                    手的移动，依然是相应的dis。手的速度变化，比较难搞，就简单化，依然增加到speed。
                    身体的移动，取决于2个dis，x方向上的最大分量和y方向的最大分量。身体的速度，也增加到speed，但反向。
                */
                Vector2 wholeOffset = new Vector2();
                Vector2 rhOffset = rhFistPosNew - rhFistPosOld;
                Vector2 lhOffset = lhFistPosNew - lhFistPosOld;
                if (Vector2.Dot(rhOffset, Vector2.right) >= Vector2.Dot(lhOffset, Vector2.right))
                {
                    wholeOffset += Vector2.Dot(rhOffset, Vector2.right) * Vector2.right;
                }
                else
                {
                    wholeOffset += Vector2.Dot(lhOffset, Vector2.right) * Vector2.right;
                }
                if (Vector2.Dot(rhOffset, Vector2.down) >= Vector2.Dot(lhOffset, Vector2.down))
                {
                    wholeOffset += Vector2.Dot(rhOffset, Vector2.down) * Vector2.down;
                }
                else
                {
                    wholeOffset += Vector2.Dot(lhOffset, Vector2.down) * Vector2.down;
                }

                //----------------------------------------------------
                rightFist.transform.localPosition = rhFistPosNew;
                leftFist.transform.localPosition = lhFistPosNew;
                rightFistVelocity = fistSpeed * rhDirection;
                leftFistVelocity = fistSpeed * lhDirection;

                wholeGrabEnvOffset = wholeMatrix * (-wholeOffset);
                /*
                    如果双手移动同向，同时又和rhDirection不同，那么这里的速度会有问题
                    应为 wholeMatrix * (-fistSpeed * wholeGrabEnvOffset.normal)
                    暂时先不改
                */
                wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
            }
        }
    }
}
