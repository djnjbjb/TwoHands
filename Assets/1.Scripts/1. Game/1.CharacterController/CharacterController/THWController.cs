using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Ludo.Extensions;
using Ludo.TwoHandsWar.THWControllerSub;

namespace Ludo.TwoHandsWar
{
    public partial class THWController : MonoBehaviour
    {
        #region Static
        public static THWController singleton;

        public static void SetSingleton(THWController controller)
        {
            singleton = controller;
        }
        #endregion
        /*
        -------------------------------------------------------------
                                逻辑
        -------------------------------------------------------------
        */
        /*                      1.Setting                          */
        //Setting - Game
        float footVerTolerance = 0.01f;
        float footHorTolerance = 0.01f;
        int dirReversedEnv {
            get {
                if (Circumstance.Setting.singleton.characterController.moveReversedGrabbing)
                    return -1;
                else
                    return 1;
            }
        }
        bool speedAddedDiagonal = true;

        //Setting - Const
        float wholeJumpHistoryTime = 0.701f;
        float wholeJumpZeroCut = 0.4f;
        float wholeJumpSimpleTime = 0.101f;
        float wholeJumpComplexTime = 0.201f;
    
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
        public ArmGesture armGesture { get; private set; }
        THWRepresent represent;
        [NonSerialized] public HandControlFade handFade;
        [NonSerialized] public HCAudio hcAudio;

        //Sections - MotionPre
        public FistStatePlus leftFistState { get; private set; } = FistState.Free;
        public FistStatePlus rightFistState { get; private set; } = FistState.Free;
        public GameObject leftGrabedStuff { get; private set; } = null;
        public GameObject rightGrabedStuff { get; private set; } = null;
        FootStatePlus footState;

        //Sections - MotionFist
        public TwoFistVelocity fistVelocity { get; private set; }
        FistVelocity leftFistVelocity;
        FistVelocity rightFistVelocity;
        TwoFistOffset fistOffset;

        //Sections - MotionWhole
        public WholeVelocityBeforeJump wholeVelocityBeforeJump { get; private set; }
        public WholeVelocityWhileJumping wholeVelocityWhileJumping { get; private set; }
        WholeOffset wholeOffset;
        public WholeState wholeState { get; private set; }

        /*                      4.Others                          */
        //写代码方便，常用的const
        float length;

        private void Awake()
        {
            SetSingleton(this);
        }
        void Start()
        {
            Init();
        }

        void Init()
        {
            //GameObject
            whole = this.gameObject;
            rightFist = whole.transform.Find("RHFist").gameObject;
            leftFist = whole.transform.Find("LHFist").gameObject;
            bottomLeftPoint = whole.transform.Find("BottomLeftPoint").gameObject;
            bottomMiddlePoint = whole.transform.Find("BottomMiddlePoint").gameObject;
            bottomRightPoint = whole.transform.Find("BottomRightPoint").gameObject;
            arrowSign = whole.transform.Find("Body").LudoFind("ArrowSign", includeInactive: true).gameObject;

            //Sections - Function
            armGesture = new ArmGesture(whole);
            represent = new THWRepresent(this, null);
            handFade = new HandControlFade(whole);
            hcAudio = new HCAudio();

            //Others - const
            float length1 = armGesture.rightJoint1.transform.localScale.x / 2 + armGesture.rightLine1.transform.localScale.x + armGesture.rightJoint2.transform.localScale.x / 2;
            float length2 = armGesture.rightJoint2.transform.localScale.x / 2 + armGesture.rightLine2.transform.localScale.x + rightFist.transform.localScale.x / 2;
            length = length1 + length2;

            //Sections - MotionPre
            footState = new FootStatePlus(FootState.Air, footVerTolerance, footHorTolerance);

            //Sections - MotionFist
            fistVelocity = new TwoFistVelocity(length);
            leftFistVelocity = fistVelocity.left;
            rightFistVelocity = fistVelocity.right;
            fistOffset = new TwoFistOffset();

            //Sections - Whole
            wholeVelocityBeforeJump =
                new WholeVelocityBeforeJump( Mathf.Max(wholeJumpHistoryTime, wholeJumpZeroCut, wholeJumpSimpleTime) );
            wholeVelocityWhileJumping = 
                new WholeVelocityWhileJumping(gravity, friction, wholeSpeedDownPartMax, wholeFistSpeedRatio,
                    historyTime: wholeJumpHistoryTime, 
                    historyZeroCutTime: wholeJumpZeroCut, 
                    historySimpleHistoryTime: wholeJumpSimpleTime, 
                    historyComplexHistoryTime: wholeJumpComplexTime,
                    jumpSpeedAddedWhenSlanting: speedAddedDiagonal);
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
            if (wholeJumpSimpleTime <= wholeJumpZeroCut
                && wholeJumpZeroCut <= wholeJumpHistoryTime)
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
}