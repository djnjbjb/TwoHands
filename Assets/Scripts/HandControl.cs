#define MYTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class HandControl : MonoBehaviour
{   
    //Person object
    [SerializeField] GameObject wholePerson;
    GameObject rhJoint1;
    GameObject rhJoint2;
    GameObject rhLine1;
    GameObject rhLine2;
    GameObject rhFist;
    GameObject lhJoint1;
    GameObject lhJoint2;
    GameObject lhLine1;
    GameObject lhLine2;
    GameObject lhFist;
    GameObject bottomLeftPoint;
    GameObject bottomRightPoint;


    //Fist Virable
    Vector2 rhJoint1PosConst;
    Vector2 lhJoint1PosConst;
    GameObject rhGrabedStuff = null;
    GameObject lhGrabedStuff = null;
    public HandState rhState;
    public HandState lhState;
    HandState rhStatePre = HandState.Free;
    HandState lhStatePre = HandState.Free;

    Vector2 rhFistVelocity;
    Vector2 lhFistVelocity;
    float fistSpeedStartingUp;
    float fistSpeedAcceleration;
    float fistSpeedDeceleration;
    float fistSpeedMax;
    float fistMoveSpeed;

    //Preson Variable
    float wholeSpeed = 0f; //一会儿删除
    Vector2 wholeVelocity = new Vector2();
    Vector2 wholeGrabEnvOffset = new Vector2();


    //frequently using variable
    float length1;
    float length2;
    public float length;
    float fistRotateSpeed_Degree;
    float minToOrigin;
    //gravity
    float gravitySpeedMax = 12;
    float gravityAcceleration = 12;
    float surfaceTolerance = 0.01f;
    //Debug
    public MyDebug myDebug;

    /*    
           O
          OO
           O
           O
           O
           O
          OOO
     */

    void Start()
    {
        ConstInit();
    }

    void ConstInit()
    {
        //Person object
        rhJoint1 = wholePerson.transform.Find("RHJoint1").gameObject;
        rhJoint2 = wholePerson.transform.Find("RHJoint2").gameObject;
        rhLine1 = wholePerson.transform.Find("RHLine1").gameObject;
        rhLine2 = wholePerson.transform.Find("RHLine2").gameObject;
        rhFist = wholePerson.transform.Find("RHFist").gameObject;
        lhJoint1 = wholePerson.transform.Find("LHJoint1").gameObject;
        lhJoint2 = wholePerson.transform.Find("LHJoint2").gameObject;
        lhLine1 = wholePerson.transform.Find("LHLine1").gameObject;
        lhLine2 = wholePerson.transform.Find("LHLine2").gameObject;
        lhFist = wholePerson.transform.Find("LHFist").gameObject;
        bottomLeftPoint = wholePerson.transform.Find("BottomLeftPoint").gameObject;
        bottomRightPoint = wholePerson.transform.Find("BottomRightPoint").gameObject;

        //frequently using variable
        length1 = rhJoint1.transform.localScale.x / 2 + rhLine1.transform.localScale.x + rhJoint2.transform.localScale.x / 2;
        length2 = rhJoint2.transform.localScale.x / 2 + rhLine2.transform.localScale.x + rhFist.transform.localScale.x / 2;
        length = length1 + length2;
        fistRotateSpeed_Degree = 360f / 15f;
        minToOrigin = rhJoint1.transform.localScale.x / 2 + rhFist.transform.localScale.x / 2;

        //Fist Virable
        rhJoint1PosConst = rhJoint1.transform.localPosition;
        lhJoint1PosConst = lhJoint1.transform.localPosition;
        fistMoveSpeed = length * 2;
        rhFistVelocity = new Vector2(0, 0);
        lhFistVelocity = new Vector2(0, 0);
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
    }
    /*
          OOO
         O   O
             O
           OO
         OO
         O
         OOOOO
    */

    void Update()
    {
        #if MYTEST
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            print($"rhFist, position:{rhFist.transform.localPosition}, rotation:{rhFist.transform.localRotation.eulerAngles}");
            print($"rhJoint2, position:{rhJoint2.transform.localPosition}, rotation:{rhJoint2.transform.localRotation.eulerAngles}");
            print($"rhLine1, position:{rhLine1.transform.localPosition}, rotation:{rhLine1.transform.localRotation.eulerAngles}");
            print($"rhLine2, position:{rhLine2.transform.localPosition}, rotation:{rhLine2.transform.localRotation.eulerAngles}");
            print($"lhFist, position:{lhFist.transform.localPosition}, rotation:{lhFist.transform.localRotation.eulerAngles}");
            print($"lhJoint2, position:{lhJoint2.transform.localPosition}, rotation:{lhJoint2.transform.localRotation.eulerAngles}");
            print($"lhLine1, position:{lhLine1.transform.localPosition}, rotation:{lhLine1.transform.localRotation.eulerAngles}");
            print($"lhLine2, position:{lhLine2.transform.localPosition}, rotation:{lhLine2.transform.localRotation.eulerAngles}");
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Logout.PrintBuffer();
        }
#endif

        HKey.Refresh();
        UpdateFistRepresent();
    }

    void UpdateFistRepresent()
    {
        //color
        Color normal = GameObject.Find("FistNormalColor").GetComponent<SpriteRenderer>().color;
        Color pressed = GameObject.Find("FistPressedColor").GetComponent<SpriteRenderer>().color;

        if (rhState.GrabPressed())
            rhFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            rhFist.GetComponent<SpriteRenderer>().color = normal;
        if (lhState.GrabPressed())
            lhFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            lhFist.GetComponent<SpriteRenderer>().color = normal;

        //grab sign
        var rGrabSign = rhFist.transform.Find("GrabSign").gameObject;
        var lGrabSign = lhFist.transform.Find("GrabSign").gameObject;
        rGrabSign.SetActive(rhState.GrabSomething());
        lGrabSign.SetActive(lhState.GrabSomething());
    }

    /*   
         OOOO
             O
             O
          OOO
             O
             O
         OOOO
    */

    void FixedUpdate()
    {
        FU_HandState();
        FU_HandStuff();
        FU_HandStatePre();
        
        //Hand
        if (rhState != HandState.GrabEnv)
            FU_Fist_NoGrabEnv_Right(HKey.rMvDir);
        if (lhState != HandState.GrabEnv)
            FU_Fist_NoGrabEnv_Left(HKey.lMvDir);
        if (rhState == HandState.GrabEnv && lhState != HandState.GrabEnv)
            FU_Fist_GrabEnv_Right();
        if (rhState != HandState.GrabEnv && lhState == HandState.GrabEnv)
            FU_Fist_GrabEnv_Left();

        if (rhState == HandState.GrabEnv && lhState == HandState.GrabEnv)
        {
            FU_Fist_GrabEnv_2Fist();
        }

        //Whole
        if (rhState != HandState.GrabEnv && lhState != HandState.GrabEnv)
        {
            FU_Whole();
        }

        //Other
        //Other其实像是纯表现 
        FU_Other();


    }

    void _____________1______________I()
    {

    }

    void FU_HandState()
    {
        LayerMask LMStuff = LayerMask.GetMask("Stuff");
        LayerMask LMEnv = LayerMask.GetMask("EnvRock", "EnvGround");
        LayerMask LMGround = LayerMask.GetMask("EnvGround");

        HandState GetHandState(GameObject fist, bool altPressed, HandState statePre)
        {
            if (!altPressed)
                return HandState.Free;

            //GrabStuff的优先级较高。比Env之类的高。
            if (statePre == HandState.GrabStuff && altPressed)
                return HandState.GrabStuff;
            var colliderStuff = Physics2D.OverlapCircle(fist.transform.position,
                fist.transform.lossyScale.x / 2, LMStuff);
            if (colliderStuff)
                return HandState.GrabStuff;
            
            var colliderEnv = Physics2D.OverlapCircle(fist.transform.position,
               fist.transform.lossyScale.x / 2, LMEnv);
            if (colliderEnv)
                return HandState.GrabEnv;

            return HandState.GrabNothing;
        }

        rhState = GetHandState(rhFist, HKey.rAlt, rhStatePre);
        lhState = GetHandState(lhFist, HKey.lAlt, lhStatePre);
    }

    void FU_HandStuff()
    {
        /*
            把stuff变成hand的子节点，这样可以省事
            对stuff的collider进行处理 - 添加/删除。暂时先这么做。
         */
        void UpdateHandStuff(ref GameObject handGrabedStuff, HandState state, HandState statePre, GameObject fist, Vector2 mvDir)
        {
            LayerMask LMStuff = LayerMask.GetMask("Stuff");
            if (statePre != HandState.GrabStuff && state == HandState.GrabStuff)
            {
                var colliderStuff = Physics2D.OverlapCircle(fist.transform.position,
                    fist.transform.lossyScale.x / 2, LMStuff);
                //这个collider，按理说一定存在
                if (!colliderStuff)
                    throw new Exception("collider == null");

                GameObject stuff = colliderStuff.gameObject;
                Destroy(colliderStuff);

                stuff.transform.SetParent(fist.transform);
                handGrabedStuff = stuff;
            }
            if (statePre == HandState.GrabStuff && state != HandState.GrabStuff)
            {
                GameObject stuff = handGrabedStuff;
                handGrabedStuff = null;
                stuff.transform.parent = null;
                stuff.AddComponent<PolygonCollider2D>();

                stuff.GetComponent<Stuff>().speed = 12f;
                stuff.GetComponent<Stuff>().direction = mvDir;
            }
        }

        UpdateHandStuff(ref rhGrabedStuff, rhState, rhStatePre, rhFist, HKey.rMvDir);
        UpdateHandStuff(ref lhGrabedStuff, lhState, lhStatePre, lhFist, HKey.lMvDir);

    }

    void FU_HandStatePre()
    {
        rhStatePre = rhState;
        lhStatePre = lhState;
    }

    Vector2 RightFistMove(Vector2 fistPos, Vector2 moveVecPossible)
    {
        Vector2 positionResult = fistPos;
        positionResult += moveVecPossible;

        float minY = rhJoint1PosConst.y - length;
        float maxY = rhJoint1PosConst.y + length;
        float middleY = rhJoint1PosConst.y + minToOrigin;
        float minX = rhJoint1PosConst.x - length;
        float middleX = rhJoint1PosConst.x + minToOrigin;
        float maxX = rhJoint1PosConst.x + length;
        if (fistPos.x < middleX)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, middleY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        else if (fistPos.y < middleY)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, middleX, maxX);
        }
        else
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        return positionResult;
    }

    Vector2 LeftFistMove(Vector2 fistPos, Vector2 moveVecPossible)
    {
        Vector2 positionResult = fistPos;
        positionResult += moveVecPossible;

        float minY = lhJoint1PosConst.y - length;
        float middleY = lhJoint1PosConst.y + minToOrigin;
        float maxY = lhJoint1PosConst.y + length;
        float minX = lhJoint1PosConst.x - length;
        float middleX = lhJoint1PosConst.x - minToOrigin;
        float maxX = lhJoint1PosConst.x + length;
        positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
        if (fistPos.x > middleX)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, middleY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        else if (fistPos.y < middleY)
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, middleX);
        }
        else
        {
            positionResult.y = Mathf.Clamp(positionResult.y, minY, maxY);
            positionResult.x = Mathf.Clamp(positionResult.x, minX, maxX);
        }
        return positionResult;
    }

    float SpeedDecelerate(float speedPre, float deceleration = float.NaN)
    {
        if (deceleration == float.NaN)
            deceleration = fistSpeedDeceleration;
        float speed = speedPre - fistSpeedDeceleration * Time.fixedDeltaTime;
        if (speed < fistSpeedStartingUp)
        {
            speed = 0f;
        }

        return speed;
    }
    float SpeedAccelerate(float speedPre, float acceleration = float.NaN)
    {
        if (acceleration == float.NaN)
            acceleration = fistSpeedAcceleration;
        float speed = speedPre + fistSpeedAcceleration * Time.fixedDeltaTime;
        speed = Mathf.Clamp(speed, fistSpeedStartingUp, fistSpeedMax);
        return speed;
    }

    void ______________2_____________II()
    {

    }

    /// <summary>
    /// return offest
    /// </summary>
    Vector2 FU_Fist_NoGrabEnv_Right(Vector2 mvDir)
    {
        /*
            手的移动，先这样
            手的移动，希望既可以短距离精确移动，又可以快速移动
            精确 + 快速。精确 vs 快速，本身就存在矛盾。

            先不搞了，先这样，再做后面的，根据之后的需求再调整。
            
            现在考虑到的
            1. 减速部分。减速需要减得更快，可能lerp之类。
            2. 加速。不一定匀速加速。根据更具体的需求来搞。
            3. 其实有很多可调整的地方。
                比如，如果仅仅是摆造型需要，那可以搞一些挡位置的东西之类。
                手的速度、加速度，也可以根据手的位置等参数而变化。
        */
        Vector2 fistPos = (Vector2)rhFist.transform.localPosition;
        Vector2 fistPosAfterMove;

        /*
            先得到 moveVecPossible
            mvDir = 0，减速。mvDir!=0，加速。
               先不管speed和mvDir方向的关系，总是加速
         */
        /*
            velocity的方向：
            如果mvDir不为0，就和mvDir一样
            如果mvDir为0，就和上一帧方向一样

            不用考虑speed为0时mvDir的情况，moveVecPossible = mvDir * speed ... 自动就解决了。
        */
        float speedPre = rhFistVelocity.magnitude;
        float speed;
        if (mvDir.magnitude != 0)
            speed = SpeedAccelerate(speedPre);
        else
            speed = SpeedDecelerate(speedPre);
        if (mvDir.magnitude == 0)
            mvDir = rhFistVelocity.normalized;
        Vector2 moveVecPossible = mvDir * speed * Time.fixedDeltaTime;
        /*
            现在根据 moveVecPossible 尝试移动
            把尝试移动的结果分为2类，没有动 vs 动了
            
            如果没有动，那么无论mvDir、speed等如何，speed都应该相对于speedPre减速。
            如果动了，那就是正常情况，speed去影响rhFistVelocity。
         
         */
        fistPosAfterMove = RightFistMove(fistPos, moveVecPossible);
        rhFist.transform.localPosition = fistPosAfterMove;
        Vector2 offset = fistPosAfterMove - fistPos;
        if (MyTool.FloatEqual0p001(offset.magnitude, 0f))
        {
            speed = SpeedDecelerate(speedPre);
        }
        else
        {
            // do nothing
        }
        rhFistVelocity = speed * mvDir;
        return offset;
    }

    /// <summary>
    /// return offest
    /// </summary>
    Vector2 FU_Fist_NoGrabEnv_Left(Vector2 mvDir)
    {
        Vector2 fistPos = lhFist.transform.localPosition;
        Vector2 fistPosAfterMove;

        float speedPre = lhFistVelocity.magnitude;
        float speed;
        if (mvDir.magnitude != 0)
            speed = SpeedAccelerate(speedPre);
        else
            speed = SpeedDecelerate(speedPre);
        if (mvDir.magnitude == 0)
            mvDir = lhFistVelocity.normalized;
        Vector2 moveVecPossible = mvDir * speed * Time.fixedDeltaTime;
        /*
            现在根据 moveVecPossible 尝试移动
            把尝试移动的结果分为2类，没有动 vs 动了
            
            如果没有动，那么无论mvDir、speed等如何，speed都应该相对于speedPre减速。
            如果动了，那就是正常情况，speed去影响rhFistVelocity。
         
         */
        fistPosAfterMove = LeftFistMove(fistPos, moveVecPossible);
        lhFist.transform.localPosition = fistPosAfterMove;
        Vector2 offset = fistPosAfterMove - fistPos;
        if (MyTool.FloatEqual0p001(offset.magnitude, 0f))
        {
            speed = SpeedDecelerate(speedPre);
        }
        else
        {
            // do nothing
        }
        lhFistVelocity = speed * mvDir;
        return offset;
    }

    void FU_Fist_GrabEnv_Right()
    {
        Matrix4x4 wholeMatrix = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale);
        wholeGrabEnvOffset = -FU_Fist_NoGrabEnv_Right(-HKey.rMvDir);
        wholeGrabEnvOffset = wholeMatrix * wholeGrabEnvOffset;
        wholeVelocity = wholeMatrix * (-rhFistVelocity);
    }

    void FU_Fist_GrabEnv_Left()
    {
        Matrix4x4 wholeMatrix = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale);
        wholeGrabEnvOffset = -FU_Fist_NoGrabEnv_Left(-HKey.rMvDir);
        wholeGrabEnvOffset = wholeMatrix * wholeGrabEnvOffset;
        wholeVelocity = wholeMatrix * (-lhFistVelocity);
    }
    void FU_Fist_GrabEnv_2Fist()
    {
        /*
            如果两只手同向，则同向移动
            如果一直有有方向，一只手没有方向，则按照有方向的移动
            其他情况不移动
        */
        Vector2 rhDirection = -HKey.rMvDir;
        Vector2 lhDirection = -HKey.lMvDir;
        Matrix4x4 wholeMatrix = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale);
        if (rhDirection == lhDirection)
        {
            /*
                需要确定的有：
                    手的offset、手的速度
                    身体的offset、身体的速度
             
                -----------------------------------------
                
                先考量offset，再考量speed
                首先让二者速度相同，之后offset的计算与以前相同。
                
                然后再计量速度。
                
                -----------------------------------------
            */
            float rhFistSpeedPre = rhFistVelocity.magnitude;
            float lhFistSpeedPre = lhFistVelocity.magnitude;
            float fistSpeedPre = Mathf.Max(rhFistSpeedPre, lhFistSpeedPre);
            float fistSpeed = SpeedAccelerate(fistSpeedPre, fistSpeedAcceleration * 2);

            //根据速度确定offset
            float dis2x = fistSpeed * Time.fixedDeltaTime;
            Vector2 mvVector = dis2x * rhDirection;
            Vector2 rhFistPosOld = rhFist.transform.localPosition;
            Vector2 rhFistPosNew = RightFistMove(rhFistPosOld, mvVector);
            Vector2 lhFistPosOld = lhFist.transform.localPosition;
            Vector2 lhFistPosNew = LeftFistMove(lhFistPosOld, mvVector);
            float rhDis = (rhFistPosNew - rhFistPosOld).magnitude;
            float lhDis = (lhFistPosNew - lhFistPosOld).magnitude;
            float smallerDis = 0;
            float biggerDis = 0;
            GameObject smallerFist = rhFist;
            GameObject biggerFist = rhFist;
            if (rhDis > lhDis)
            {
                smallerDis = lhDis;
                smallerFist = lhFist;
                biggerDis = rhDis;
                biggerFist = rhFist;
            }
            else
            {
                smallerDis = rhDis;
                smallerFist = rhFist;
                biggerDis = lhDis;
                biggerFist = lhFist;
            }
            if (MyTool.FloatEqual0p001(biggerDis, 0))
            {
                //如果双手移动都是0
                //位置都不动，手和身体都减速
                fistSpeed = SpeedDecelerate(fistSpeedPre);
                rhFistVelocity = fistSpeed * rhDirection;
                lhFistVelocity = fistSpeed * lhDirection;

                wholeGrabEnvOffset = new Vector2();
                wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
            }
            else if (MyTool.FloatEqual0p001(smallerDis, 0) || !MyTool.FloatEqual0p001(biggerDis, 0))
            {
                //如果一只手的移动是0
                //身体和移动的那只手，等效于单手移动
                //另一只手减速
                if (biggerFist == rhFist)
                {
                    FU_Fist_GrabEnv_Right();
                    lhFistVelocity = SpeedDecelerate(lhFistVelocity.magnitude) * lhDirection;
                }
                else if (biggerFist == lhFist)
                {
                    FU_Fist_GrabEnv_Left();
                    rhFistVelocity = SpeedDecelerate(rhFistVelocity.magnitude) * rhDirection;
                }
            }
            else
            {
                /*  
                    双手移动都不是0
                        其实分2种情况，一种是都可以移动到dis2x，一种是不行，但在计算时没有区别

                    双手都移动到对应的dis。身体的offset是biggerDis。
                    双手速度都增加到speed。身体速度则是-speed。
                */
                Vector2 smallerMove = smallerDis * rhDirection;
                Vector2 biggerMove = biggerDis * rhDirection;

                smallerFist.transform.localPosition += (Vector3)smallerMove;
                biggerFist.transform.localPosition += (Vector3)biggerMove;
                rhFistVelocity = fistSpeed * rhDirection;
                lhFistVelocity = fistSpeed * lhDirection;

                wholeGrabEnvOffset = wholeMatrix * (-biggerMove);
                wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
            }
        }
        else if (rhDirection.magnitude != 0 && lhDirection.magnitude == 0)
        {
            FU_Fist_GrabEnv_Right();
            lhFistVelocity = SpeedDecelerate(lhFistVelocity.magnitude) * lhDirection;
        }
        else if (rhDirection.magnitude == 0 && lhDirection.magnitude != 0)
        {
            FU_Fist_GrabEnv_Left();
            rhFistVelocity = SpeedDecelerate(rhFistVelocity.magnitude) * rhDirection;
        }
        else
        {
            //do nothing
        }
    }

    void ___________________________IIII()
    {

    }

    enum FootState
    {
        Air,
        Surface,
        EnvGround,
        EnvRock
    }

    void FU_Whole()
    {
        /*
            当手有GrabEnv时，用offset计算
            没有时，用speed计算
        */

        //在FU_Main_Gravity()，需要处理   gravitySpeed 和 whole的位置
        //没有考虑从左右碰撞collider的问题。因为现在只有竖直运动，就先这样。

        //1.判断foot状态
        FootState footState;
        {
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hit = Physics2D.Linecast(pointBL, pointBR);

            Vector2 pointBLUp = (Vector2)bottomLeftPoint.transform.position + new Vector2(0, surfaceTolerance);
            Vector2 pointBRUp = (Vector2)bottomRightPoint.transform.position + new Vector2(0, surfaceTolerance);
            RaycastHit2D hitUp = Physics2D.Linecast(pointBLUp, pointBRUp);

            Vector2 pointBLDown = (Vector2)bottomLeftPoint.transform.position - new Vector2(0, surfaceTolerance);
            Vector2 pointBRDown = (Vector2)bottomRightPoint.transform.position - new Vector2(0, surfaceTolerance);
            RaycastHit2D hitDown = Physics2D.Linecast(pointBLDown, pointBRDown);

            if (!hitUp.collider && !hitDown.collider)
                footState = FootState.Air;
            else if (!hitUp.collider && hitDown.collider)
                footState = FootState.Surface;
            else
            {
                footState = FootState.EnvRock;

                int layerMaskGround = LayerMask.GetMask("EnvGround");
                RaycastHit2D hitDownGround = Physics2D.Linecast(pointBLDown, pointBRDown, layerMaskGround);
                RaycastHit2D hitUpGround = Physics2D.Linecast(pointBLUp, pointBRUp, layerMaskGround);
                if (hitDownGround.collider)
                    footState = FootState.EnvGround;
            }
        }

        if (footState == FootState.Surface || footState == FootState.EnvGround)
        {
            wholeSpeed = 0f;
        }
        else if (footState == FootState.Air)
        {
            float gravitySpeedNew = Mathf.Clamp(wholeSpeed + gravityAcceleration * Time.fixedDeltaTime, 0, gravitySpeedMax);
            float dis = (wholeSpeed + gravitySpeedNew) / 2 * Time.fixedDeltaTime;

            //只用2个点做Raycast，不能保证，但先这样
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), dis);
            RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), dis);

            if (!hitBL.collider && !hitBR.collider)
            {
                wholeSpeed = gravitySpeedNew;
                wholePerson.transform.position += new Vector3(0, -dis, 0);
            }
            else
            {
                if (hitBL.collider)
                {
                    dis = hitBL.distance;
                }
                if (hitBR.collider)
                {
                    if (hitBR.distance < dis)
                    {
                        dis = hitBR.distance;
                    }
                }
                wholeSpeed = 0;
                wholePerson.transform.position += new Vector3(0, -dis, 0);
            }
        }
        else if (footState == FootState.EnvRock)
        {
            float gravitySpeedNew = Mathf.Clamp(wholeSpeed + gravityAcceleration * Time.fixedDeltaTime, 0, gravitySpeedMax);
            float dis = (wholeSpeed + gravitySpeedNew) / 2 * Time.fixedDeltaTime;

            //只用2个点做Raycast，不能保证，但先这样
            //当前是EnvRock，如果经过一个Air，再出现EnvRock。这里的算法就会有问题。但一帧移动的很少，目前应该不会出现。先不管它。
            int layerMaskGround = LayerMask.GetMask("EnvGround");
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), dis, layerMaskGround);
            RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), dis, layerMaskGround);

            if (!hitBL.collider && !hitBR.collider)
            {
                wholeSpeed = gravitySpeedNew;
                wholePerson.transform.position += new Vector3(0, -dis, 0);
            }
            else
            {
                if (hitBL.collider)
                {
                    dis = hitBL.distance;
                }
                if (hitBR.collider)
                {
                    if (hitBR.distance < dis)
                    {
                        dis = hitBR.distance;
                    }
                }
                wholeSpeed = 0;
                wholePerson.transform.position += new Vector3(0, -dis, 0);
            }
        }
    }

    void ___________________________L()
    {

    }

    void FU_Other()
    {
        FU_Other_Right();
        FU_Other_Left();
    }

    void FU_Other_Left()
    {
        //左手需要通过镜像的方式计算
        Vector2 lhFistPosMirred = new Vector2(-lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
        Vector2 lhJoint1PosMirred = new Vector2(-lhJoint1.transform.localPosition.x, lhJoint1.transform.localPosition.y);

        Vector2 joint2PosMirred;
        Vector2 line1PosMirred;
        Quaternion line1RotationMirred;
        Vector2 line2PosMirred;
        Quaternion line2AndFistRotationMirred;

        FU_Other_Right_Params(
            lhFistPosMirred, lhJoint1PosMirred,
            lhJoint1.transform.localScale.x, lhJoint2.transform.localScale.x,
            lhLine1.transform.localScale.x, lhLine2.transform.localScale.x,
            lhFist.transform.localScale.x,
            length1, length2, length,
            out joint2PosMirred, out line1PosMirred, out line1RotationMirred, out line2PosMirred, out line2AndFistRotationMirred
            );
        lhJoint2.transform.localPosition = new Vector2(-joint2PosMirred.x, joint2PosMirred.y);
        lhLine1.transform.localPosition = new Vector2(-line1PosMirred.x, line1PosMirred.y);
        Vector3 axis;
        float angle;
        line1RotationMirred.ToAngleAxis(out angle, out axis);
        lhLine1.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
        lhLine2.transform.localPosition = new Vector2(-line2PosMirred.x, line2PosMirred.y);
        line2AndFistRotationMirred.ToAngleAxis(out angle, out axis);
        lhLine2.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
        lhFist.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
    }

    void FU_Other_Right()
    {
        Vector2 joint2Pos;
        Vector2 line1Pos;
        Quaternion line1Rotation;
        Vector2 line2Pos;
        Quaternion line2AndFistRotation;
        FU_Other_Right_Params(
            rhFist.transform.localPosition, rhJoint1.transform.localPosition,
            rhJoint1.transform.localScale.x, rhJoint2.transform.localScale.x,
            rhLine1.transform.localScale.x, rhLine2.transform.localScale.x,
            rhFist.transform.localScale.x,
            length1, length2, length,
            out joint2Pos, out line1Pos, out line1Rotation, out line2Pos, out line2AndFistRotation
            );
        rhJoint2.transform.localPosition = joint2Pos;
        rhLine1.transform.localPosition = line1Pos;
        rhLine1.transform.localRotation = line1Rotation;
        rhLine2.transform.localPosition = line2Pos;
        rhLine2.transform.localRotation = line2AndFistRotation;
        rhFist.transform.localRotation = line2AndFistRotation;
    }

    static void FU_Other_Right_Params(
        in Vector2 fistPos, in Vector2 joint1Pos,
        in float joint1ScaleX, in float joint2ScaleX, 
        in float line1ScaleX, in float line2ScaleX,
        in float fistScaleX, 
        in float length1, in float length2,in float length,
        out Vector2 joint2Pos,
        out Vector2 line1Pos, out Quaternion line1Rotation,
        out Vector2 line2Pos, out Quaternion line2AndFistRotation
    )
    {
        //the triangle
        float a = (new Vector2(fistPos.x, fistPos.y) - joint1Pos).magnitude;
        float b = length1;
        float c = length2;

        //1. posJoint2
        joint2Pos = new Vector2();
        if (b + c > a)
        {
            float cosOrigin = (a * a + b * b - c * c) / (2 * a * b);
            float angleOrigin = Mathf.Acos(cosOrigin);
            Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;
            Vector2 joint1ToJoint2 = MyTool.Vec2Rotate(joint1ToFist.normalized, -angleOrigin) * length1;
            joint2Pos = joint1Pos + joint1ToJoint2;
        }
        else
        {
            Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;
            Vector2 offset = Vector2.Lerp(new Vector2(), joint1ToFist, length1 / length);
            joint2Pos = joint1Pos + offset;
        }

        //2. Line1's pos and orientation, Line2's, Fist's Orientation
        line1Pos = new Vector2();
        line1Rotation = new Quaternion();
        {
            Vector2 joint1ToJoint2 = new Vector2(joint2Pos.x, joint2Pos.y) - joint1Pos;
            Vector2 offsetLine1 = Vector2.Lerp(
                new Vector2(),
                joint1ToJoint2,
                (joint1ScaleX / 2 + line1ScaleX / 2) / length1
            );
            line1Pos = joint1Pos + offsetLine1;
            float angleLine1 = Vector2.SignedAngle(new Vector2(1, 0), joint1ToJoint2);
            line1Rotation = Quaternion.Euler(0, 0, angleLine1);
        }
        line2Pos = new Vector2();
        line2AndFistRotation = new Quaternion();
        {
            Vector2 joint2ToFist = (
                new Vector2(fistPos.x, fistPos.y)
                - new Vector2(joint2Pos.x, joint2Pos.y)
            );
            Vector2 offsetLine2 = Vector2.Lerp(
                new Vector2(),
                joint2ToFist,
                (joint2ScaleX / 2 + line2ScaleX / 2) / length2
            );
            line2Pos = joint2Pos + offsetLine2;
            float angleLine2 = Vector2.SignedAngle(new Vector2(1, 0), joint2ToFist);
            line2AndFistRotation = Quaternion.Euler(0, 0, angleLine2);
        }
    }
}
