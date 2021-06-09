using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        if (rightFistState == FistState.GrabEnv && leftFistState == FistState.GrabEnv)
        {
            if (fistVelocity.left.direction.magnitude != 0 
                && fistVelocity.right.direction.magnitude != 0
                && (fistVelocity.left.direction != fistVelocity.right.direction) )
            {
                //移动为0，代码do nothing
            }
            else
            {
                HandControlTool.TryMove(leftParams, leftMovePossible, out leftOffset);
                HandControlTool.TryMove(rightParams, rightMovePossible, out rightOffset);
            }
        }
        else
        {
            HandControlTool.TryMove(leftParams, leftMovePossible, out leftOffset);
            HandControlTool.TryMove(rightParams, rightMovePossible, out rightOffset);
        }

        fistOffset.left = leftOffset;
        fistOffset.right = rightOffset;
    }

    void FU_FistOffetInfluenceSpeed()
    {
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

    void FU_Whole()
    {
        FU_Whole_Velocity();
        FU_Whole_Offset();
        //Apply
        whole.transform.position += (Vector3)wholeOffset.offset;
    }

    void FU_Whole_Velocity()
    {
        Matrix4x4 wholeMatrix = Matrix4x4.TRS(whole.transform.position, whole.transform.localRotation, whole.transform.localScale);
        if (leftFistState == FistState.GrabEnv || rightFistState == FistState.GrabEnv)
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
        if (rightFistState == FistState.GrabEnv || leftFistState == FistState.GrabEnv)
        {
            Vector2 leftFistOffset = fistOffset.left;
            Vector2 rightFistOffset = fistOffset.right;
            if (leftFistState == FistState.GrabEnv && rightFistState != FistState.GrabEnv)
            {
                tempWholeOffset = wholeMatrix * -leftFistOffset;
            }
            if (rightFistState == FistState.GrabEnv && leftFistState != FistState.GrabEnv)
            {
                tempWholeOffset = wholeMatrix * -rightFistOffset;
            }
            if (leftFistState == FistState.GrabEnv && rightFistState == FistState.GrabEnv)
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

                    //只用2个点做Raycast，不能保证，但先这样
                    Vector2 pointBL = bottomLeftPoint.transform.position;
                    Vector2 pointBR = bottomRightPoint.transform.position;
                    RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), downDis, LMRockOrGround);
                    RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), downDis, LMRockOrGround);

                    if (!hitBL.collider && !hitBR.collider)
                    {
                        //whole position
                        tempWholeOffset = velocity * Time.fixedDeltaTime;
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

                        tempWholeOffset = velocityRightPart * Time.fixedDeltaTime * Vector2.right
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
                        tempWholeOffset = velocity * Time.fixedDeltaTime;
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