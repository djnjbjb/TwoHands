using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Ludo.Extensions;
using Ludo.TwoHandsWar.PlayerInput;

public partial class PlayerControl : MonoBehaviour
{
    /*
    -------------------------------------------------------------
                            Singleton
    -------------------------------------------------------------
    */
    public static PlayerControl playerControl = null;

    /*
    -------------------------------------------------------------
                            逻辑
    -------------------------------------------------------------
    */
    /*                      1.Setting                          */
    //Setting - Game
    float footState_surfaceUpDownOffset = 0.01f;
    float footState_surfaceLeftRightInaccuracy = 0.01f;
    int moveDirectionReverseIfGrabEnv;  //从PlayerControlStaticSetting获取值
    bool jumpSpeedAddedWhenSlanting = true;

    //Setting - Const
    float wholeJumpSpeedHistoryTime = 0.701f;
    float wholeJumpSpeedZeroCutTime = 0.4f;
    float wholeJumpSpeedSimpleDirectionTime = 0.101f;
    float wholeJumpSpeedComplexDirectionTime = 0.201f;
    
    float wholeSpeedDownPartMax = 48f;
    float wholeFistSpeedRatio = 1.634f;
    float gravity = 30.864f;
    float friction = 32f; //摩擦力。 摩擦力是早先的设定。现在没用了，水平位移直接停止。

    /*                      2.GameObject                         */
    GameObject whole;
    public GameObject leftFist { get; private set; }
    public GameObject rightFist { get; private set; }
    GameObject bottomLeftPoint;
    GameObject bottomMiddlePoint;
    GameObject bottomRightPoint;
    GameObject arrowSign;

    /*                      3.Sections                          */
    //Sections - Function
    HandRepresent handRepresent;
    [NonSerialized] public HandControlFade handFade;
    [NonSerialized] public HCAudio hcAudio;

    //Sections - MotionPre
    FistStatePlus leftFistState = FistState.Free;
    FistStatePlus rightFistState = FistState.Free;
    GameObject leftGrabedStuff = null;
    GameObject rightGrabedStuff = null;
    FootStatePlus footState;

    //Sections - MotionFist
    TwoFistVelocity fistVelocity;
    FistVelocity leftFistVelocity;
    FistVelocity rightFistVelocity;
    TwoFistOffset fistOffset;

    //Sections - MotionWhole
    WholeVelocityBeforeJump wholeVelocityBeforeJump;
    WholeVelocityWhileJumping wholeVelocityWhileJumping;
    WholeOffset wholeOffset;
    [NonSerialized] public WholeState wholeState;


    /*                      4.Others                          */
    //写代码方便，常用的const
    float length;

    /*
    -------------------------------------------------------------
                                纯表现
    -------------------------------------------------------------
    */

    /*  
    目前，在HandControlFade中，会改变颜色。（直接改变对应的SpriteRenderer的颜色）
        所以，这里只是记录SpriteRenderer。在使用的时候，从SpriteRenderer取颜色。
    */
    SpriteRenderer normalColorRender;
    SpriteRenderer pressedColorRender;

    private void Awake()
    {
        playerControl = this;
    }
    void Start()
    {
        Init();
    }

    void Init()
    {
        //Setting
        moveDirectionReverseIfGrabEnv = PlayerControlStaticSetting.GetMoveDirectionReverseIfGrabEnv();

        //GameObject
        whole = this.gameObject;
        rightFist = whole.transform.Find("RHFist").gameObject;
        leftFist = whole.transform.Find("LHFist").gameObject;
        bottomLeftPoint = whole.transform.Find("BottomLeftPoint").gameObject;
        bottomMiddlePoint = whole.transform.Find("BottomMiddlePoint").gameObject;
        bottomRightPoint = whole.transform.Find("BottomRightPoint").gameObject;
        arrowSign = whole.transform.Find("Body").LudoFind("ArrowSign", includeInactive: true).gameObject;

        //Sections - Function
        handRepresent = new HandRepresent(whole);
        handFade = new HandControlFade(whole);
        hcAudio = new HCAudio();

        //Others - const
        float length1 = handRepresent.rightJoint1.transform.localScale.x / 2 + handRepresent.rightLine1.transform.localScale.x + handRepresent.rightJoint2.transform.localScale.x / 2;
        float length2 = handRepresent.rightJoint2.transform.localScale.x / 2 + handRepresent.rightLine2.transform.localScale.x + rightFist.transform.localScale.x / 2;
        length = length1 + length2;

        //Sections - MotionPre
        footState = new FootStatePlus(FootState.Air, footState_surfaceUpDownOffset, footState_surfaceLeftRightInaccuracy);

        //Sections - MotionFist
        fistVelocity = new TwoFistVelocity(length);
        leftFistVelocity = fistVelocity.left;
        rightFistVelocity = fistVelocity.right;
        fistOffset = new TwoFistOffset();

        //Sections - Whole
        wholeVelocityBeforeJump =
            new WholeVelocityBeforeJump( Mathf.Max(wholeJumpSpeedHistoryTime, wholeJumpSpeedZeroCutTime, wholeJumpSpeedSimpleDirectionTime) );
        wholeVelocityWhileJumping = 
            new WholeVelocityWhileJumping(gravity, friction, wholeSpeedDownPartMax, wholeFistSpeedRatio,
                                          historyTime: wholeJumpSpeedHistoryTime, 
                                          historyZeroCutTime: wholeJumpSpeedZeroCutTime, 
                                          historySimpleHistoryTime: wholeJumpSpeedSimpleDirectionTime, 
                                          historyComplexHistoryTime: wholeJumpSpeedComplexDirectionTime,
                                          jumpSpeedAddedWhenSlanting: jumpSpeedAddedWhenSlanting);
        wholeOffset = new WholeOffset();

        //纯表现
        normalColorRender = this.transform.LudoFind("FistNormalColor", includeInactive: true).gameObject.GetComponent<SpriteRenderer>();
        pressedColorRender = this.transform.LudoFind("FistPressedColor", includeInactive: true).gameObject.GetComponent<SpriteRenderer>();

        //--CheckSetting
        CheckSetting();

        //--DebugShow
        YDebugPanelShow_Init();
    }

    void CheckSetting()
    {
        if (wholeJumpSpeedSimpleDirectionTime <= wholeJumpSpeedZeroCutTime
            && wholeJumpSpeedZeroCutTime <= wholeJumpSpeedHistoryTime)
        {

        }
        else
        {
            Debug.LogWarning("WholeJumpSpeed Time Setting, 值的大小顺序不对");
        }
    }

    void YDebugPanelShow_Init()
    {
        Y.DebugPanel.Log(_message: length, _name: "Length", _category: "Player");
    }
}
