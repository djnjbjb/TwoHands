using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class HandControl : MonoBehaviour
{
    /*
        FixedUpdate的思路如下
        1.先做一些准备，
            包括常用变量的更新
            HandState的更新
                HandStuff的处理目前也在这里，因为比较少，也没地方好放
        2.处理手相关的行为
            这时，手的一些量会坐实，同时产出一些整体移动需要的变量
        3.处理身体相关的行为
            需要引入一些变量
        4.善后工作
            纯表现的更新
        2、3是主体

        需要把一些接口的行为定义清楚
     
     */


    void FixedUpdate()
    {
        //-----------------------------------------------------------------------------
        FU_Pre();


        /*
            计算手的作用手的作用对自己的变化，手的作用对whole的影响
        */
        wholeGrabEnvOffset = new Vector2();

        //Hand
        if (rightFistState != HandState.GrabEnv)
            FU_Fist_NoGrabEnv_Right(HKey.rMvDir);
        if (leftFistState != HandState.GrabEnv)
            FU_Fist_NoGrabEnv_Left(HKey.lMvDir);
        if (rightFistState == HandState.GrabEnv && leftFistState != HandState.GrabEnv)
            FU_Fist_GrabEnv_Right();
        if (rightFistState != HandState.GrabEnv && leftFistState == HandState.GrabEnv)
            FU_Fist_GrabEnv_Left();

        if (rightFistState == HandState.GrabEnv && leftFistState == HandState.GrabEnv)
        {
            FU_Fist_GrabEnv_2Fist();
        }

        //Whole
        FU_Whole();

        //-----------------------------------------------------------------------------
        //Late
        handOther.Refresh();
    }

    void ___________________________I()
    {

    }

    Vector2 RightFistMove(Vector2 fistPos, Vector2 moveVecPossible)
    {
        Vector2 rightJoint1Pos = handOther.RightJoint1.transform.localPosition;

        Vector2 positionResult = fistPos;
        positionResult += moveVecPossible;

        float minY = rightJoint1Pos.y - length;
        float maxY = rightJoint1Pos.y + length;
        float middleY = rightJoint1Pos.y + minToOrigin;
        float minX = rightJoint1Pos.x - length;
        float middleX = rightJoint1Pos.x + minToOrigin;
        float maxX = rightJoint1Pos.x + length;
        if (fistPos.x < middleX)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, middleY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        else if (fistPos.y < middleY)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, middleX, maxX);
        }
        else
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        return positionResult;
    }

    Vector2 LeftFistMove(Vector2 fistPos, Vector2 moveVecPossible)
    {
        Vector2 leftJoint1Pos = handOther.LeftJoint1.transform.localPosition;
        Vector2 positionResult = fistPos;
        positionResult += moveVecPossible;

        float minY = leftJoint1Pos.y - length;
        float middleY = leftJoint1Pos.y + minToOrigin;
        float maxY = leftJoint1Pos.y + length;
        float minX = leftJoint1Pos.x - length;
        float middleX = leftJoint1Pos.x - minToOrigin;
        float maxX = leftJoint1Pos.x + length;
        positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
        if (fistPos.x > middleX)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, middleY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        else if (fistPos.y < middleY)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, middleX);
        }
        else
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        return positionResult;
    }

    float SpeedDecelerate(float speedPre, float deceleration = float.NaN)
    {
        if (float.IsNaN(deceleration))
            deceleration = fistSpeedDeceleration;
        float speed = speedPre - deceleration * Time.fixedDeltaTime;
        if (speed < fistSpeedStartingUp)
        {
            speed = 0f;
        }

        return speed;
    }
    float SpeedAccelerate(float speedPre, float acceleration = float.NaN)
    {
        if (float.IsNaN(acceleration))
            acceleration = fistSpeedAcceleration;
        float speed = speedPre + acceleration * Time.fixedDeltaTime;
        speed = Mathf.Clamp(speed, fistSpeedStartingUp, fistSpeedMax);
        return speed;
    }

    void ___________________________II()
    {

    }

    /// <summary>
    /// return offest
    /// </summary>
    Vector2 FU_Fist_NoGrabEnv_Right(Vector2 mvDir)
    {
        /*
            手的移动，先这样
            手的移动，希望既可以短距离精确移动，又可以快速移动
            精确 + 快速。精确 vs 快速，本身就存在矛盾。

            先不搞了，先这样，再做后面的，根据之后的需求再调整。
            
            现在考虑到的
            1. 减速部分。减速需要减得更快，可能lerp之类。
            2. 加速。不一定匀速加速。根据更具体的需求来搞。
            3. 其实有很多可调整的地方。
                比如，如果仅仅是摆造型需要，那可以搞一些挡位置的东西之类。
                手的速度、加速度，也可以根据手的位置等参数而变化。
        */
        Vector2 fistPos = (Vector2)rightFist.transform.localPosition;
        Vector2 fistPosAfterMove;

        /*
            先得到 moveVecPossible
            mvDir = 0，减速。mvDir!=0，加速。
               先不管speed和mvDir方向的关系，总是加速
         */
        /*
            velocity的方向：
            如果mvDir不为0，就和mvDir一样
            如果mvDir为0，就和上一帧方向一样

            不用考虑speed为0时mvDir的情况，moveVecPossible = mvDir * speed ... 自动就解决了。
        */
        float speedPre = rightFistVelocity.magnitude;
        float speed;
        if (mvDir.magnitude != 0)
            speed = SpeedAccelerate(speedPre);
        else
            speed = SpeedDecelerate(speedPre);
        if (mvDir.magnitude == 0)
            mvDir = rightFistVelocity.normalized;
        Vector2 moveVecPossible = mvDir * speed * Time.fixedDeltaTime;
        /*
            现在根据 moveVecPossible 尝试移动
            把尝试移动的结果分为2类，没有动 vs 动了
            
            如果没有动，那么无论mvDir、speed等如何，speed都应该相对于speedPre减速。
            如果动了，那就是正常情况，speed去影响rhFistVelocity。
         
         */
        fistPosAfterMove = RightFistMove(fistPos, moveVecPossible);
        rightFist.transform.localPosition = fistPosAfterMove;
        Vector2 offset = fistPosAfterMove - fistPos;
        if (MyTool.FloatEqual0p001(offset.magnitude, 0f))
        {
            speed = SpeedDecelerate(speedPre);
        }
        else
        {
            // do nothing
        }
        rightFistVelocity = speed * mvDir;
        return offset;
    }

    /// <summary>
    /// return offest
    /// </summary>
    Vector2 FU_Fist_NoGrabEnv_Left(Vector2 mvDir)
    {
        Vector2 fistPos = leftFist.transform.localPosition;
        Vector2 fistPosAfterMove;

        float speedPre = leftFistVelocity.magnitude;
        float speed;
        if (mvDir.magnitude != 0)
            speed = SpeedAccelerate(speedPre);
        else
            speed = SpeedDecelerate(speedPre);
        if (mvDir.magnitude == 0)
            mvDir = leftFistVelocity.normalized;
        Vector2 moveVecPossible = mvDir * speed * Time.fixedDeltaTime;
        /*
            现在根据 moveVecPossible 尝试移动
            把尝试移动的结果分为2类，没有动 vs 动了
            
            如果没有动，那么无论mvDir、speed等如何，speed都应该相对于speedPre减速。
            如果动了，那就是正常情况，speed去影响rhFistVelocity。
         
         */
        fistPosAfterMove = LeftFistMove(fistPos, moveVecPossible);
        leftFist.transform.localPosition = fistPosAfterMove;
        Vector2 offset = fistPosAfterMove - fistPos;
        if (MyTool.FloatEqual0p001(offset.magnitude, 0f))
        {
            speed = SpeedDecelerate(speedPre);
        }
        else
        {
            // do nothing
        }
        leftFistVelocity = speed * mvDir;
        return offset;
    }

    void FU_Fist_GrabEnv_Right()
    {
        wholeGrabEnvOffset = -FU_Fist_NoGrabEnv_Right(-HKey.rMvDir);
        wholeGrabEnvOffset = wholeMatrix * wholeGrabEnvOffset;
        wholeVelocity = wholeMatrix * (-rightFistVelocity);
    }

    void FU_Fist_GrabEnv_Left()
    {
        wholeGrabEnvOffset = -FU_Fist_NoGrabEnv_Left(-HKey.lMvDir);
        wholeGrabEnvOffset = wholeMatrix * wholeGrabEnvOffset;
        wholeVelocity = wholeMatrix * (-leftFistVelocity);
    }

    void FU_Fist_GrabEnv_2Fist()
    {
        /*
            如果两只手同向，则同向移动
            如果一直有有方向，一只手没有方向，则按照有方向的移动
            其他情况不移动
        */
        Vector2 rhDirection = -HKey.rMvDir;
        Vector2 lhDirection = -HKey.lMvDir;
        if (rhDirection == lhDirection)
        {
            /*
                需要确定的有：
                    手的offset、手的速度
                    身体的offset、身体的速度
             
                -----------------------------------------
                
                先考量offset，再考量speed
                首先让二者速度相同，之后offset的计算与以前相同。
                
                然后再计量速度。
                
                -----------------------------------------
                注意，offset的方向，可能和fistVelocity不同。因为，比如我斜向移动，但是到头了，于是结果可能是横向移动
            */
            float rhFistSpeedPre = rightFistVelocity.magnitude;
            float lhFistSpeedPre = leftFistVelocity.magnitude;
            float fistSpeedPre = Mathf.Max(rhFistSpeedPre, lhFistSpeedPre);
            float fistSpeed = SpeedAccelerate(fistSpeedPre, fistSpeedAcceleration * 2);

            //根据速度确定offset
            float dis2x = fistSpeed * Time.fixedDeltaTime;
            Vector2 mvVector = dis2x * rhDirection;
            Vector2 rhFistPosOld = rightFist.transform.localPosition;
            Vector2 rhFistPosNew = RightFistMove(rhFistPosOld, mvVector);
            Vector2 lhFistPosOld = leftFist.transform.localPosition;
            Vector2 lhFistPosNew = LeftFistMove(lhFistPosOld, mvVector);
            float rhDis = (rhFistPosNew - rhFistPosOld).magnitude;
            float lhDis = (lhFistPosNew - lhFistPosOld).magnitude;
            float smallerDis = 0;
            float biggerDis = 0;
            GameObject smallerFist = rightFist;
            GameObject biggerFist = rightFist;
            if (rhDis > lhDis)
            {
                smallerDis = lhDis;
                smallerFist = leftFist;
                biggerDis = rhDis;
                biggerFist = rightFist;
            }
            else
            {
                smallerDis = rhDis;
                smallerFist = rightFist;
                biggerDis = lhDis;
                biggerFist = leftFist;
            }
            if (MyTool.FloatEqual0p001(biggerDis, 0))
            {
                //如果双手移动都是0
                //位置都不动，手和身体都减速
                fistSpeed = SpeedDecelerate(fistSpeedPre);
                rightFistVelocity = fistSpeed * rhDirection;
                leftFistVelocity = fistSpeed * lhDirection;

                wholeGrabEnvOffset = new Vector2();
                wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
            }
            else if (MyTool.FloatEqual0p001(smallerDis, 0) && !MyTool.FloatEqual0p001(biggerDis, 0))
            {
                //如果一只手的移动是0
                //身体和移动的那只手，等效于单手移动
                //另一只手减速
                if (biggerFist == rightFist)
                {
                    FU_Fist_GrabEnv_Right();
                    leftFistVelocity = SpeedDecelerate(leftFistVelocity.magnitude) * lhDirection;
                }
                else if (biggerFist == leftFist)
                {
                    FU_Fist_GrabEnv_Left();
                    rightFistVelocity = SpeedDecelerate(rightFistVelocity.magnitude) * rhDirection;
                }
            }
            else
            {
                /*  
                    双手移动都不是0
                        其实分2种情况，一种是都可以移动到dis2x，一种是不行，但在计算时没有区别
                    
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
        else if (rhDirection.magnitude != 0 && lhDirection.magnitude == 0)
        {
            FU_Fist_GrabEnv_Right();
            leftFistVelocity = SpeedDecelerate(leftFistVelocity.magnitude) * lhDirection;
        }
        else if (rhDirection.magnitude == 0 && lhDirection.magnitude != 0)
        {
            FU_Fist_GrabEnv_Left();
            rightFistVelocity = SpeedDecelerate(rightFistVelocity.magnitude) * rhDirection;
        }
        else
        {
            //do nothing
        }
    }

    void ___________________________IIII()
    {

    }

    void FU_Whole()
    {
        /*
            当手有GrabEnv时，用offset计算。
            没有时，用speed计算。
            
            有GrabEnv时，不考虑任何碰撞。
            没有时，只考虑向下运动的碰撞。
                没有考虑从左右碰撞collider的问题。感觉暂时不需要。

            这部分需要搞定whole的position和speed。

            ----------------
            水平方向有摩擦力减速
            只有在地面时，才有摩擦力减速
                向上无摩擦力
                向下，只有接触地面时有摩擦力
        */

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
        /*
            1
            有GrabEnv 
        */
        if (rightFistState == HandState.GrabEnv || leftFistState == HandState.GrabEnv)
        {
            //whole position
            whole.transform.position += (Vector3)wholeGrabEnvOffset;
            //whole speed -> 不变
            return;
        }

        /*
            2
            无GrabEnv，无向下运动
        */
        if (wholeVelNew_DownPart <= 0)
        {
            //whole position
            whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
            //whole speed
            wholeVelocity = wholeVelocityNew;
            return;
        }

        /*
            3
            无GrabEnv，有向下运动
        */
        //3.1 get FootState
        FootState footState;
        {
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hit = Physics2D.Linecast(pointBL, pointBR);

            Vector2 pointBLUp = (Vector2)bottomLeftPoint.transform.position + new Vector2(0, surfaceTolerance);
            Vector2 pointBRUp = (Vector2)bottomRightPoint.transform.position + new Vector2(0, surfaceTolerance);
            RaycastHit2D hitUp = Physics2D.Linecast(pointBLUp, pointBRUp);

            Vector2 pointBLDown = (Vector2)bottomLeftPoint.transform.position - new Vector2(0, surfaceTolerance);
            Vector2 pointBRDown = (Vector2)bottomRightPoint.transform.position - new Vector2(0, surfaceTolerance);
            RaycastHit2D hitDown = Physics2D.Linecast(pointBLDown, pointBRDown);

            if (!hitUp.collider && !hitDown.collider)
                footState = FootState.Air;
            else if (!hitUp.collider && hitDown.collider)
                footState = FootState.Surface;
            else
            {
                footState = FootState.EnvRock;

                int layerMaskGround = LayerMask.GetMask("EnvGround");
                RaycastHit2D hitDownGround = Physics2D.Linecast(pointBLDown, pointBRDown, layerMaskGround);
                RaycastHit2D hitUpGround = Physics2D.Linecast(pointBLUp, pointBRUp, layerMaskGround);
                if (hitDownGround.collider)
                    footState = FootState.EnvGround;
            }
        }

        //3.2 do regarding to footState
        if (footState == FootState.Surface || footState == FootState.EnvGround)
        {
            float wholeVelNew_RightPart = Vector2.Dot(wholeVelocityNew, Vector2.right);
            wholeVelocityNew = wholeVelNew_RightPart * Vector2.right;
            //水平方向摩擦力减速
            float speedNew = wholeVelocityNew.magnitude - fistSpeedDeceleration * Time.fixedDeltaTime;
            speedNew = Mathf.Clamp(speedNew, 0f, wholeSpeedMax);
            wholeVelocityNew = speedNew * wholeVelocityNew.normalized;
            //whole position
            whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
            //whole speed
            wholeVelocity = wholeVelocityNew;

        }
        else if (footState == FootState.Air)
        {
            float downDis = wholeVelNew_DownPart * Time.fixedDeltaTime;

            //只用2个点做Raycast，不能保证，但先这样
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis);
            RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis);

            if (!hitBL.collider && !hitBR.collider)
            {
                //whole position
                whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
                //whole speed
                wholeVelocity = wholeVelocityNew;
            }
            else
            {
                if (hitBL.collider)
                {
                    downDis = hitBL.distance;
                }
                if (hitBR.collider)
                {
                    if (hitBR.distance < downDis)
                    {
                        downDis = hitBR.distance;
                    }
                }

                float wholeVelNew_RightPart = Vector2.Dot(wholeVelocityNew, Vector2.right);
                wholeVelocityNew = wholeVelNew_RightPart * Vector2.right;
                //whole position
                whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime + new Vector3(0, -downDis, 0);
                //whole speed
                wholeVelocity = wholeVelocityNew;
            }
        }
        else if (footState == FootState.EnvRock)
        {
            float downDis = wholeVelNew_DownPart * Time.fixedDeltaTime;

            //只用2个点做Raycast，不能保证，但先这样
            //当前是EnvRock，如果经过一个Air，再出现EnvRock。这里的算法就会有问题。但一帧移动的很少，目前应该不会出现。先不管它。
            int layerMaskGround = LayerMask.GetMask("EnvGround");
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis, layerMaskGround);
            RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis, layerMaskGround);

            if (!hitBL.collider && !hitBR.collider)
            {
                //whole position
                whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
                //whole speed
                wholeVelocity = wholeVelocityNew;
            }
            else
            {
                if (hitBL.collider)
                {
                    downDis = hitBL.distance;
                }
                if (hitBR.collider)
                {
                    if (hitBR.distance < downDis)
                    {
                        downDis = hitBR.distance;
                    }
                }

                float wholeVelNew_RightPart = Vector2.Dot(wholeVelocityNew, Vector2.right);
                wholeVelocityNew = wholeVelNew_RightPart * Vector2.right;
                //whole position
                whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime + new Vector3(0, -downDis, 0);
                //whole speed
                wholeVelocity = wholeVelocityNew;
            }
        }
    }
}