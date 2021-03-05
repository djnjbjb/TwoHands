using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum HandState
{
    Free = 0,
    GrabStuff = 1,
    GrabEnv = 2,
    GrabNothing = 3
}
public static class HandStateExendsion
{
    public static bool FreeToMove(this HandState state)
    {
        return state == HandState.Free || state == HandState.GrabNothing || state == HandState.GrabStuff;
    }

    public static bool GrabPressed(this HandState state)
    {
        return state == HandState.GrabNothing || state == HandState.GrabEnv || state == HandState.GrabStuff;
    }
    public static bool GrabSomething(this HandState state)
    {
        return state == HandState.GrabEnv || state == HandState.GrabStuff;
    }
}
public class HandControl : MonoBehaviour
{
    enum RightOrLeftHand
    {
        Right = 1,
        Left = -1
    }
    
    //person part
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
    //frequently using variable
    float length1;
    float length2;
    float length;
    Vector2 rhJoint1Pos;
    Vector2 lhJoint1Pos;
    float fistMoveSpeed;
    float fistRotateSpeed_Degree;
    float minToOrigin;
    //gravity
    float gravitySpeed = 0;
    float gravitySpeedMax = 12;
    float gravityAcceleration = 12;
    float surfaceTolerance = 0.01f;
    //grab stuff
    GameObject rhGrabedStuff = null;
    GameObject lhGrabedStuff = null;
    public HandState rhState;
    public HandState lhState;
    HandState rhStatePre = HandState.Free;
    HandState lhStatePre = HandState.Free;

    //Debug
    public MyDebug myDebug;

