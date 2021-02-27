using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControl : MonoBehaviour
{
    //From Editor
    [SerializeField] GameObject rhJoint1;
    [SerializeField] GameObject rhJoint2;
    [SerializeField] GameObject rhLine1;
    [SerializeField] GameObject rhLine2;
    [SerializeField] GameObject rhFist;
    [SerializeField] GameObject lhJoint1;
    [SerializeField] GameObject lhJoint2;
    [SerializeField] GameObject lhLine1;
    [SerializeField] GameObject lhLine2;
    [SerializeField] GameObject lhFist;
    [SerializeField] GameObject wholePerson;

    enum RightOrLeftHand
    {
        Right = 1,
        Left = -1
    }

    //Key Status
    public bool keyRUp;
    public bool keyRDown;
    public bool keyRLeft;
    public bool keyRRight;
    public bool keyRAlt;

    public bool keyLUp;
    public bool keyLDown;
    public bool keyLLeft;
    public bool keyLRight;
    public bool keyLAlt;

    //const variable
    public float length1;
    public float length2;
    public float length;
    public Vector2 rhJoint1Pos;
    public Vector2 lhJoint1Pos;
    float fistMoveSpeed;
    float fistRotateSpeed_Degree;
    float minXToOrigin;

    //Debug
    public MyDebug myDebug;

    void Start()
    {
        ConstVariableInit();
    }

    void Update()
    {
        UpdateKeyStatus();
        UpdateFistColor();
    }

    #region UpdateSubFunc
    void UpdateKeyStatus()
    {
        keyRUp = Input.GetKey(KeyCode.O);
        keyRDown = Input.GetKey(KeyCode.L);
        keyRLeft = Input.GetKey(KeyCode.K);
        keyRRight = Input.GetKey(KeyCode.Semicolon);
        keyRAlt = Input.GetKey(KeyCode.RightAlt);

        keyLUp = Input.GetKey(KeyCode.W);
        keyLDown = Input.GetKey(KeyCode.S);
        keyLLeft = Input.GetKey(KeyCode.A);
        keyLRight = Input.GetKey(KeyCode.D);
        keyLAlt = Input.GetKey(KeyCode.LeftAlt);
    }

    void UpdateFistColor()
    {
        Color normal = new Color((float)0xF5 / 255, (float)0xF5 / 255, (float)0x42 / 255);
        Color pressed = new Color((float)0xE7 / 255, (float)0x75 / 255, 0f);

        if (keyRAlt)
            rhFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            rhFist.GetComponent<SpriteRenderer>().color = normal;
        if (keyLAlt)
            lhFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            lhFist.GetComponent<SpriteRenderer>().color = normal;
    }
    #endregion


    void FixedUpdate()
    {
        if (!keyRAlt)
            FixedUpdate_FistAndWhole_FistMoveWholeNot_RightHand();
        if (!keyLAlt)
            FixedUpdate_FistAndWhole_FistMoveWholeNot_LeftHand();

        if (keyRAlt && !keyLAlt)
            FixedUpdate_FistAndWhole_WholeMoveFistNot(rhFist, rhJoint1Pos, RightOrLeftHand.Right);
        if (!keyRAlt && keyLAlt)
            FixedUpdate_FistAndWhole_WholeMoveFistNot(lhFist, lhJoint1Pos, RightOrLeftHand.Left);

        if (keyRAlt && keyLAlt)
        {
            FixedUpdate_FistAndWhole_TwoHandsAltPressed();
        }

        FixedUpdate_Other();
    }
    void FixedUpdate_FistAndWhole_FistMoveWholeNot(GameObject fist, Vector2 joint1Pos, RightOrLeftHand rightOrLeft)
    {
        Vector2 fistPos = new Vector2(fist.transform.localPosition.x, fist.transform.localPosition.y);

        int moveRight = (keyRRight ? 1 : 0) - (keyRLeft ? 1 : 0);
        int moveUp = (keyRUp ? 1 : 0) - (keyRDown ? 1 : 0);
        if (rightOrLeft == RightOrLeftHand.Left)
        {
            moveRight = (keyLRight ? 1 : 0) - (keyLLeft ? 1 : 0);
            moveUp = (keyLUp ? 1 : 0) - (keyLDown ? 1 : 0);
        }

        Vector2 direction = new Vector2(moveRight, moveUp);
        Vector2 velocity = direction.normalized * fistMoveSpeed * Time.fixedDeltaTime;
        fistPos = fistPos + velocity;
        if (rightOrLeft == RightOrLeftHand.Right)
        {
            fistPos.x = Mathf.Clamp(fistPos.x, joint1Pos.x + minXToOrigin, joint1Pos.x + length);
        }
        else if (rightOrLeft == RightOrLeftHand.Left)
        {
            fistPos.x = Mathf.Clamp(fistPos.x, joint1Pos.x - length, joint1Pos.x - minXToOrigin);  
        }
        fistPos.y = Mathf.Clamp(fistPos.y, joint1Pos.y - length, joint1Pos.y + length);

        fist.transform.localPosition = fistPos;
    }

    void FixedUpdate_FistAndWhole_WholeMoveFistNot(GameObject fist, Vector2 joint1Pos, RightOrLeftHand rightOrLeft)
    {
        // 计算wholePersonPos时，手离身体的距离依然不能超越范围，所以，先计算身体不动情况下的fistPos，再由此计算wholePersonPos
        //1. 计算新旧fistPos，并设置fistPos
        Vector2 oldFistPos = new Vector2(fist.transform.localPosition.x, fist.transform.localPosition.y);
        Vector2 newFistPos = oldFistPos;

        int moveRight = -((keyRRight ? 1 : 0) - (keyRLeft ? 1 : 0));
        int moveUp = -((keyRUp ? 1 : 0) - (keyRDown ? 1 : 0));
        if (rightOrLeft == RightOrLeftHand.Left)
        {
            moveRight = -((keyLRight ? 1 : 0) - (keyLLeft ? 1 : 0));
            moveUp = -((keyLUp ? 1 : 0) - (keyLDown ? 1 : 0));
        }

        Vector2 direction = new Vector2(moveRight, moveUp);
        Vector2 velocity = direction.normalized * fistMoveSpeed * Time.fixedDeltaTime;
        newFistPos = newFistPos + velocity;
        if (rightOrLeft == RightOrLeftHand.Right)
        {
            newFistPos.x = Mathf.Clamp(newFistPos.x, joint1Pos.x + minXToOrigin, joint1Pos.x + length);
        }
        else if (rightOrLeft == RightOrLeftHand.Left)
        {
            newFistPos.x = Mathf.Clamp(newFistPos.x, joint1Pos.x - length, joint1Pos.x - minXToOrigin);
        }
        newFistPos.y = Mathf.Clamp(newFistPos.y, joint1Pos.y - length, joint1Pos.y + length);

        fist.transform.localPosition = newFistPos;

        //2. wholePersonPos
        Vector2 wholePersonPos = new Vector2(wholePerson.transform.localPosition.x, wholePerson.transform.localPosition.y);
        Vector2 offset = newFistPos - oldFistPos;
        offset = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation,wholePerson.transform.localScale) * (Vector3)offset;
        wholePersonPos = wholePersonPos - offset;
        wholePerson.transform.localPosition = wholePersonPos;
    }

    void FixedUpdate_FistAndWhole_FistMoveWholeNot_RightHand()
    {
        Vector2 fistPos = new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y);
        int moveRight = (keyRRight ? 1 : 0) - (keyRLeft ? 1 : 0);
        int moveUp = (keyRUp ? 1 : 0) - (keyRDown ? 1 : 0);
        Vector2 direction = new Vector2(moveRight, moveUp).normalized;
        Vector2 moveVecPossible = direction * fistMoveSpeed * Time.fixedDeltaTime;
        Vector2 fistPosAfterMove;
        float minX = rhJoint1Pos.x + minXToOrigin;
        float maxX = rhJoint1Pos.x + length;
        float minY = rhJoint1Pos.y - length;
        float maxY = rhJoint1Pos.y + length;

        FixedUpdate_FistAndWhole_FistMoveWholeNot_WithParameter(fistPos, moveVecPossible, minX, maxX, minY, maxY, out fistPosAfterMove);
        
        rhFist.transform.localPosition = fistPosAfterMove;
    }

    void FixedUpdate_FistAndWhole_FistMoveWholeNot_LeftHand()
    {
        Vector2 fistPos = new Vector2(lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
        int moveRight = (keyLRight ? 1 : 0) - (keyLLeft ? 1 : 0);
        int moveUp = (keyLUp ? 1 : 0) - (keyLDown ? 1 : 0);
        Vector2 direction = new Vector2(moveRight, moveUp).normalized;
        Vector2 moveVecPossible = direction * fistMoveSpeed * Time.fixedDeltaTime;
        Vector2 fistPosAfterMove;
        float minX = lhJoint1Pos.x - length;
        float maxX = lhJoint1Pos.x - minXToOrigin;
        float minY = lhJoint1Pos.y - length;
        float maxY = lhJoint1Pos.y + length;

        FixedUpdate_FistAndWhole_FistMoveWholeNot_WithParameter(fistPos, moveVecPossible, minX, maxX, minY, maxY, out fistPosAfterMove);

        lhFist.transform.localPosition = fistPosAfterMove;
    }

    static void FixedUpdate_FistAndWhole_FistMoveWholeNot_WithParameter(in Vector2 fistPos, in Vector2 moveVecPossible,in float minX,in float maxX,in float minY,in float maxY, out Vector2 fistPosAfterMove)
    {

        fistPosAfterMove = fistPos + moveVecPossible;
        fistPosAfterMove.x = Mathf.Clamp(fistPosAfterMove.x, minX, maxX);
        fistPosAfterMove.y = Mathf.Clamp(fistPosAfterMove.y, minY, maxY);
    }

    static void FixedUpdate_FistAndWhole_RotateRoundRightHand_InputParam(in float angle_Deg, in GameObject rhFist, in GameObject wholePerson, out Vector2 offset, out float angleWholePerson_Deg)
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

    void FixedUpdate_FistAndWhole_TwoHandsAltPressed()
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
        Vector2 rhDirection = new Vector2();
        {
            int moveRight = -((keyRRight ? 1 : 0) - (keyRLeft ? 1 : 0));
            int moveUp = -((keyRUp ? 1 : 0) - (keyRDown ? 1 : 0));
            rhDirection = new Vector2(moveRight, moveUp).normalized;
        }
        Vector2 lhDirection = new Vector2();
        {
            int moveRight = -((keyLRight ? 1 : 0) - (keyLLeft ? 1 : 0));
            int moveUp = -((keyLUp ? 1 : 0) - (keyLDown ? 1 : 0));
            lhDirection = new Vector2(moveRight, moveUp).normalized;
        }
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
            Vector2 velocity = rhDirection * (fistMoveSpeed*2) * Time.fixedDeltaTime;
            Vector2 rhFistPosOld = rhFist.transform.localPosition;
            Vector2 rhFistPosNew = rhFistPosOld + velocity;
            rhFistPosNew.x = Mathf.Clamp(rhFistPosNew.x, rhJoint1Pos.x + minXToOrigin, rhJoint1Pos.x + length);
            rhFistPosNew.y = Mathf.Clamp(rhFistPosNew.y, rhJoint1Pos.y - length, rhJoint1Pos.y + length);
            Vector2 lhFistPosOld = lhFist.transform.localPosition;
            Vector2 lhFistPosNew = lhFistPosOld + velocity;
            lhFistPosNew.x = Mathf.Clamp(lhFistPosNew.x, lhJoint1Pos.x - length, lhJoint1Pos.x - minXToOrigin);
            lhFistPosNew.y = Mathf.Clamp(lhFistPosNew.y, lhJoint1Pos.y - length, lhJoint1Pos.y + length);
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
                wholePerson.transform.localPosition += -(Vector3)offset;
            }
        }
        else if (Mathf.Approximately(rhDirection.y, 1) || Mathf.Approximately(lhDirection.y, 1))
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
                    FixedUpdate_FistAndWhole_WholeMoveFistNot(lhFist, lhJoint1Pos, RightOrLeftHand.Left);
                }

                float angle_Deg = fistRotateSpeed_Degree * Time.fixedDeltaTime;
                Vector2 offset;
                float angleWholePerson;
                FixedUpdate_FistAndWhole_RotateRoundRightHand_InputParam(angle_Deg, rhFist, wholePerson, out offset, out angleWholePerson);
                
                wholePerson.transform.localPosition += (Vector3)offset;
                wholePerson.transform.Rotate(new Vector3(0, 0, angleWholePerson), Space.Self);
            }
            else if (Mathf.Approximately(lhDirection.y, 1))
            {
                /*
                if (!Mathf.Approximately(rhDirection.magnitude, 0))
                {
                    FixedUpdate_FistAndWhole_WholeMoveFistNot(rhFist, rhJoint1Pos, RightOrLeftHand.Right);
                }
                */
                //沿左手旋转，需要根据右手做镜像
                float angle_Deg = fistRotateSpeed_Degree * Time.fixedDeltaTime;
                Vector2 mirredOffset;
                float mirredAngleWholePerson;
                Vector2 rhOldPos = rhFist.transform.localPosition;
                Vector2 lhMirredPos = new Vector2(-lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
                rhFist.transform.localPosition = lhMirredPos;
                FixedUpdate_FistAndWhole_RotateRoundRightHand_InputParam(angle_Deg, rhFist, wholePerson, out mirredOffset, out mirredAngleWholePerson);
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

    void FixedUpdate_Other()
    {
        FixedUpdate_Other_RightHand();
        FixedUpdate_Other_LeftHand();
    }

    void FixedUpdate_Other_LeftHand()
    {
        //左手需要通过镜像的方式计算
        Vector2 lhFistPosMirred = new Vector2(-lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
        Vector2 lhJoint1PosMirred = new Vector2(-lhJoint1.transform.localPosition.x, lhJoint1.transform.localPosition.y);

        Vector2 joint2PosMirred;
        Vector2 line1PosMirred;
        Quaternion line1RotationMirred;
        Vector2 line2PosMirred;
        Quaternion line2AndFistRotationMirred;

        FixedUpdate_Other_RightHand_InputParams(
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

    void FixedUpdate_Other_RightHand()
    {
        Vector2 joint2Pos;
        Vector2 line1Pos;
        Quaternion line1Rotation;
        Vector2 line2Pos;
        Quaternion line2AndFistRotation;
        FixedUpdate_Other_RightHand_InputParams(
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

    static void FixedUpdate_Other_RightHand_InputParams(
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

    
    void ConstVariableInit()
    {
        length1 = rhJoint1.transform.localScale.x / 2 + rhLine1.transform.localScale.x + rhJoint2.transform.localScale.x / 2;
        length2 = rhJoint2.transform.localScale.x / 2 + rhLine2.transform.localScale.x + rhFist.transform.localScale.x / 2;
        length = length1 + length2;
        rhJoint1Pos = rhJoint1.transform.localPosition;
        lhJoint1Pos = lhJoint1.transform.localPosition;

        fistMoveSpeed = length;
        fistRotateSpeed_Degree = 360f / 15f;
        minXToOrigin = rhJoint1.transform.localScale.x / 2 + rhFist.transform.localScale.x / 2;
    }
}
