using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public partial class HandControl : MonoBehaviour
{
    //Setting
    float surfaceTolerance = 0.01f;


    //GameObject
    [SerializeField] GameObject whole;
    public GameObject leftFist { get; private set; }
    public GameObject rightFist { get; private set; }
    GameObject bottomLeftPoint;
    GameObject bottomRightPoint;
    HandRepresent handRepresent;
    //常用const
    int ifGrabMoveReverse = 1;
    float length;

    //Pre
    FistStatePlus leftFistState = FistState.Free;
    FistStatePlus rightFistState = FistState.Free;
    GameObject leftGrabedStuff = null;
    GameObject rightGrabedStuff = null;
    FootStatePlus footState;

    //Fist Move Variable
    TwoFistVelocity fistVelocity;

    //Whole Move Variable
    WholeVelocityBeforeJump wholeVelocityBeforeJump;
    WholeVelocityWhileJumping wholeVelocityWhileJumping;
    //const
    float wholeSpeedDownPartMax = 48f;
    float gravity = 48f;

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

        //fistVelocicy
        fistVelocity = new TwoFistVelocity(length);


        //Whole Move Variable
        wholeSpeedMax = 5*length;

        Yurowm.DebugTools.DebugPanel.Log("Length", "常量", length);
    }
}
