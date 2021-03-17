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
    HandOther handOther;
    //常用const
    float length;
    //常用变量，每帧更新的
    Matrix4x4 wholeMatrix;

    //Hand State
    HandState rightFistState = HandState.Free;
    HandState leftFistState = HandState.Free;
    GameObject rightGrabedStuff = null;
    GameObject leftGrabedStuff = null;

    //Fist Move Variable
    Vector2 rightFistVelocity = new Vector2();
    Vector2 leftFistVelocity = new Vector2();
    //const
    float fistSpeedStartingUp;
    float fistSpeedAcceleration;
    float fistSpeedDeceleration;
    float fistSpeedMax;

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
        handOther = new HandOther(whole);
        //常用const
        float length1 = handOther.RightJoint1.transform.localScale.x / 2 + handOther.RightLine1.transform.localScale.x + handOther.RightJoint2.transform.localScale.x / 2;
        float length2 = handOther.RightJoint2.transform.localScale.x / 2 + handOther.RightLine2.transform.localScale.x + rightFist.transform.localScale.x / 2;
        length = length1 + length2;

        //Hand State
        //不用初始化

        //Fist Move Variable
        /*
            初始速度大概为length
            从上到下，2*lengh，用时0.8s
            由此得出
                fistSpeedStartingUp = length;
                fistSpeedAcceleration = 3.75f * length;
                fistSpeedMax = 4f * length;
            由于可以双手，所以我希望 fistSpeedMax 再乘以2
        */
        fistSpeedStartingUp = length;
        fistSpeedAcceleration = 3.75f * length;
        fistSpeedMax = (4f * length) * 2;
        fistSpeedDeceleration = 8f * length;

        //Whole Move Variable
        wholeSpeedMax = fistSpeedMax;
    }
}
