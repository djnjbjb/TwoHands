using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Ludo.Utility;
using HandControlTool;

public partial class HandControl : MonoBehaviour
{

    void ___________FU_____________()
    {

    }
    
    void FixedUpdate()
    {
        Ludo.LogFile.Log($"😃 Fixed Update, Time {Time.fixedTime}");

        FU_Pre();

        FU_Main();

        handRepresent.Refresh();

        hcAudio.PlayHandRelated(leftState: leftFistState, leftStatePre: leftFistState.pre,
                        rightState: rightFistState, rightStatePre: rightFistState.pre,
                        leftOffset: fistOffset.left, rightOffset: fistOffset.right);
    }

    void FU_Main()
    {
        FU_Fist();
        FU_Whole();
        //Log
        Ludo.LogFile.Log($"Whole Velocity Before Jump, speed:{wholeVelocityBeforeJump.speed}, direction: ({wholeVelocityBeforeJump.direction.x}, {wholeVelocityBeforeJump.direction.y})");
        Ludo.LogFile.Log($"Whole Velocity While Jump, speed:{wholeVelocityWhileJumping.speed}, direction: ({wholeVelocityWhileJumping.direction.x}, {wholeVelocityWhileJumping.direction.y})");

        Ludo.LogFile.Log($"leftOffset: ({fistOffset.left.x}, {fistOffset.left.y})");
        Ludo.LogFile.Log($"rightOffset: ({fistOffset.right.x}, {fistOffset.right.y})");
        Ludo.LogFile.Log($"leftSpeed: {leftFistVelocity.speed}");
        Ludo.LogFile.Log($"rightSpeed: {rightFistVelocity.speed}");

        //Debug
        {
            Y.DebugPanel.Log("leftSpeed", "HandControl", fistVelocity.left.speed);
            Y.DebugPanel.Log("rightSpeed", "HandControl", fistVelocity.right.speed);
            Y.DebugPanel.Log("leftOffset", "HandControl", $"{fistOffset.left.x}, {fistOffset.left.y}");
            Y.DebugPanel.Log("rightOffset", "HandControl", $"{fistOffset.right.x}, {fistOffset.right.y}");
            
            Y.DebugPanel.Log("WholeVelocity Before Jump", "HandControl", $"{wholeVelocityBeforeJump.speed}");
            Y.DebugPanel.Log("WholeVelocity WhileJumping", "HandControl", $"{wholeVelocityWhileJumping.speed}");
            Y.DebugPanel.Log("wholeOffset", "HandControl", $"{wholeOffset.offset.x}, {wholeOffset.offset.y}");
        }

    }

    void FU_Fist()
    {
        FU_FistSpeed();
        FU_FistOffset();
        FU_FistOffetInfluenceSpeed();
        //Apply FistOffset
        this.leftFist.transform.localPosition += (Vector3)fistOffset.left;
        this.rightFist.transform.localPosition += (Vector3)fistOffset.right;
        fistVelocity.FixedUpdateHistoryManually();
        fistOffset.FixedUpdateHistoryManually();
    }

    void FU_FistSpeed()
    {
        FistState leftFistState = this.leftFistState;
        FistState rightFistState = this.rightFistState;
        FistGrabStuffEnvHelper(ref leftFistState, ref rightFistState);

        /*Fist Speed
            输入
                FistState
                HKey
                此外，还需要加上ifGrabMoveReverse来确定方向
            输出
                FistSpeed
        */

        ParameterForFistVelocity leftParameter = new ParameterForFistVelocity
        {
            anyKeyHold = HKey.lDown || HKey.lUp || HKey.lRight || HKey.lLeft,
            keyDir = HKey.lMvDir,
            handState = leftFistState,
            moveDir = HKey.lMvDir * (leftFistState == FistState.GrabEnv ? moveDirectionReverseIfGrabEnv : 1)
        };
        ParameterForFistVelocity rightParameter = new ParameterForFistVelocity
        {
            anyKeyHold = HKey.rDown || HKey.rUp || HKey.rRight || HKey.rLeft,
            keyDir = HKey.rMvDir,
            handState = rightFistState,
            moveDir = HKey.rMvDir * (rightFistState == FistState.GrabEnv ? moveDirectionReverseIfGrabEnv : 1)
        };
        fistVelocity.RefreshSpeed(leftParameter: leftParameter, rightParameter: rightParameter);
    }

