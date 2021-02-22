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
    float fistSpeed;
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

    void FixedUpdate()
    {
        if (keyRAlt)
            FistFixedUpdate_AltPressed(rhFist, rhJoint1Pos, RightOrLeftHand.Right);
        else
            FistFixedUpdate_AltNotPressed(rhFist, rhJoint1Pos, RightOrLeftHand.Right);

        if (keyLAlt)
            FistFixedUpdate_AltPressed(lhFist, lhJoint1Pos, RightOrLeftHand.Left);
        else
            FistFixedUpdate_AltNotPressed(lhFist, lhJoint1Pos, RightOrLeftHand.Left);

        OtherFixedUpdate();
    }

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

    void RHFixedUpdate_AltNotPressed()
    {
        //1. update fist pos
        {
            Vector2 posRFist = new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y);
            int RRight = (keyRRight ? 1 : 0) - (keyRLeft ? 1 : 0);
            int RUp = (keyRUp ? 1 : 0) - (keyRDown ? 1 : 0);
            Vector2 direction = new Vector2(RRight, RUp);
            Vector2 velocity = direction.normalized * fistSpeed * Time.fixedDeltaTime;
            posRFist = posRFist + velocity;
            posRFist.x = Mathf.Clamp(posRFist.x, rhJoint1Pos.x + minXToOrigin, rhJoint1Pos.x + length);
            posRFist.y = Mathf.Clamp(posRFist.y, rhJoint1Pos.y - length, rhJoint1Pos.y + length);
            rhFist.transform.localPosition = posRFist;
        }
    }

    void RHFixedUpdate_AltPressed()
    {
        //1. update wholePerson pos
        Vector2 wholePersonPos = new Vector2(wholePerson.transform.localPosition.x, wholePerson.transform.localPosition.y);
        {
            // 计算wholePersonPos时，手离身体的距离依然不能超越范围，所以，先计算身体不动情况下的fistPos，再由此计算wholePersonPos
            Vector2 oldFistPos = new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y);
            int RRight = (keyRRight ? 1 : 0) - (keyRLeft ? 1 : 0);
            int RUp = (keyRUp ? 1 : 0) - (keyRDown ? 1 : 0);
            Vector2 direction = new Vector2(RRight, RUp);
            Vector2 velocity = direction.normalized * fistSpeed * Time.fixedDeltaTime;
            Vector2 newFistPos = oldFistPos + velocity;
            newFistPos.x = Mathf.Clamp(newFistPos.x, rhJoint1Pos.x + minXToOrigin, rhJoint1Pos.x + length);
            newFistPos.y = Mathf.Clamp(newFistPos.y, rhJoint1Pos.y - length, rhJoint1Pos.y + length);
            rhFist.transform.localPosition = newFistPos;

            Vector2 offset = newFistPos - oldFistPos;
            wholePersonPos = wholePersonPos - offset;
            wholePerson.transform.localPosition = wholePersonPos;
        }
    }

    void RHFixedUpdate_AfterFistUpdate()
    {
        //-- the triangle
        float a = (new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y) - rhJoint1Pos).magnitude;
        float b = length1;
        float c = length2;

        //2. posJoint2
        Vector2 posJoint2 = new Vector2();
        if (b + c > a)
        {
            float cosOrigin = (a * a + b * b - c * c) / (2 * a * b);
            float angleOrigin = Mathf.Acos(cosOrigin);

            float cosJoint2 = (b * b + c * c - a * a) / (2 * b * c);
            float angleJoint2 = Mathf.Acos(cosJoint2);

            Vector2 OriginToFist = new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y) - rhJoint1Pos;
            Vector2 OriginToJoint2_ = MyTool.Vec2Rotate(OriginToFist.normalized, -angleOrigin) * length1;

            posJoint2 = rhJoint1Pos + OriginToJoint2_;
            rhJoint2.transform.localPosition = posJoint2;

            myDebug.d_cosOrigin = cosOrigin;
            myDebug.d_angleOrigin = angleOrigin;
            myDebug.d_cosJoint2 = cosJoint2;
            myDebug.d_angleJoint2 = angleJoint2;
            myDebug.d_OriginToFist = OriginToFist;
            myDebug.d_OriginToJoint2 = OriginToJoint2_;
            myDebug.d_posJoint2 = posJoint2;
        }
        else
        {
            Vector2 OriginToFist = new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y) - rhJoint1Pos;
            Vector2 offset_ = Vector2.Lerp(new Vector2(), OriginToFist, length1 / length);
            posJoint2 = rhJoint1Pos + offset_;
            rhJoint2.transform.localPosition = posJoint2;

            myDebug.d_cosOrigin = 0;
            myDebug.d_angleOrigin = 0;
            myDebug.d_cosJoint2 = 0;
            myDebug.d_angleJoint2 = 0;
            myDebug.d_OriginToFist = new Vector2();
            myDebug.d_OriginToJoint2 = new Vector2();
            myDebug.d_posJoint2 = new Vector2();
        }


        //3. Line1's pos and orientation, Line2's, Fist's Orientation
        //--Line1
        Vector2 OriginToJoint2 = new Vector2(rhJoint2.transform.localPosition.x, rhJoint2.transform.localPosition.y) - rhJoint1Pos;
        Vector2 offsetLine1 = Vector2.Lerp(
            new Vector2(),
            OriginToJoint2,
            (rhJoint1.transform.localScale.x / 2 + rhLine1.transform.localScale.x / 2) / length1
        );
        rhLine1.transform.localPosition = rhJoint1Pos + offsetLine1;
        float angleLine1 = Vector2.SignedAngle(new Vector2(1, 0), OriginToJoint2);
        rhLine1.transform.localRotation = Quaternion.Euler(0, 0, angleLine1);
        //--Line2
        Vector2 FistToJoint2 = (
            new Vector2(rhJoint2.transform.localPosition.x, rhJoint2.transform.localPosition.y)
            - new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y)
        );
        Vector2 offsetLine2 = Vector2.Lerp(
            new Vector2(),
            FistToJoint2,
            (rhFist.transform.localScale.x / 2 + rhLine2.transform.localScale.x / 2) / length2
        );
        rhLine2.transform.localPosition = new Vector2(rhFist.transform.localPosition.x, rhFist.transform.localPosition.y) + offsetLine2;
        float angleLine2 = Vector2.SignedAngle(new Vector2(1, 0), -FistToJoint2);
        rhLine2.transform.localRotation = Quaternion.Euler(0, 0, angleLine2);
        //--Fist
        rhFist.transform.localRotation = Quaternion.Euler(0, 0, angleLine2);
    }

    void FistFixedUpdate_AltNotPressed(GameObject fist, Vector2 joint1Pos, RightOrLeftHand rightOrLeft)
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
        Vector2 velocity = direction.normalized * fistSpeed * Time.fixedDeltaTime;
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

    void FistFixedUpdate_AltPressed(GameObject fist, Vector2 joint1Pos, RightOrLeftHand rightOrLeft)
    {
        // 计算wholePersonPos时，手离身体的距离依然不能超越范围，所以，先计算身体不动情况下的fistPos，再由此计算wholePersonPos
        //1. 计算新旧fistPos，并设置fistPos
        Vector2 oldFistPos = new Vector2(fist.transform.localPosition.x, fist.transform.localPosition.y);
        Vector2 newFistPos = oldFistPos;

        int moveRight = (keyRRight ? 1 : 0) - (keyRLeft ? 1 : 0);
        int moveUp = (keyRUp ? 1 : 0) - (keyRDown ? 1 : 0);
        if (rightOrLeft == RightOrLeftHand.Left)
        {
            moveRight = (keyLRight ? 1 : 0) - (keyLLeft ? 1 : 0);
            moveUp = (keyLUp ? 1 : 0) - (keyLDown ? 1 : 0);
        }

        Vector2 direction = new Vector2(moveRight, moveUp);
        Vector2 velocity = direction.normalized * fistSpeed * Time.fixedDeltaTime;
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
        wholePersonPos = wholePersonPos - offset;
        wholePerson.transform.localPosition = wholePersonPos;
    }

    void OtherFixedUpdate()
    {
        OtherFixedUpdate_RightHand();
        OtherFixedUpdate_LeftHand();
    }

    void OtherFixedUpdate_LeftHand()
    {
        //左手需要通过镜像的方式计算
        Vector2 lhFistPosMirred = new Vector2(-lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
        Vector2 lhJoint1PosMirred = new Vector2(-lhJoint1.transform.localPosition.x, lhJoint1.transform.localPosition.y);

        Vector2 joint2PosMirred;
        Vector2 line1PosMirred;
        Quaternion line1RotationMirred;
        Vector2 line2PosMirred;
        Quaternion line2AndFistRotationMirred;

        OtherFixedUpdate_RightHand_InputParams(
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

    void OtherFixedUpdate_RightHand()
    {
        Vector2 joint2Pos;
        Vector2 line1Pos;
        Quaternion line1Rotation;
        Vector2 line2Pos;
        Quaternion line2AndFistRotation;
        OtherFixedUpdate_RightHand_InputParams(
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

    static void OtherFixedUpdate_RightHand_InputParams(
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

        fistSpeed = length;
        minXToOrigin = rhJoint1.transform.localScale.x / 2 + rhFist.transform.localScale.x / 2;
    }
}
