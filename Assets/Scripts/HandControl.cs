using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public partial class HandControl : MonoBehaviour
{   
    //GameObject
    [SerializeField] GameObject whole;
    GameObject rightFist;
    GameObject leftFist;
    GameObject bottomLeftPoint;
    GameObject bottomRightPoint;
    HandRepresent handRepresent;
    //常用const
    int ifGrabMoveReverse = 1;
    float length;
    //常用变量，每帧更新的
    Matrix4x4 wholeMatrix;

    //Hand State
    HandState rightFistState = HandState.Free;
    GameObject rightGrabedStuff = null;
    HandState leftFistState = HandState.Free;
    GameObject leftGrabedStuff = null;

    //Fist Move Variable
    FistVelocity fistVelocity;

    //Whole Move Variable
    Vector2 wholeVelocity = new Vector2();
    Vector2 wholeGrabEnvOffset = new Vector2();
    //const
    float wholeSpeedMax;
    float gravityAcceleration = 12f;
    float surfaceTolerance = 0.01f;

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

        //Hand State
        //不用初始化

        //fistVelocicy
        fistVelocity = new FistVelocity(length);


        //Whole Move Variable
        wholeSpeedMax = 5*length;

        Yurowm.DebugTools.DebugPanel.Log("Length", "常量", length);
    }
}