    void FU_FistOffset()
    {
        FistState leftFistState = this.leftFistState;
        FistState rightFistState = this.rightFistState;
        FistGrabStuffEnvHelper(ref leftFistState, ref rightFistState);


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

        Tool.TryMoveRegionParams leftParams = new Tool.TryMoveRegionParams();
        leftParams.fistPos = leftFist.transform.localPosition;
        leftParams.joint1Pos = handRepresent.leftJoint1.transform.localPosition;
        leftParams.length = length;
        Tool.TryMoveRegionParams rightParams = new Tool.TryMoveRegionParams();
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

        /*
            offset的计算，要分很多情况
            1.双手都不GrabEnv
            2.单手GrabEnv
            3.双手GrabEnv
                3.1 双手都有方向，但不同
                3.2 一手有方向，一手没有
                3.3 双手都有方向，但不同
                
                设计：
                如果两只手有方向，且同向，则同向移动。
                如果一直有有方向，一只手没有方向，单手移动。
                其他情况不移动。
            
            但，实际上，在计算了speed之后，
            除了3.3的移动为0外，其他的移动都可以用TryMove计算。
        */
        if (leftFistState == FistState.GrabEnv && rightFistState == FistState.GrabEnv)
        {
            if (fistVelocity.left.direction.magnitude != 0 
                && fistVelocity.right.direction.magnitude != 0
                && (fistVelocity.left.direction != fistVelocity.right.direction) )
            {
                //移动为0，代码do nothing
            }
            else
            {
                Tool.TryMove(leftParams, leftMovePossible, out leftOffset);
                Tool.TryMove(rightParams, rightMovePossible, out rightOffset);
            }
        }
        else
        {
            Tool.TryMove(leftParams, leftMovePossible, out leftOffset);
            Tool.TryMove(rightParams, rightMovePossible, out rightOffset);
        }

        fistOffset.left = leftOffset;
        fistOffset.right = rightOffset;
    }

    void FU_FistOffetInfluenceSpeed()
    {
        FistState leftFistState = this.leftFistState;
        FistState rightFistState = this.rightFistState;
        FistGrabStuffEnvHelper(ref leftFistState, ref rightFistState);

        ParameterForFistVelocity leftParameter = new ParameterForFistVelocity
        {
            anyKeyHold = HKey.lDown || HKey.lUp || HKey.lRight || HKey.lLeft,
            keyDir = HKey.lMvDir,
            handState = leftFistState,
            moveDir = HKey.lMvDir * (leftFistState == FistState.GrabEnv ? moveDirectionReverseIfGrabEnv : 1)
        };
        ParameterForFistVelocity rightParameter = new ParameterForFistVelocity
        {
            anyKeyHold = HKey.rDown || HKey.rUp || HKey.rRight || HKey.rLeft,
            keyDir = HKey.rMvDir,
            handState = rightFistState,
            moveDir = HKey.rMvDir * (rightFistState == FistState.GrabEnv ? moveDirectionReverseIfGrabEnv : 1)
        };
        fistVelocity.OffsetReflectSpeed(leftParameter: leftParameter, leftOffset: fistOffset.left,
                                        rightParameter: rightParameter, rightOffset: fistOffset.right);
    }

