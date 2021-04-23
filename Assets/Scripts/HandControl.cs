using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public partial class HandControl : MonoBehaviour
{
    //Setting
    float surfaceTolerance = 0.01f;
    int moveDirectionReverseIfGrabEnv = 1;

    //GameObject
    [SerializeField] GameObject whole;
    public GameObject leftFist { get; private set; }
    public GameObject rightFist { get; private set; }
    GameObject bottomLeftPoint;
    GameObject bottomRightPoint;
    HandRepresent handRepresent;
    //常用const
    float length;

    //Pre
    FistStatePlus leftFistState = FistState.Free;
    FistStatePlus rightFistState = FistState.Free;
    GameObject leftGrabedStuff = null;
    GameObject rightGrabedStuff = null;
    FootStatePlus footState;

    //Fist Move Variable
    TwoFistVelocity fistVelocity;
    FistVelocity leftFistVelocity;
    FistVelocity rightFistVelocity;
    TwoFistOffset fistOffset;

    //Whole Move Variable
    WholeVelocityBeforeJump wholeVelocityBeforeJump;
    WholeVelocityWhileJumping wholeVelocityWhileJumping;
    WholeOffset wholeOffset;
    //const
    float wholeJumpSpeedCutTime = 0.151f;
    float wholeJumpSpeedHistoryTime = 0.21f;
    float wholeJumpSpeedFastHistoryTime = 0.11f;
    float wholeSpeedDownPartMax = 48f;
    float wholeFistSpeedRatio = 24f / 13f;
    float gravity = 48f;
    float friction = 32f; //摩擦力 //摩擦力现在不被用到了，水平位移直接停止

    void Start()
    {
        Init();
    }

    void Init()
    {
        //GameObject
        rightFist = whole.transform.Find("RHFist").gameObject;
        leftFist = whole.transform.Find("LHFist").gameObject;
        bottomLeftPoint = whole.transform.Find("BottomLeftPoint").gameObject;
        bottomRightPoint = whole.transform.Find("BottomRightPoint").gameObject;
        handRepresent = new HandRepresent(whole);
        //常用const
        float length1 = handRepresent.rightJoint1.transform.localScale.x / 2 + handRepresent.rightLine1.transform.localScale.x + handRepresent.rightJoint2.transform.localScale.x / 2;
        float length2 = handRepresent.rightJoint2.transform.localScale.x / 2 + handRepresent.rightLine2.transform.localScale.x + rightFist.transform.localScale.x / 2;
        length = length1 + length2;

        //Pre
        footState = new FootStatePlus(FootState.Air, surfaceTolerance);

        //Fist Move
        fistVelocity = new TwoFistVelocity(length);
        leftFistVelocity = fistVelocity.left;
        rightFistVelocity = fistVelocity.right;
        fistOffset = new TwoFistOffset();

        //Whole Move
        wholeVelocityBeforeJump =
            new WholeVelocityBeforeJump( Mathf.Max(wholeJumpSpeedHistoryTime, wholeJumpSpeedCutTime, wholeJumpSpeedFastHistoryTime) );
        wholeVelocityWhileJumping = 
            new WholeVelocityWhileJumping(gravity, friction, wholeSpeedDownPartMax, wholeFistSpeedRatio,
                                          speedCutTime: wholeJumpSpeedCutTime, historyTime: wholeJumpSpeedHistoryTime, fastHistoryTime: wholeJumpSpeedFastHistoryTime);
        wholeOffset = new WholeOffset();

        //CheckSetting
        CheckSetting();
        //Debug
        Y.DebugPanel.Log("Length", "常量", length);
    }

    void CheckSetting()
    {
        if (wholeJumpSpeedFastHistoryTime <= wholeJumpSpeedCutTime
            && wholeJumpSpeedCutTime <= wholeJumpSpeedHistoryTime)
        {

        }
        else
        {
            Debug.LogWarning("WholeJumpSpeed Time Setting, 值的大小顺序不对");
        }
    }
}
