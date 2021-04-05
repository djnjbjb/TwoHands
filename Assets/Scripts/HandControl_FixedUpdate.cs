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
        return;
        
        FU_Pre();

        //-----------------------------------------------------------------------------
        
        //2 Fist
        FU_Fists(rightFistState, HKey.rMvDir, leftFistState, HKey.lMvDir);

        //Whole
        //FU_Whole();

        //-----------------------------------------------------------------------------

        //Late
        handRepresent.Refresh();
    }

    void ___________________________I()
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
    static Vector2 TryMove(Vector2 joint1Pos, float length, Vector2 fistPos, Vector2 moveVector)
    {
        float minY = joint1Pos.y - length;
        float maxY = joint1Pos.y + length;
        float minX = joint1Pos.x - length;
        float maxX = joint1Pos.x + length;

        Vector2 afterMove = fistPos + moveVector;
        afterMove.x = Mathf.Clamp(afterMove.x, minX, maxX);
        afterMove.y = Mathf.Clamp(afterMove.y, minY, maxY);

        return afterMove;
    }

    void ___________________________II()
    {

    }

    void FU_Fist_NoGrabEnv_Right(Vector2 mvDir, out Vector2 offset)
    {
        Vector2 fistPos = rightFist.transform.localPosition;
        Vector2 joint1Pos = handRepresent.rightJoint1.transform.localPosition;
        float speed = fistVelocity.right.speed;
        /*
         *  moveVecPossible = mvDir * speed * Time.fixedDeltaTime
         *  这个表示，但MvDir不动时，手立刻停止移动。
         */
        Vector2 moveVecPossible = mvDir * speed * Time.fixedDeltaTime;
        Vector2 fistPosAfterMove = TryMove(joint1Pos, length, fistPos, moveVecPossible);
        offset = fistPosAfterMove - fistPos;
    }

    void FU_Fist_NoGrabEnv_Left(Vector2 mvDir, out Vector2 offset)
    {
        Vector2 fistPos = leftFist.transform.localPosition;
        Vector2 joint1Pos = handRepresent.leftJoint1.transform.localPosition;
        float speed = fistVelocity.left.speed;
        Vector2 moveVecPossible = mvDir * speed * Time.fixedDeltaTime;
        Vector2 fistPosAfterMove = TryMove(joint1Pos, length, fistPos, moveVecPossible);
        offset = fistPosAfterMove - fistPos;
    }

    void FU_Fist_GrabEnv_Right(out Vector2 fistOffset, out Vector2 wholeOffset)
    {
        FU_Fist_NoGrabEnv_Right(ifGrabMoveReverse * HKey.rMvDir, out fistOffset);
        wholeOffset = wholeMatrix * -fistOffset;
    }

    void FU_Fist_GrabEnv_Left(out Vector2 fistOffset, out Vector2 wholeOffset)
    {
        FU_Fist_NoGrabEnv_Left(ifGrabMoveReverse * HKey.lMvDir, out fistOffset);
        wholeOffset = wholeMatrix * -fistOffset;
    }

    //void FU_Fist_GrabEnv_2Fist(
    //    out Vector2 rightFistOffset,  out float rightFistSpeed
    //    out Vector2 leftFistOffset,   out float leftFistSpeed)
    //{
    //    /*
    //        如果两只手同向，则同向移动
    //        如果一直有有方向，一只手没有方向，则按照有方向的移动
    //        其他情况不移动
    //    */
    //    Vector2 rhDirection = ifGrabMoveReverse * HKey.rMvDir;
    //    Vector2 lhDirection = ifGrabMoveReverse * HKey.lMvDir;
    //    if (rhDirection == lhDirection)
    //    {
    //        /*
    //            需要确定的有：
    //                手的offset、手的速度
    //                身体的offset、身体的速度
             
    //            -----------------------------------------
                
    //            先考量offset，再考量speed
    //            首先让二者速度相同，之后offset的计算与以前相同。
                
    //            然后再计量速度。
                
    //            -----------------------------------------
    //            注意，offset的方向，可能和fistVelocity不同。因为，比如我斜向移动，但是到头了，于是结果可能是横向移动
    //        */
    //        float rhFistSpeedPre = rightFistVelocity.magnitude;
    //        float lhFistSpeedPre = leftFistVelocity.magnitude;
    //        float fistSpeedPre = Mathf.Max(rhFistSpeedPre, lhFistSpeedPre);
    //        float fistSpeed = SpeedAccelerate(fistSpeedPre, fistSpeedAcceleration * 2);

    //        //根据速度确定offset
    //        float dis2x = fistSpeed * Time.fixedDeltaTime;
    //        Vector2 mvVector = dis2x * rhDirection;
    //        Vector2 rhFistPosOld = rightFist.transform.localPosition;
    //        Vector2 rhFistPosNew = TryMoveVectorRight(handRepresent.rightJoint1.transform.localPosition, rhFistPosOld, mvVector);
    //        Vector2 lhFistPosOld = leftFist.transform.localPosition;
    //        Vector2 lhFistPosNew = LeftFistMove(lhFistPosOld, mvVector);
    //        float rhDis = (rhFistPosNew - rhFistPosOld).magnitude;
    //        float lhDis = (lhFistPosNew - lhFistPosOld).magnitude;
    //        float smallerDis = 0;
    //        float biggerDis = 0;
    //        GameObject smallerFist = rightFist;
    //        GameObject biggerFist = rightFist;
    //        if (rhDis > lhDis)
    //        {
    //            smallerDis = lhDis;
    //            smallerFist = leftFist;
    //            biggerDis = rhDis;
    //            biggerFist = rightFist;
    //        }
    //        else
    //        {
    //            smallerDis = rhDis;
    //            smallerFist = rightFist;
    //            biggerDis = lhDis;
    //            biggerFist = leftFist;
    //        }
    //        if (MyTool.FloatEqual0p001(biggerDis, 0))
    //        {
    //            //如果双手移动都是0
    //            //位置都不动，手和身体都减速
    //            fistSpeed = SpeedDecelerate(fistSpeedPre);
    //            rightFistVelocity = fistSpeed * rhDirection;
    //            leftFistVelocity = fistSpeed * lhDirection;

    //            wholeGrabEnvOffset = new Vector2();
    //            wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
    //        }
    //        else if (MyTool.FloatEqual0p001(smallerDis, 0) && !MyTool.FloatEqual0p001(biggerDis, 0))
    //        {
    //            //如果一只手的移动是0
    //            //身体和移动的那只手，等效于单手移动
    //            //另一只手减速
    //            if (biggerFist == rightFist)
    //            {
    //                FU_Fist_GrabEnv_Right();
    //                leftFistVelocity = SpeedDecelerate(leftFistVelocity.magnitude) * lhDirection;
    //            }
    //            else if (biggerFist == leftFist)
    //            {
    //                FU_Fist_GrabEnv_Left();
    //                rightFistVelocity = SpeedDecelerate(rightFistVelocity.magnitude) * rhDirection;
    //            }
    //        }
    //        else
    //        {
    //            /*  
    //                双手移动都不是0
    //                    其实分2种情况，一种是都可以移动到dis2x，一种是不行，但在计算时没有区别
                    
    //                如果双手的移动方向一致：
    //                双手都移动到对应的dis。身体的offset是biggerDis。
    //                双手速度都增加到speed。身体速度则是-speed。

    //                注意，offset的方向，可能和fistVelocity不同。
    //                这时应如何计算呢？
    //                手的移动，依然是相应的dis。手的速度变化，比较难搞，就简单化，依然增加到speed。
    //                身体的移动，取决于2个dis，x方向上的最大分量和y方向的最大分量。身体的速度，也增加到speed，但反向。
    //            */
    //            Vector2 wholeOffset = new Vector2();
    //            Vector2 rhOffset = rhFistPosNew - rhFistPosOld;
    //            Vector2 lhOffset = lhFistPosNew - lhFistPosOld;
    //            if (Vector2.Dot(rhOffset, Vector2.right) >= Vector2.Dot(lhOffset, Vector2.right))
    //            {
    //                wholeOffset += Vector2.Dot(rhOffset, Vector2.right) * Vector2.right;
    //            }
    //            else
    //            {
    //                wholeOffset += Vector2.Dot(lhOffset, Vector2.right) * Vector2.right;
    //            }
    //            if (Vector2.Dot(rhOffset, Vector2.down) >= Vector2.Dot(lhOffset, Vector2.down))
    //            {
    //                wholeOffset += Vector2.Dot(rhOffset, Vector2.down) * Vector2.down;
    //            }
    //            else
    //            {
    //                wholeOffset += Vector2.Dot(lhOffset, Vector2.down) * Vector2.down;
    //            }

    //            //----------------------------------------------------
    //            rightFist.transform.localPosition = rhFistPosNew;
    //            leftFist.transform.localPosition = lhFistPosNew;
    //            rightFistVelocity = fistSpeed * rhDirection;
    //            leftFistVelocity = fistSpeed * lhDirection;

    //            wholeGrabEnvOffset = wholeMatrix * (-wholeOffset);
    //            /*
    //                如果双手移动同向，同时又和rhDirection不同，那么这里的速度会有问题
    //                应为 wholeMatrix * (-fistSpeed * wholeGrabEnvOffset.normal)
    //                暂时先不改
    //            */
    //            wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
    //        }
    //    }
    //    else if (rhDirection.magnitude != 0 && lhDirection.magnitude == 0)
    //    {
    //        FU_Fist_GrabEnv_Right();
    //        leftFistVelocity = SpeedDecelerate(leftFistVelocity.magnitude) * lhDirection;
    //    }
    //    else if (rhDirection.magnitude == 0 && lhDirection.magnitude != 0)
    //    {
    //        FU_Fist_GrabEnv_Left();
    //        rightFistVelocity = SpeedDecelerate(rightFistVelocity.magnitude) * rhDirection;
    //    }
    //    else
    //    {
    //        //do nothing
    //    }
    //}

    void FU_Fists(
        HandState rightFistState, Vector2 rightMoveDir,
        HandState leftFistState, Vector2 leftMoveDir
        )
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
        this.wholeGrabEnvOffset = new Vector2();

        //Hand
        if (rightFistState != HandState.GrabEnv)
        {
            Vector2 offset;

            FU_Fist_NoGrabEnv_Right(rightMoveDir, out offset);

            this.rightFist.transform.localPosition += (Vector3)offset;
        }
        if (leftFistState != HandState.GrabEnv)
        {
            Vector2 offset;

            FU_Fist_NoGrabEnv_Left(leftMoveDir, out offset);

            this.leftFist.transform.localPosition += (Vector3)offset;
        }

        
        if (rightFistState == HandState.GrabEnv && leftFistState != HandState.GrabEnv)
        {
            Vector2 fistOffset, wholeOffset;
            FU_Fist_GrabEnv_Right(out fistOffset, out wholeOffset);

            this.rightFist.transform.localPosition += (Vector3)fistOffset;
            whole.transform.position += (Vector3)wholeOffset;
        }
            
        if (rightFistState != HandState.GrabEnv && leftFistState == HandState.GrabEnv)
        {
            Vector2 fistOffset, wholeOffset;
            FU_Fist_GrabEnv_Left(out fistOffset, out wholeOffset);

            this.leftFist.transform.localPosition += (Vector3)fistOffset;
            whole.transform.position += (Vector3)wholeOffset;
        }
            
        if (rightFistState == HandState.GrabEnv && leftFistState == HandState.GrabEnv)
        {
            //FU_Fist_GrabEnv_2Fist();
        }
        
    }

    void ___________________________IIII()
    {

    }

    //void FU_Whole()
    //{
    //    /*
    //        当手有GrabEnv时，用offset计算。
    //        没有时，用speed计算。
            
    //        有GrabEnv时，不考虑任何碰撞。
    //        没有时，只考虑向下运动的碰撞。
    //            没有考虑从左右碰撞collider的问题。感觉暂时不需要。

    //        这部分需要搞定whole的position和speed。

    //        ----------------
    //        水平方向有摩擦力减速
    //        只有在地面时，才有摩擦力减速
    //            向上无摩擦力
    //            向下，只有接触地面时有摩擦力
    //    */

    //    Vector2 wholeVelocityNew;
    //    float wholeVelNew_DownPart;
    //    {
    //        float deltaSpeed = gravityAcceleration * Time.fixedDeltaTime;
    //        Vector2 deltaVelocity = deltaSpeed * Vector2.down;

    //        wholeVelocityNew = wholeVelocity + deltaVelocity;
    //        if (wholeVelocityNew.magnitude > wholeSpeedMax)
    //        {
    //            wholeVelocityNew = wholeVelocityNew.normalized * wholeSpeedMax;
    //        }
    //        wholeVelNew_DownPart = Vector2.Dot(wholeVelocityNew, Vector2.down);
    //    }
    //    /*
    //        1
    //        有GrabEnv 
    //    */
    //    if (rightFistState == HandState.GrabEnv || leftFistState == HandState.GrabEnv)
    //    {
    //        //whole position
    //        whole.transform.position += (Vector3)wholeGrabEnvOffset;
    //        //whole speed -> 不变
    //        return;
    //    }

    //    /*
    //        2
    //        无GrabEnv，无向下运动
    //    */
    //    if (wholeVelNew_DownPart <= 0)
    //    {
    //        //whole position
    //        whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
    //        //whole speed
    //        wholeVelocity = wholeVelocityNew;
    //        return;
    //    }

    //    /*
    //        3
    //        无GrabEnv，有向下运动
    //    */
    //    //3.1 get FootState
    //    FootState footState;
    //    {
    //        Vector2 pointBL = bottomLeftPoint.transform.position;
    //        Vector2 pointBR = bottomRightPoint.transform.position;
    //        RaycastHit2D hit = Physics2D.Linecast(pointBL, pointBR);

    //        Vector2 pointBLUp = (Vector2)bottomLeftPoint.transform.position + new Vector2(0, surfaceTolerance);
    //        Vector2 pointBRUp = (Vector2)bottomRightPoint.transform.position + new Vector2(0, surfaceTolerance);
    //        RaycastHit2D hitUp = Physics2D.Linecast(pointBLUp, pointBRUp);

    //        Vector2 pointBLDown = (Vector2)bottomLeftPoint.transform.position - new Vector2(0, surfaceTolerance);
    //        Vector2 pointBRDown = (Vector2)bottomRightPoint.transform.position - new Vector2(0, surfaceTolerance);
    //        RaycastHit2D hitDown = Physics2D.Linecast(pointBLDown, pointBRDown);

    //        if (!hitUp.collider && !hitDown.collider)
    //            footState = FootState.Air;
    //        else if (!hitUp.collider && hitDown.collider)
    //            footState = FootState.Surface;
    //        else
    //        {
    //            footState = FootState.EnvRock;

    //            int layerMaskGround = LayerMask.GetMask("EnvGround");
    //            RaycastHit2D hitDownGround = Physics2D.Linecast(pointBLDown, pointBRDown, layerMaskGround);
    //            RaycastHit2D hitUpGround = Physics2D.Linecast(pointBLUp, pointBRUp, layerMaskGround);
    //            if (hitDownGround.collider)
    //                footState = FootState.EnvGround;
    //        }
    //    }

    //    //3.2 do regarding to footState
    //    if (footState == FootState.Surface || footState == FootState.EnvGround)
    //    {
    //        float wholeVelNew_RightPart = Vector2.Dot(wholeVelocityNew, Vector2.right);
    //        wholeVelocityNew = wholeVelNew_RightPart * Vector2.right;
    //        //水平方向摩擦力减速
    //        float speedNew = wholeVelocityNew.magnitude - fistSpeedDeceleration * Time.fixedDeltaTime;
    //        speedNew = Mathf.Clamp(speedNew, 0f, wholeSpeedMax);
    //        wholeVelocityNew = speedNew * wholeVelocityNew.normalized;
    //        //whole position
    //        whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
    //        //whole speed
    //        wholeVelocity = wholeVelocityNew;

    //    }
    //    else if (footState == FootState.Air)
    //    {
    //        float downDis = wholeVelNew_DownPart * Time.fixedDeltaTime;

    //        //只用2个点做Raycast，不能保证，但先这样
    //        Vector2 pointBL = bottomLeftPoint.transform.position;
    //        Vector2 pointBR = bottomRightPoint.transform.position;
    //        RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis);
    //        RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis);

    //        if (!hitBL.collider && !hitBR.collider)
    //        {
    //            //whole position
    //            whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
    //            //whole speed
    //            wholeVelocity = wholeVelocityNew;
    //        }
    //        else
    //        {
    //            if (hitBL.collider)
    //            {
    //                downDis = hitBL.distance;
    //            }
    //            if (hitBR.collider)
    //            {
    //                if (hitBR.distance < downDis)
    //                {
    //                    downDis = hitBR.distance;
    //                }
    //            }

    //            float wholeVelNew_RightPart = Vector2.Dot(wholeVelocityNew, Vector2.right);
    //            wholeVelocityNew = wholeVelNew_RightPart * Vector2.right;
    //            //whole position
    //            whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime + new Vector3(0, -downDis, 0);
    //            //whole speed
    //            wholeVelocity = wholeVelocityNew;
    //        }
    //    }
    //    else if (footState == FootState.EnvRock)
    //    {
    //        float downDis = wholeVelNew_DownPart * Time.fixedDeltaTime;

    //        //只用2个点做Raycast，不能保证，但先这样
    //        //当前是EnvRock，如果经过一个Air，再出现EnvRock。这里的算法就会有问题。但一帧移动的很少，目前应该不会出现。先不管它。
    //        int layerMaskGround = LayerMask.GetMask("EnvGround");
    //        Vector2 pointBL = bottomLeftPoint.transform.position;
    //        Vector2 pointBR = bottomRightPoint.transform.position;
    //        RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis, layerMaskGround);
    //        RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis, layerMaskGround);

    //        if (!hitBL.collider && !hitBR.collider)
    //        {
    //            //whole position
    //            whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime;
    //            //whole speed
    //            wholeVelocity = wholeVelocityNew;
    //        }
    //        else
    //        {
    //            if (hitBL.collider)
    //            {
    //                downDis = hitBL.distance;
    //            }
    //            if (hitBR.collider)
    //            {
    //                if (hitBR.distance < downDis)
    //                {
    //                    downDis = hitBR.distance;
    //                }
    //            }

    //            float wholeVelNew_RightPart = Vector2.Dot(wholeVelocityNew, Vector2.right);
    //            wholeVelocityNew = wholeVelNew_RightPart * Vector2.right;
    //            //whole position
    //            whole.transform.position += (Vector3)wholeVelocityNew * Time.fixedDeltaTime + new Vector3(0, -downDis, 0);
    //            //whole speed
    //            wholeVelocity = wholeVelocityNew;
    //        }
    //    }
    //}
}