    void FistGrabStuffEnvHelper(ref FistState leftFistStateTemp, ref FistState rightFistStateTemp)
    {
        //GrabStuffEnv的特殊处理
        /*
            GrabStuffEnv的情况比较好处理
            根据手的运动方向，可以GrabStuffEnv把转成Env或Stuff。
            需要注意，无方向时，等价于Env。
        */
        if (leftFistStateTemp == FistState.GrabStuffEnv)
        {
            var leftStuffValue = leftGrabedStuff.GetComponent<Stuff.Sword>().GetValueForFist_AtFUPre();
            var stuffDirection = leftStuffValue.direction.normalized;
            var moveDir = HKey.lMvDir.normalized;
            if (FloatEqual_WithIn0p001(Vector2.Angle(moveDir, stuffDirection), 0) ||
                    FloatEqual_WithIn0p001(Vector2.Angle(moveDir, stuffDirection), 180))
            {
                leftFistStateTemp = FistState.GrabStuff;
            }
            else
            {
                leftFistStateTemp = FistState.GrabEnv;
            }
        }
        if (rightFistStateTemp == FistState.GrabStuffEnv)
        {
            var rightStuffValue = rightGrabedStuff.GetComponent<Stuff.Sword>().GetValueForFist_AtFUPre();
            var stuffDirection = rightStuffValue.direction.normalized;
            var moveDir = HKey.rMvDir.normalized;
            if (FloatEqual_WithIn0p001(Vector2.Angle(moveDir, stuffDirection), 0) ||
                    FloatEqual_WithIn0p001(Vector2.Angle(moveDir, stuffDirection), 180))
            {
                rightFistStateTemp = FistState.GrabStuff;
            }
            else
            {
                rightFistStateTemp = FistState.GrabEnv;
            }
        }
    }

    void FU_Whole()
    {
        /*                   ---加入GrabEnv后的变化---
            
            这里有一个概念

            早先
            有GrabEnv表示一种情况，BeforeJump。
                （可以看到，这时BeforeJump这个名字就不太好。）
                物体的速度以某种形式去作用。物体的offset使用手的offset计算。
            无GrabEnv表示另一种情况，WhileJumping。
                物体的速度，以某种形式去作用。物体的Offset，通过速度，以某种方式来计算。

            加入StuffEnv后
            目前想要简单的处理
            有GrabEnv/GrabStuffEnv表示一种情况，BeforeJump。
            无GrabEnv/GrabStuffEnv表示一种情况，WhileJumping。
                这里对原来的模式，有个破坏。
                在原来的模式中，对于BeforeJump，物体的运动总是与GrabEnv的Offset反向。由此，还有些略微复杂的逻辑，比如速度什么的。
                加入GrabStuff后则变了。对于GrabStuffEnv，在他等效于GrabStuff时，物体的运动处理很多与Stuff相同。
                似乎，把这个问题处理好，一切就解决了。

        */
        FU_Whole_Velocity();
        FU_Whole_Offset();
        //Apply
        whole.transform.position += (Vector3)wholeOffset.offset;
    }

    void FU_Whole_Velocity()
    {
        FistState leftFistState_GrabStuffEnvProcessed = this.leftFistState;
        FistState rightFistState_GrabStuffEnvProcessed = this.rightFistState;
        FistGrabStuffEnvHelper(ref leftFistState_GrabStuffEnvProcessed, ref rightFistState_GrabStuffEnvProcessed);

        Matrix4x4 wholeMatrix = Matrix4x4.TRS(whole.transform.position, whole.transform.localRotation, whole.transform.localScale);
        if (leftFistState.IsGrabing_Env_StuffEnv() || rightFistState.IsGrabing_Env_StuffEnv())
        {
            WholeVelocityBeforeJump.Params @params = new WholeVelocityBeforeJump.Params();
            @params.leftFistState = leftFistState;
            @params.rightFistState = rightFistState;
            @params.leftFistState_GrabStuffEnvProcessed = leftFistState_GrabStuffEnvProcessed;
            @params.rightFistState_GrabStuffEnvProcessed = rightFistState_GrabStuffEnvProcessed;
            @params.leftFistVelocity = fistVelocity.left;
            @params.rightFistVelocity = fistVelocity.right;
            @params.leftFistOffset = fistOffset.left;
            @params.rightFistOffset = fistOffset.right;
            @params.wholeMatrix = wholeMatrix;
            wholeVelocityBeforeJump.FixedUpdateManually(@params);
        }
        else
        {
            if (leftFistState.pre.IsGrabing_Env_StuffEnv() || rightFistState.pre.IsGrabing_Env_StuffEnv())
            {
                wholeVelocityWhileJumping.StartJumpWithLog(wholeVelocityBeforeJump, wholeOffset);
            }

            Ludo.LogFile.Log($"😊 Whole Velocity While Jump Before Fix, speed:{wholeVelocityWhileJumping.speed}, direction: ({wholeVelocityWhileJumping.direction.x}, {wholeVelocityWhileJumping.direction.y})");

            WholeVelocityWhileJumping.Params @params = new WholeVelocityWhileJumping.Params();
            @params.leftFistState = leftFistState;
            @params.rightFistState = rightFistState;
            @params.footState = footState;
            wholeVelocityWhileJumping.FixedUpdateManually(@params);
            
        }
    }