    void ConstInit()
    {
        //person part
        rhJoint1 = wholePerson.transform.Find("RJoint1").gameObject;
        rhJoint2 = wholePerson.transform.Find("RJoint2").gameObject;
        rhLine1 = wholePerson.transform.Find("RLine1").gameObject;
        rhLine2 = wholePerson.transform.Find("RLine2").gameObject;
        rhFist = wholePerson.transform.Find("RFist").gameObject;
        lhJoint1 = wholePerson.transform.Find("LJoint1").gameObject;
        lhJoint2 = wholePerson.transform.Find("LJoint2").gameObject;
        lhLine1 = wholePerson.transform.Find("LLine1").gameObject;
        lhLine2 = wholePerson.transform.Find("LLine2").gameObject;
        lhFist = wholePerson.transform.Find("LFist").gameObject;
        bottomLeftPoint = wholePerson.transform.Find("BottomLeftPoint").gameObject;
        bottomRightPoint = wholePerson.transform.Find("BottomRightPoint").gameObject;
        //frequently using variable
        length1 = rhJoint1.transform.localScale.x / 2 + rhLine1.transform.localScale.x + rhJoint2.transform.localScale.x / 2;
        length2 = rhJoint2.transform.localScale.x / 2 + rhLine2.transform.localScale.x + rhFist.transform.localScale.x / 2;
        length = length1 + length2;
        rhJoint1Pos = rhJoint1.transform.localPosition;
        lhJoint1Pos = lhJoint1.transform.localPosition;
        fistMoveSpeed = length;
        fistRotateSpeed_Degree = 360f / 15f;
        minToOrigin = rhJoint1.transform.localScale.x / 2 + rhFist.transform.localScale.x / 2;
    }

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
        HKey.Refresh();
        UpdateFistRepresent();
    }

    void UpdateFistRepresent()
    {
        //color
        Color normal = new Color((float)0xF5 / 255, (float)0xF5 / 255, (float)0x42 / 255);
        Color pressed = new Color((float)0xE7 / 255, (float)0x75 / 255, 0f);

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
        
        //Main
        if (rhState != HandState.GrabEnv)
            FU_Main_0Fist_Right(HKey.rMvDir);
        if (lhState != HandState.GrabEnv)
            FU_Main_0Fist_Left(HKey.lMvDir);

        if (rhState == HandState.GrabEnv && lhState != HandState.GrabEnv)
            FU_Main_0Whole_Right();
        if (rhState != HandState.GrabEnv && lhState == HandState.GrabEnv)
            FU_Main_0Whole_Left();

        if (rhState == HandState.GrabEnv && lhState == HandState.GrabEnv)
        {
            FU_Main_0TwoHandsAltPressed();
        }

        //Other
        FU_Other();

        //Gravity
        if (rhState != HandState.GrabEnv && lhState != HandState.GrabEnv)
        {
            FU_Main_Gravity();
        }


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

    void ___________________________III()
    {

    }

    /// <summary>
    /// return offest
    /// </summary>
    Vector2 FU_Main_0Fist_Right(in Vector2 mvDir)
    {
        Vector2 fistPos = new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y);
        Vector2 moveVecPossible = mvDir * fistMoveSpeed * Time.fixedDeltaTime;
        Vector2 fistPosAfterMove;
        fistPosAfterMove = RightFistMove(fistPos, moveVecPossible);
       
        rhFist.transform.localPosition = fistPosAfterMove;
        return fistPosAfterMove - fistPos;
    }

    /// <summary>
    /// return offest
    /// </summary>
    Vector2 FU_Main_0Fist_Left(in Vector2 mvDir)
    {
        Vector2 fistPos = new Vector2(lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
        Vector2 moveVecPossible = mvDir * fistMoveSpeed * Time.fixedDeltaTime;
        Vector2 fistPosAfterMove;
        fistPosAfterMove = LeftFistMove(fistPos, moveVecPossible);

        lhFist.transform.localPosition = fistPosAfterMove;
        return fistPosAfterMove - fistPos;
    }

    Vector2 RightFistMove(Vector2 fistPos, Vector2 moveVecPossible)
    {
        Vector2 positionResult = fistPos;
        positionResult += moveVecPossible;

        float minY = rhJoint1Pos.y - length;
        float maxY = rhJoint1Pos.y + length;
        float middleY = rhJoint1Pos.y + minToOrigin;
        float minX = rhJoint1Pos.x - length;
        float middleX = rhJoint1Pos.x + minToOrigin;
        float maxX = rhJoint1Pos.x + length;
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

        float minY = lhJoint1Pos.y - length;
        float middleY = lhJoint1Pos.y + minToOrigin;
        float maxY = lhJoint1Pos.y + length;
        float minX = lhJoint1Pos.x - length;
        float middleX = lhJoint1Pos.x - minToOrigin;
        float maxX = lhJoint1Pos.x + length;
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

    void ___________________________I()
    {

    }

    void FU_Main_0Whole_Right()
    {
        // 计算wholePersonPos时，手离身体的距离依然不能超越范围，所以，先计算身体不动情况下的fistPos，再由此计算wholePersonPos
        //1. 计算新旧fistPos，并设置fistPos
        Vector2 offset = FU_Main_0Fist_Right(-HKey.rMvDir);

        Vector2 wholePersonPos = new Vector2(wholePerson.transform.localPosition.x, wholePerson.transform.localPosition.y);
        offset = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale) * (Vector3)offset;
        wholePersonPos = wholePersonPos - offset;
        wholePerson.transform.localPosition = wholePersonPos;
    }

    void FU_Main_0Whole_Left()
    {
        // 计算wholePersonPos时，手离身体的距离依然不能超越范围，所以，先计算身体不动情况下的fistPos，再由此计算wholePersonPos
        //1. 计算新旧fistPos，并设置fistPos
        Vector2 offset = FU_Main_0Fist_Left(-HKey.lMvDir);

        Vector2 wholePersonPos = new Vector2(wholePerson.transform.localPosition.x, wholePerson.transform.localPosition.y);
        offset = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale) * (Vector3)offset;
        wholePersonPos = wholePersonPos - offset;
        wholePerson.transform.localPosition = wholePersonPos;
    }

    void ___________________________II()
    {

    }

    void FU_Main_0TwoHandsAltPressed()
    {
        /*
        用【下】表示绕着那只手旋转
            身体旋转，像是2指手握着方向盘，然后用力，方向盘发生旋转，人跟着旋转
        
        1.如果两只手都按着同一个方向，则向那个方向移动
            双倍速度
        2.如果一只手按着下，另一只手按别的，则旋转并配合另一只手移动
            2.1如果是下 + 无，则只是旋转
            2.2如果是下 + 其他，则旋转 + 移动
        3.如果两只手按着不同方向，且没有一个是下，则不响应
        */
        Vector2 rhDirection = -HKey.rMvDir;
        Vector2 lhDirection = -HKey.lMvDir;
        if (rhDirection == lhDirection)
        {
            /*
            同时移动时，需要考量两只手的移动距离。
                想象一只手已经移动到尽头，另一只手没到的情况。
            
            如果两只手都能以2倍速移动，则以2倍速移动
            如果一只手不行，则
                两只手可共同移动的长度，两只手以双倍速移动
                剩下的部分，那只手以但倍速移动

            设2倍速距离为dis2。
            首先，求出两只手在方向上可移动的距离。l1，r1
            （由于求l1,r1较为复杂，可以先尝试移动dis2，然后Clamp。这样得到的l1、r1，为 l1 = min(dis2,l1),r1=min(dis2,r1)，也能用于后面的计算，不影响）
            取得l1,r1中较小min1，另一个是max1。
            与dis2比较。
            如果大于等于，则
                手移动2倍速距离。
                身体移动2倍速距离。
            如果小于，则
                较小的那个手，移动min1
                另一个手，移动 min(   max1, min1+(dis2-min1)/2  )
                身体，移动 min(   max1, min1+(dis2-min1)/2  )
            */

            float dis2x = (fistMoveSpeed * 2) * Time.fixedDeltaTime;
            Vector2 mvVector = rhDirection * (fistMoveSpeed*2) * Time.fixedDeltaTime;
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

            if (Mathf.Approximately(smallerDis, dis2x) || smallerDis >= dis2x)
            {
                rhFist.transform.localPosition = rhFistPosNew;
                lhFist.transform.localPosition = lhFistPosNew;

                Vector2 offset = rhFistPosNew - rhFistPosOld;
                offset = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale) * (Vector3)offset;
                Vector2 wholePersonPosNew = (Vector2)wholePerson.transform.localPosition - offset;
                wholePerson.transform.localPosition = wholePersonPosNew;
            }
            else
            {
                Vector2 smallerMove = smallerDis * rhDirection;
                biggerDis = Mathf.Min(biggerDis, smallerDis + (dis2x - smallerDis) / 2);
                Vector2 biggerMove = biggerDis * rhDirection;

                smallerFist.transform.localPosition += (Vector3)smallerMove;
                biggerFist.transform.localPosition += (Vector3)biggerMove;
                
                Vector3 offset = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale) * (Vector3)biggerMove;
                wholePerson.transform.localPosition += -offset;
            }
        }
        //旋转的代码先不执行
        //else if (Mathf.Approximately(rhDirection.y, 1) || Mathf.Approximately(lhDirection.y, 1))
        else if (false)
        {
            if (Mathf.Approximately(rhDirection.y, 1))
            {
                /*
                    如果只是旋转，则旋转。
                    如果旋转和移动结合，其实是个积分。
                        如果想算得好一点，等于(先旋转一半)→(再移动)→(再旋转)。
                        这里就凑活算了。让它等于：先移动，再旋转。
                    综合而言，两种情况都需要旋转。不同是，需要移动时，先移动一下。

                    所以，代码写成
                    1.如果需要移动，先移动。
                    2.然后，旋转。
                */
                if (!Mathf.Approximately(lhDirection.magnitude, 0))
                {
                    FU_Main_0Whole_Left();
                }

                float angle_Deg = fistRotateSpeed_Degree * Time.fixedDeltaTime;
                Vector2 offset;
                float angleWholePerson;
                FU_Main_1RotateRoundRight_Params(angle_Deg, rhFist, wholePerson, out offset, out angleWholePerson);
                
                wholePerson.transform.localPosition += (Vector3)offset;
                wholePerson.transform.Rotate(new Vector3(0, 0, angleWholePerson), Space.Self);
            }
            else if (Mathf.Approximately(lhDirection.y, 1))
            {
                
                if (!Mathf.Approximately(rhDirection.magnitude, 0))
                {
                    FU_Main_0Whole_Right();
                }
                
                //沿左手旋转，需要根据右手做镜像
                float angle_Deg = fistRotateSpeed_Degree * Time.fixedDeltaTime;
                Vector2 mirredOffset;
                float mirredAngleWholePerson;
                Vector2 rhOldPos = rhFist.transform.localPosition;
                Vector2 lhMirredPos = new Vector2(-lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
                rhFist.transform.localPosition = lhMirredPos;
                FU_Main_1RotateRoundRight_Params(angle_Deg, rhFist, wholePerson, out mirredOffset, out mirredAngleWholePerson);
                rhFist.transform.localPosition = rhOldPos;

                //向量的mirror，要根据wholePerson的中轴来做
                Vector2 wholeAxis = (wholePerson.transform.localRotation * new Vector3(0, 1, 0)).normalized;
                Vector2 offset = -Vector2.Reflect(mirredOffset, wholeAxis);
                wholePerson.transform.localPosition += (Vector3)offset;
                wholePerson.transform.Rotate(new Vector3(0, 0, -mirredAngleWholePerson), Space.Self);
            }
        }
        else
        {
            //do nothing
        }

    }

    static void FU_Main_1RotateRoundRight_Params(in float angle_Deg, in GameObject rhFist, in GameObject wholePerson, out Vector2 offset, out float angleWholePerson_Deg)
    {
        //旋转时，身体和手的相对位置不变，只是身体绕fist旋转
        //whole的父亲有一个坐标系，需要计算的是在这个坐标系中的移动和旋转变化
        Matrix4x4 fistLocalMatrix = Matrix4x4.TRS(
            rhFist.transform.localPosition,
            rhFist.transform.localRotation,
            rhFist.transform.localScale
        );
        Matrix4x4 wholeLocalMatrix = Matrix4x4.TRS(
            wholePerson.transform.localPosition,
            wholePerson.transform.localRotation,
            wholePerson.transform.localScale
        );
        Matrix4x4 fistMatrixInWholeParentCoor = wholeLocalMatrix * fistLocalMatrix;
        Vector2 fistPosInWholeParentCoor = fistMatrixInWholeParentCoor.ExtractPosition();
        Vector2 fistPos = fistPosInWholeParentCoor;
        Vector2 wholePos = wholePerson.transform.localPosition;
        Vector2 fistToWhole = wholePos - fistPos;
        Vector2 fistToWholeAfterRotate = MyTool.Vec2Rotate(fistToWhole, -angle_Deg * Mathf.Deg2Rad);

        offset = fistToWholeAfterRotate - fistToWhole;
        angleWholePerson_Deg = -angle_Deg;
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

    void FU_Main_Gravity()
    {
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
            gravitySpeed = 0f;
        }
        else if (footState == FootState.Air)
        {
            float gravitySpeedNew = Mathf.Clamp(gravitySpeed + gravityAcceleration * Time.fixedDeltaTime, 0, gravitySpeedMax);
            float dis = (gravitySpeed + gravitySpeedNew) / 2 * Time.fixedDeltaTime;

            //只用2个点做Raycast，不能保证，但先这样
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), dis);
            RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), dis);

            if (!hitBL.collider && !hitBR.collider)
            {
                gravitySpeed = gravitySpeedNew;
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
                gravitySpeed = 0;
                wholePerson.transform.position += new Vector3(0, -dis, 0);
            }
        }
        else if (footState == FootState.EnvRock)
        {
            float gravitySpeedNew = Mathf.Clamp(gravitySpeed + gravityAcceleration * Time.fixedDeltaTime, 0, gravitySpeedMax);
            float dis = (gravitySpeed + gravitySpeedNew) / 2 * Time.fixedDeltaTime;

            //只用2个点做Raycast，不能保证，但先这样
            //当前是EnvRock，如果经过一个Air，再出现EnvRock。这里的算法就会有问题。但一帧移动的很少，目前应该不会出现。先不管它。
            int layerMaskGround = LayerMask.GetMask("EnvGround");
            Vector2 pointBL = bottomLeftPoint.transform.position;
            Vector2 pointBR = bottomRightPoint.transform.position;
            RaycastHit2D hitBL = Physics2D.Raycast(pointBL, new Vector2(0, -1), dis, layerMaskGround);
            RaycastHit2D hitBR = Physics2D.Raycast(pointBR, new Vector2(0, -1), dis, layerMaskGround);

            if (!hitBL.collider && !hitBR.collider)
            {
                gravitySpeed = gravitySpeedNew;
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
                gravitySpeed = 0;
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
