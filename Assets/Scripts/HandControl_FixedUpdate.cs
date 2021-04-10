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
        FU_Pre();

        FU_Main();

        handRepresent.Refresh();
    }

    void FU_Main()
    {
        FU_FistSpeed();
        FU_FistOffset();
        /*
            offset计算完成后，offset是否会对speed产生影响呢？
            可以产生影响，也可以不产生影响，比如offset为0的情况。
            只考虑Fist这部分，其实没太大影响。就先不做，到whole部分再处理。
        */
        //Apply
        this.rightFist.transform.localPosition += (Vector3)fistOffset.left;
        this.leftFist.transform.localPosition += (Vector3)fistOffset.right;

        //Whole
        FU_Whole();
    }

    void ____________FU_General_______________()
    {

    }

    /// <summary>
    /// 
    /// return afterMovePosition
    /// 
    /// </summary>
    /// <param name="joint1Pos"></param>
    /// <param name="length"></param>
    /// <param name="fistPos"></param>
    /// <param name="moveVector"></param>
    /// <returns> After Move Position </returns>
    static void TryMove(Vector2 joint1Pos, float length, Vector2 fistPos, Vector2 moveVector, out Vector2 offset)
    {
        float minY = joint1Pos.y - length;
        float maxY = joint1Pos.y + length;
        float minX = joint1Pos.x - length;
        float maxX = joint1Pos.x + length;

        Vector2 afterMove = fistPos + moveVector;
        afterMove.x = Mathf.Clamp(afterMove.x, minX, maxX);
        afterMove.y = Mathf.Clamp(afterMove.y, minY, maxY);

        offset = afterMove - fistPos;
    }

    void ___________FU_Fist_____________()
    {

    }

    void FU_FistSpeed()
    {
        /*Fist Speed
            输入
                FistState
                HKey
                此外，还需要加上ifGrabMoveReverse来确定方向
            输出
                FistSpeed
        */
        ParameterForSpeed leftParameter = new ParameterForSpeed
        {
            anyKeyHold = HKey.lDown || HKey.lUp || HKey.lRight || HKey.lLeft,
            keyDir = HKey.lMvDir,
            handState = leftFistState,
            moveDir = HKey.lMvDir * (leftFistState == FistState.GrabEnv ? ifGrabMoveReverse : 1)
        };
        ParameterForSpeed rightParameter = new ParameterForSpeed
        {
            anyKeyHold = HKey.rDown || HKey.rUp || HKey.rRight || HKey.rLeft,
            keyDir = HKey.rMvDir,
            handState = rightFistState,
            moveDir = HKey.rMvDir * (rightFistState == FistState.GrabEnv ? ifGrabMoveReverse : 1)
        };
        fistVelocity.RefreshSpeed(leftParameter: leftParameter, rightParameter: rightParameter);

        Yurowm.DebugTools.DebugPanel.Log("rightSpeed", "Speed", fistVelocity.right.speed);
        Yurowm.DebugTools.DebugPanel.Log("leftSpeed", "Speed", fistVelocity.left.speed);
    }

    void FU_FistOffset()
    {
        /*
            FistOffset
            输入
                FistState
                FistSpeed
                Fist的位置信息
                    Fist范围相关的量，joint1、lengh等，一直保持不变，所以不必当做输入
            输出
                FistOffset
        */

        HandControlTool.TryMoveRegionParams leftParams = new HandControlTool.TryMoveRegionParams();
        leftParams.fistPos = leftFist.transform.localPosition;
        leftParams.joint1Pos = handRepresent.leftJoint1.transform.localPosition;
        leftParams.length = length;
        HandControlTool.TryMoveRegionParams rightParams = new HandControlTool.TryMoveRegionParams();
        rightParams.fistPos = rightFist.transform.localPosition;
        rightParams.joint1Pos = handRepresent.rightJoint1.transform.localPosition;
        rightParams.length = length;
        /*
            velocity包含speed和direction
            speed不为0时，direction可能为(0,0)
            这使得，不按按键，手就不移动，自然就导致MovePossible为0。
        */
        Vector2 leftMovePossible = fistVelocity.left.velocity * Time.fixedDeltaTime;
        Vector2 rightMovePossible = fistVelocity.right.velocity * Time.fixedDeltaTime;
        Vector2 leftOffset = new Vector2();
        Vector2 rightOffset = new Vector2();


        if (leftFistState != FistState.GrabEnv)
        {
            HandControlTool.TryMove(leftParams,leftMovePossible,out leftOffset);
        }
        if (rightFistState != FistState.GrabEnv)
        {
            HandControlTool.TryMove(rightParams, rightMovePossible, out rightOffset);
        }
        if (leftFistState == FistState.GrabEnv && rightFistState != FistState.GrabEnv)
        {
            HandControlTool.TryMove(leftParams, leftMovePossible, out leftOffset);
        }
        if (leftFistState != FistState.GrabEnv && rightFistState == FistState.GrabEnv)
        {
            HandControlTool.TryMove(rightParams, rightMovePossible, out rightOffset);
        }
        if (rightFistState == FistState.GrabEnv && leftFistState == FistState.GrabEnv)
        {
            FU_FistOffset_GrabEnvTwoFist(leftFistOffset: out leftOffset, rightFistOffset: out rightOffset);
        }

        fistOffset.left = leftOffset;
        fistOffset.right = rightOffset;
    }

    void FU_Fists(
       FistState leftFistState, Vector2 leftMoveDir,
       FistState rightFistState, Vector2 rightMoveDir )
    {
        /*
           计算手的作用手的作用对自己的变化，手的作用对whole的影响

           重构时，需要把变化表达清楚。

           --双手--
           简单想想
               输入是什么：按键的方向，手的速度
               输出是什么：手的offset，手的速度，身体的offset，身体的速度？
               哪些用于手自己
               哪些用于身体
           整理
               首先，把双手的事情看做一个整体，看它有什么输入，有什么输出。
               抽象出来一个函数。

               这个函数并不起实际作用。只会计算。其实计算出的是一些指导性意见。
               然后，再搞个应用。

               输入: 按键的方向，手在上一阵的速
               输出：手的offset，手的速度

           --身体--
           其实计算出，也是个指导意见，比如offset。
           offset作为输入，根据其他东西去输出。

           --还是需要一个目的，我要做的关卡是什么--
           这一版本的目的还是很简单，只是移动和跳跃，对身体移动限制的东西基本没有，
           重构只是为了清晰，把那些变化的量明确出来，作为指导意见
        */


        /*
         输出：
            手的offset、手的速度。这二者的大小、方向是否一致。
                这些可以直接应用。
            （？）身体offset。这部分应该不应用，只是作为指导。
                也许可以不输出，只是由身体部分去决定。
                用handState和手的offset，是可以计算的。
            （无）身体veloctiy。这个不输出。在身体部分代码，应当根据手的offset、手的速度等东西，去决定自己的速度。
            (重复)HandState。身体的计算，也要参考这个。

            加入双手代码后，除了HandState，还需要考虑mvDir的方向。
            在双手的部分，受这个而影响。
                能否统一化呢？
                只要让不动，似乎就可以。
                也许不是很有必要。
                我的目的，其实是把hand和whole的移动完全分离。
                分离的目的，是因为whole部分作用的东西比较多，比如移动的平台等。为了方便那部分的书写。

        输入：
            HandState
            操作的MvDir
            手的速度
            也许不用那么明确输入。
                还有些细节，比如joint1，这就不重要。
                上面那3个，感觉也不重要。
         
        */
        //Fist Speed
        ParameterForSpeed leftParameter = new ParameterForSpeed
        {
            anyKeyHold = HKey.lDown || HKey.lUp || HKey.lRight || HKey.lLeft,
            keyDir = HKey.lMvDir,
            handState = leftFistState,
            moveDir = HKey.lMvDir * (leftFistState == FistState.GrabEnv ? ifGrabMoveReverse : 1)
        };
        ParameterForSpeed rightParameter = new ParameterForSpeed
        {
            anyKeyHold = HKey.rDown || HKey.rUp || HKey.rRight || HKey.rLeft,
            keyDir = HKey.rMvDir,
            handState = rightFistState,
            moveDir = HKey.rMvDir * (rightFistState == FistState.GrabEnv ? ifGrabMoveReverse : 1)
        };
        fistVelocity.RefreshSpeed(leftParameter: leftParameter, rightParameter: rightParameter);

        Yurowm.DebugTools.DebugPanel.Log("rightSpeed", "Speed", fistVelocity.right.speed);
        Yurowm.DebugTools.DebugPanel.Log("leftSpeed", "Speed", fistVelocity.left.speed);

        //Fist offset
        Vector2 leftFistOffset = new Vector2();
        Vector2 rightFistOffset = new Vector2();
        if (rightFistState != FistState.GrabEnv)
        {
            FU_Fist_NoGrabEnv_Right(rightMoveDir, out rightFistOffset);
        }
        if (leftFistState != FistState.GrabEnv)
        {
            FU_Fist_NoGrabEnv_Left(leftMoveDir, out leftFistOffset);
        }

        if (rightFistState == FistState.GrabEnv && leftFistState != FistState.GrabEnv)
        {
            FU_Fist_GrabEnv_Right(out rightFistOffset);
        }
        if (rightFistState != FistState.GrabEnv && leftFistState == FistState.GrabEnv)
        {
            FU_Fist_GrabEnv_Left(out leftFistOffset);

        }
        if (rightFistState == FistState.GrabEnv && leftFistState == FistState.GrabEnv)
        {
            FU_FistOffset_GrabEnvTwoFist(leftFistOffset: out leftFistOffset, rightFistOffset: out rightFistOffset);
        }

        fistOffset.left = leftFistOffset;
        fistOffset.right = rightFistOffset;

    }

    void FU_FistOffset_GrabEnvTwoFist(out Vector2 leftFistOffset, out Vector2 rightFistOffset)
    {
        /*
            在这一部分，只需要确定双手的offset。
            whole的部分不用考虑。whole会依照手offset和HandState的一些情况，来考虑自己。
            
            如果两只手同向，则同向移动。
            如果一直有有方向，一只手没有方向，单手移动。
            其他情况不移动。
        */
        Vector2 rhDirection = ifGrabMoveReverse * HKey.rMvDir;
        Vector2 lhDirection = ifGrabMoveReverse * HKey.lMvDir;
        if ((rhDirection == lhDirection) && rhDirection.magnitude != 0)
        {
            float dis = fistVelocity.left.speed * Time.fixedDeltaTime;
            Vector2 moveVector = dis * rhDirection;
            Vector2 rightFistPos = rightFist.transform.localPosition;
            Vector2 rightFistJoint1Pos = handRepresent.rightJoint1.transform.localPosition;
            TryMove(rightFistJoint1Pos, length, rightFistPos, moveVector, out rightFistOffset);
            Vector2 leftFistPos = leftFist.transform.localPosition;
            Vector2 leftFistJoint1Pos = handRepresent.leftJoint1.transform.localPosition;
            TryMove(leftFistJoint1Pos, length, leftFistPos, moveVector, out leftFistOffset);
        }
        else if (rhDirection.magnitude != 0 && lhDirection.magnitude == 0)
        {
            leftFistOffset = new Vector2();
            FU_Fist_GrabEnv_Right(out rightFistOffset);
        }
        else if (rhDirection.magnitude == 0 && lhDirection.magnitude != 0)
        {
            FU_Fist_GrabEnv_Left(out leftFistOffset);
            rightFistOffset = new Vector2();
        }
        else
        {
            leftFistOffset = new Vector2();
            rightFistOffset = new Vector2();
        }
    }

    void _________FU_Whole__________________()
    {

    }

    void FU_Whole()
    {
        FU_Whole_Velocity();
        FU_Whole_Offset();
    }

    void FU_Whole_Velocity()
    {
        Matrix4x4 wholeMatrix = Matrix4x4.TRS(whole.transform.position, whole.transform.localRotation, whole.transform.localScale);
        if (leftFistState != FistState.GrabEnv || rightFistState == FistState.GrabEnv)
        {
            WholeVelocityBeforeJump.Params @params = new WholeVelocityBeforeJump.Params();
            @params.leftFistState = leftFistState;
            @params.rightFistState = rightFistState;
            @params.leftFistVelocity = fistVelocity.left;
            @params.rightFistVelocity = fistVelocity.right;
            @params.leftFistOffset = fistOffset.left;
            @params.rightFistOffset = fistOffset.right;
            @params.wholeMatrix = wholeMatrix;
            wholeVelocityBeforeJump.FixedUpdateManually(@params);
        }
        else
        {
            if (leftFistState.pre == FistState.GrabEnv || rightFistState.pre == FistState.GrabEnv)
            {
                wholeVelocityWhileJumping.StartJump(wholeVelocityBeforeJump);
            }
            else
            {
                WholeVelocityWhileJumping.Params @params = new WholeVelocityWhileJumping.Params();
                @params.leftFistState = leftFistState;
                @params.rightFistState = rightFistState;
                @params.footState = footState;
                wholeVelocityWhileJumping.FixedUpdateManually(@params);
            }
        }

    }

    void FU_Whole_Offset()
    {
        /*
            Summary
            首先区分有无GrabEnv。
            当手有GrabEnv时，用offset计算。
            没有时，用speed计算。

            在这之前，可以先准备些常用参数。
        */

        /*
            0. 准备         
         */
        Matrix4x4 wholeMatrix = Matrix4x4.TRS(whole.transform.position, whole.transform.localRotation, whole.transform.localScale);
        Vector2 wholeOffset = new Vector2();
        /*
            1. 有GrabEnv 
            有GrabEnv时，不考虑任何碰撞。
            根据HandState分情况讨论。
        */
        if (rightFistState == FistState.GrabEnv || leftFistState == FistState.GrabEnv)
        {
            Vector2 rightFistOffset = new Vector2();
            Vector2 leftFistOffset = new Vector2();
            if (rightFistState == FistState.GrabEnv && leftFistState != FistState.GrabEnv)
            {
                wholeOffset = wholeMatrix * -rightFistOffset;
            }
            if (rightFistState != FistState.GrabEnv && leftFistState == FistState.GrabEnv)
            {
                wholeOffset = wholeMatrix * -leftFistOffset;
            }
            if (rightFistState == FistState.GrabEnv && leftFistState == FistState.GrabEnv)
            {
                /*
                    注意，offset的方向，可能和fistVelocity不同。
                    比如我斜向移动，但是到头了，于是结果可能是横向移动。
                */

                float smallerDis = 0;
                float biggerDis = 0;
                GameObject smallerFist = rightFist;
                GameObject biggerFist = rightFist;
                {
                    float rhDis = rightFistOffset.magnitude;
                    float lhDis = leftFistOffset.magnitude;
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
                }

                if (MyTool.FloatEqual0p001(biggerDis, 0))
                {
                    wholeOffset = new Vector2();
                }
                else if (MyTool.FloatEqual0p001(smallerDis, 0) && !MyTool.FloatEqual0p001(biggerDis, 0))
                {
                    //如果一只手的移动是0
                    //身体和移动的那只手，等效于单手移动
                    //另一只手减速
                    if (biggerFist == rightFist)
                    {
                        wholeOffset = wholeMatrix * -rightFistOffset;
                    }
                    if (biggerFist == leftFist)
                    {
                        wholeOffset = wholeMatrix * -leftFistOffset;
                    }
                }
                else
                {
                    /*  
                        双手移动都不是0
                            其实分2种情况，一种是都可以移动到dis，一种是不行，但在计算时没有区别

                        注意，offset的方向，可能和fistVelocity不同。
                            比如，极端情况。按键是右上，但两只手都没向右上移动，一只手向右，一只手向上。
                        这时应如何计算呢？
                        身体的移动，取决于2个dis，x方向上的最大分量和y方向的最大分量。
                    */
                    Vector2 Offset = new Vector2();
                    if (Mathf.Abs(Vector2.Dot(rightFistOffset, Vector2.right))
                        >= Mathf.Abs(Vector2.Dot(leftFistOffset, Vector2.right))
                       )
                    {
                        Offset += Vector2.Dot(rightFistOffset, Vector2.right) * Vector2.right;
                    }
                    else
                    {
                        Offset += Vector2.Dot(leftFistOffset, Vector2.right) * Vector2.right;
                    }
                    if (Mathf.Abs(Vector2.Dot(rightFistOffset, Vector2.down))
                        >= Mathf.Abs(Vector2.Dot(leftFistOffset, Vector2.down))
                       )
                    {
                        Offset += Vector2.Dot(rightFistOffset, Vector2.down) * Vector2.down;
                    }
                    else
                    {
                        Offset += Vector2.Dot(leftFistOffset, Vector2.down) * Vector2.down;
                    }
                    wholeOffset = wholeMatrix * -Offset;
                }
            }

            whole.transform.position += (Vector3)wholeOffset;
            return;
        }
        /*
            2. 无GrabEnv 

            没有时，只考虑向下运动的碰撞。
                没有考虑从左右碰撞collider的问题。感觉暂时不需要。

            ----------------
            水平方向有摩擦力减速
            只有在地面时，才有摩擦力减速
                向上无摩擦力
                向下，只有接触地面时有摩擦力
         
         */
        {
            //2.1 无向下运动
            Vector2 velocity = wholeVelocityWhileJumping.velocity;
            float velocityDownPart = Vector2.Dot(velocity, Vector2.down);
            float velocityRightPart = Vector2.Dot(velocity, Vector2.right);
            if (velocityDownPart <= 0)
            {
                //whole position
                wholeOffset = velocity * Time.fixedDeltaTime;
                return;
            }
            //2.2 有向下运动
            if (footState == FootState.Surface || footState == FootState.EnvGround)
            {
                wholeOffset = velocityRightPart * Time.fixedDeltaTime * Vector2.right;
            }
            else if (footState == FootState.Air)
            {
                float downDis = velocityDownPart * Time.fixedDeltaTime;

                //只用2个点做Raycast，不能保证，但先这样
                Vector2 pointBL = bottomLeftPoint.transform.position;
                Vector2 pointBR = bottomRightPoint.transform.position;
                RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis);
                RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis);

                if (!hitBL.collider && !hitBR.collider)
                {
                    //whole position
                    wholeOffset = velocity * Time.fixedDeltaTime;
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

                    wholeOffset = velocityRightPart * Time.fixedDeltaTime * Vector2.right
                                  + new Vector2(0, -downDis);
                }
            }
            else if (footState == FootState.EnvRock)
            {
                float downDis = velocityDownPart * Time.fixedDeltaTime;

                //只用2个点做Raycast，不能保证，但先这样
                //当前是EnvRock，如果经过一个Air，再出现EnvRock。这里的算法就会有问题。但一帧移动的很少，目前应该不会出现。先不管它。
                int layerMaskGround = LayerMask.GetMask("EnvGround");
                Vector2 pointBL = bottomLeftPoint.transform.position;
                Vector2 pointBR = bottomRightPoint.transform.position;
                RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis, layerMaskGround);
                RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis, layerMaskGround);

                if (!hitBL.collider && !hitBR.collider)
                {
                    wholeOffset = velocity * Time.fixedDeltaTime;
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

                    wholeOffset = velocityRightPart * Time.fixedDeltaTime * Vector2.right
                                  + new Vector2(0, -downDis);
                }
            }
            whole.transform.position += (Vector3)wholeOffset;
            return;
        }
       
    }

    void FU_Whole_OffsetReflectVelocity()
    {
        
    }
}