    void FU_Whole_Offset()
    {
        /*
         GrabStuffEnv带来的变化

            在判断用offset还是速度计算时。GrabStuffEnv总是等效于GrabEnv。
            
            但是，使用Offset计算时，方法不同。
            GrabStuffEnv等效于Env时，计算Offset也等效于Env。
            等效于Stuff时，不对Offset产生影响。
            具体代码再看。
         */
        FistState leftFistState_GrabStuffEnvProcessed = this.leftFistState;
        FistState rightFistState_GrabStuffEnvProcessed = this.rightFistState;
        FistGrabStuffEnvHelper(ref leftFistState_GrabStuffEnvProcessed, ref rightFistState_GrabStuffEnvProcessed);

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
        Vector2 tempWholeOffset = new Vector2();
        /*
            1. 有GrabEnv 
            有GrabEnv时，不考虑任何碰撞。
            根据HandState分情况讨论。
        */
        if (leftFistState.IsGrabing_Env_StuffEnv() || rightFistState.IsGrabing_Env_StuffEnv())
        {
            Vector2 leftFistOffset = fistOffset.left;
            Vector2 rightFistOffset = fistOffset.right;
            /*
                leftFistState_GrabStuffEnvProcessed，在StuffEnv等效于Stuff时，对offset的作用为零。
                这里就不用处理了。默认tempWholeOffset就是零。
            */
            if (leftFistState_GrabStuffEnvProcessed == FistState.GrabEnv && rightFistState_GrabStuffEnvProcessed != FistState.GrabEnv)
            {
                tempWholeOffset = wholeMatrix * -leftFistOffset;
            }
            if (rightFistState_GrabStuffEnvProcessed == FistState.GrabEnv && leftFistState_GrabStuffEnvProcessed != FistState.GrabEnv)
            {
                tempWholeOffset = wholeMatrix * -rightFistOffset;
            }
            if (leftFistState_GrabStuffEnvProcessed == FistState.GrabEnv && rightFistState_GrabStuffEnvProcessed == FistState.GrabEnv)
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

                if (Ludo.Utility.FloatEqual_WithIn0p001(biggerDis, 0))
                {
                    tempWholeOffset = new Vector2();
                }
                else if (Ludo.Utility.FloatEqual_WithIn0p001(smallerDis, 0) && !Ludo.Utility.FloatEqual_WithIn0p001(biggerDis, 0))
                {
                    //如果一只手的移动是0
                    //身体和移动的那只手，等效于单手移动
                    //另一只手减速
                    if (biggerFist == rightFist)
                    {
                        tempWholeOffset = wholeMatrix * -rightFistOffset;
                    }
                    if (biggerFist == leftFist)
                    {
                        tempWholeOffset = wholeMatrix * -leftFistOffset;
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
                    tempWholeOffset = wholeMatrix * -Offset;
                }
            }
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
        else
        {

            //2.1 无向下运动
            Vector2 velocity = wholeVelocityWhileJumping.velocity;
            float velocityDownPart = Vector2.Dot(velocity, Vector2.down);
            float velocityRightPart = Vector2.Dot(velocity, Vector2.right);
            if (velocityDownPart < 0)
            {
                //whole position
                tempWholeOffset = velocity * Time.fixedDeltaTime;
            }
            //2.2 有向下运动
            else
            {
                if (footState == FootState.Surface || footState == FootState.EnvGround)
                {
                    tempWholeOffset = velocityRightPart * Time.fixedDeltaTime * Vector2.right;
                }
                else if (footState == FootState.Air)
                {
                    LayerMask LMRockOrGround = LayerMask.GetMask("EnvRock", "EnvGround");

                    float downDis = velocityDownPart * Time.fixedDeltaTime;

                    Vector2 pointBL = bottomLeftPoint.transform.position;
                    Vector2 pointBM = bottomMiddlePoint.transform.position;
                    Vector2 pointBR = bottomRightPoint.transform.position;
                    RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis, LMRockOrGround);
                    RaycastHit2D hitBM = Physics2D.Raycast(pointBM, new Vector2(0, -1), downDis, LMRockOrGround);
                    RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis, LMRockOrGround);

                    if (!hitBL.collider && !hitBM.collider  && !hitBR.collider)
                    {
                        //whole position
                        tempWholeOffset = velocity * Time.fixedDeltaTime;
                    }
                    else
                    {
                        float downDisBL = float.MaxValue;
                        float downDisBM = float.MaxValue;
                        float downDisBR = float.MaxValue;
                        if (hitBL.collider)
                        {
                            downDisBL = hitBL.distance;
                        }
                        if (hitBM.collider)
                        {
                            downDisBM = hitBM.distance;
                        }
                        if (hitBR.collider)
                        {
                            downDisBR = hitBR.distance;
                        }
                        downDis = Mathf.Min(downDisBL, downDisBM, downDisBR);

                        tempWholeOffset = velocityRightPart * Time.fixedDeltaTime * Vector2.right
                                        + new Vector2(0, -downDis);
                    }
                }
                else if (footState == FootState.EnvRock)
                {
                    float downDis = velocityDownPart * Time.fixedDeltaTime;

                    //当前是EnvRock，如果经过一个Air，再出现EnvRock。这里的算法就会有问题。但一帧移动的很少，目前应该不会出现。先不管它。
                    int layerMaskGround = LayerMask.GetMask("EnvGround");
                    Vector2 pointBL = bottomLeftPoint.transform.position;
                    Vector2 pointBM = bottomMiddlePoint.transform.position;
                    Vector2 pointBR = bottomRightPoint.transform.position;
                    RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis, layerMaskGround);
                    RaycastHit2D hitBM = Physics2D.Raycast(pointBM, new Vector2(0, -1), downDis, layerMaskGround);
                    RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis, layerMaskGround);

                    if (!hitBL.collider && !hitBM.collider && !hitBR.collider)
                    {
                        tempWholeOffset = velocity * Time.fixedDeltaTime;
                    }
                    else
                    {
                        float downDisBL = float.MaxValue;
                        float downDisBM = float.MaxValue;
                        float downDisBR = float.MaxValue;
                        if (hitBL.collider)
                        {
                            downDisBL = hitBL.distance;
                        }
                        if (hitBM.collider)
                        {
                            downDisBM = hitBM.distance;
                        }
                        if (hitBR.collider)
                        {
                            downDisBR = hitBR.distance;
                        }
                        downDis = Mathf.Min(downDisBL, downDisBM, downDisBR);

                        tempWholeOffset = velocityRightPart * Time.fixedDeltaTime * Vector2.right
                                        + new Vector2(0, -downDis);
                    }
                }
            }
            
            
        }
        this.wholeOffset.offset = tempWholeOffset;
    }

    void FU_Audio()
    {

    }
}