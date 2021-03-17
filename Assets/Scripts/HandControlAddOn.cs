using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HKey
{
    static public bool rUp;
    static public bool rDown;
    static public bool rLeft;
    static public bool rRight;
    static public bool rAlt;

    static public bool lUp;
    static public bool lDown;
    static public bool lLeft;
    static public bool lRight;
    static public bool lAlt;

    static public int rMvRight;
    static public int rMvUp;
    static public Vector2 rMvDir;
    static public int lMvRight;
    static public int lMvUp;
    static public Vector2 lMvDir;

    static public void Refresh()
    {
        rUp = Input.GetKey(KeyCode.O);
        rDown = Input.GetKey(KeyCode.L);
        rLeft = Input.GetKey(KeyCode.K);
        rRight = Input.GetKey(KeyCode.Semicolon);
        rAlt = Input.GetKey(KeyCode.RightAlt);

        lUp = Input.GetKey(KeyCode.W);
        lDown = Input.GetKey(KeyCode.S);
        lLeft = Input.GetKey(KeyCode.A);
        lRight = Input.GetKey(KeyCode.D);
        lAlt = Input.GetKey(KeyCode.LeftAlt);

        rMvRight = (rRight ? 1 : 0) - (rLeft ? 1 : 0);
        rMvUp = (rUp ? 1 : 0) - (rDown ? 1 : 0);
        rMvDir = new Vector2(rMvRight, rMvUp).normalized;
        lMvRight = (lRight ? 1 : 0) - (lLeft ? 1 : 0);
        lMvUp = (lUp ? 1 : 0) - (lDown ? 1 : 0);
        lMvDir = new Vector2(lMvRight, lMvUp).normalized;
    }
}






public class HandOther
{
    public GameObject RightJoint1;
    public GameObject RightJoint2;
    public GameObject RightLine1;
    public GameObject RightLine2;
    public GameObject RightFist;
    public GameObject LeftJoint1;
    public GameObject LeftJoint2;
    public GameObject LeftLine1;
    public GameObject LeftLine2;
    public GameObject LeftFist;
    float length1;
    float length2;
    float length;

    public HandOther(GameObject whole)
    {
        RightJoint1 = whole.transform.Find("RHJoint1").gameObject;
        RightJoint2 = whole.transform.Find("RHJoint2").gameObject;
        RightLine1 = whole.transform.Find("RHLine1").gameObject;
        RightLine2 = whole.transform.Find("RHLine2").gameObject;
        RightFist = whole.transform.Find("RHFist").gameObject;
        LeftJoint1 = whole.transform.Find("LHJoint1").gameObject;
        LeftJoint2 = whole.transform.Find("LHJoint2").gameObject;
        LeftLine1 = whole.transform.Find("LHLine1").gameObject;
        LeftLine2 = whole.transform.Find("LHLine2").gameObject;
        LeftFist = whole.transform.Find("LHFist").gameObject;

        length1 = RightJoint1.transform.localScale.x / 2 + RightLine1.transform.localScale.x + RightJoint2.transform.localScale.x / 2;
        length2 = RightJoint2.transform.localScale.x / 2 + RightLine2.transform.localScale.x + RightFist.transform.localScale.x / 2;
        length = length1 + length2;
    }

    public void Refresh()
    {
        RefreshRight();
        RefreshLeft();
    }
    void RefreshRight()
    {
        Vector2 joint2PosPre = RightJoint2.transform.localPosition;
        Vector2 joint2Pos;
        Vector2 line1Pos;
        Quaternion line1Rotation;
        Vector2 line2Pos;
        Quaternion line2Rotation;
        Quaternion fistRotation;
        CalulateShapeRight(
            RightFist.transform.localPosition, RightJoint1.transform.localPosition, joint2PosPre,
            RightJoint1.transform.localScale.x, RightJoint2.transform.localScale.x,
            RightLine1.transform.localScale.x, RightLine2.transform.localScale.x,
            length1, length2, length,
            out joint2Pos, 
            out line1Pos, out line1Rotation, 
            out line2Pos, out line2Rotation,
            out fistRotation
            );

        RightJoint2.transform.localPosition = joint2Pos;
        RightLine1.transform.localPosition = line1Pos;
        RightLine1.transform.localRotation = line1Rotation;
        RightLine2.transform.localPosition = line2Pos;
        RightLine2.transform.localRotation = line2Rotation;
        RightFist.transform.localRotation = fistRotation;
    }

    void RefreshLeft()
    {
        //左手需要通过镜像的方式计算
        Vector2 lhFistPosMirred = new Vector2(-LeftFist.transform.localPosition.x, LeftFist.transform.localPosition.y);
        Vector2 lhJoint1PosMirred = new Vector2(-LeftJoint1.transform.localPosition.x, LeftJoint1.transform.localPosition.y);
        Vector2 lhJoint2PosPreMirred = new Vector2(-LeftJoint2.transform.localPosition.x, LeftJoint2.transform.localPosition.y);

        Vector2 joint2PosMirred;
        Vector2 line1PosMirred;
        Quaternion line1RotationMirred;
        Vector2 line2PosMirred;
        Quaternion line2RotationMirred;
        Quaternion fistRotationMirred;

        CalulateShapeRight(
            lhFistPosMirred, lhJoint1PosMirred, lhJoint2PosPreMirred,
            LeftJoint1.transform.localScale.x, LeftJoint2.transform.localScale.x,
            LeftLine1.transform.localScale.x, LeftLine2.transform.localScale.x,
            length1, length2, length,
            out joint2PosMirred, 
            out line1PosMirred, out line1RotationMirred, 
            out line2PosMirred, out line2RotationMirred,
            out fistRotationMirred
            );
        LeftJoint2.transform.localPosition = new Vector2(-joint2PosMirred.x, joint2PosMirred.y);
        LeftLine1.transform.localPosition = new Vector2(-line1PosMirred.x, line1PosMirred.y);
        Vector3 axis;
        float angle;
        line1RotationMirred.ToAngleAxis(out angle, out axis);
        LeftLine1.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
        LeftLine2.transform.localPosition = new Vector2(-line2PosMirred.x, line2PosMirred.y);
        line2RotationMirred.ToAngleAxis(out angle, out axis);
        LeftLine2.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
        fistRotationMirred.ToAngleAxis(out angle, out axis);
        LeftFist.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
    }

    static void CalulateShapeRight(
        in Vector2 fistPos, in Vector2 joint1Pos, in Vector2 joint2PosPre,
        in float joint1ScaleX, in float joint2ScaleX,
        in float line1ScaleX, in float line2ScaleX,
        in float length1, in float length2, in float length,
        out Vector2 joint2Pos,
        out Vector2 line1Pos, out Quaternion line1Rotation,
        out Vector2 line2Pos, out Quaternion line2Rotation,
        out Quaternion fistRotation
    )
    {
        joint2Pos = GetShapeRightJoint2Pos(
            fistPos, joint1Pos, joint2PosPre,
            length1, length2, length);

        GetShapeRightOther(
            fistPos, joint1Pos, joint2Pos,
            joint1ScaleX, joint2ScaleX,
            line1ScaleX, line2ScaleX,
            length1, length2, length,
            out line1Pos, out line1Rotation,
            out line2Pos, out line2Rotation,
            out fistRotation);
    }

    static Vector2 GetShapeRightJoint2Pos (
        in Vector2 fistPos, in Vector2 joint1Pos,
        in Vector2 joint2PosPre,
        in float length1, in float length2, in float length )
    {
        //the triangle
        float a = (new Vector2(fistPos.x, fistPos.y) - joint1Pos).magnitude;
        float b = length1;
        float c = length2;

        //posJoint2
        if (b + c > a)
        {
            //是三角形
            /*
                Joint2所在位置，可能在 根→手 连线的左侧，也可能在右侧。左侧优先。
                
                分别计算两种情况下 Joint2Pos 和 JointPosPre的距离。
                如果某个小，就用某一个。
                如果差不多，就用左侧。

                值得注意的是，这里差不多的计算。和其他的差不多用通用的阈值（MyTool.FloatEqual0p001）可能不合适，但先这样试一下。

            */
            float cosJoint1 = (a * a + b * b - c * c) / (2 * a * b);
            float angleJoint1 = Mathf.Acos(cosJoint1);
            Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;

            Vector2 joint2Pos_pointOnLeft;
            Vector2 joint1ToJoint2_pointOnLeft = MyTool.Vec2Rotate(joint1ToFist.normalized, -angleJoint1) * length1;
            joint2Pos_pointOnLeft = joint1Pos + joint1ToJoint2_pointOnLeft;
            Vector2 joint2Pos_pointOnRight;
            Vector2 joint1ToJoint2_pointOnRight = MyTool.Vec2Rotate(joint1ToFist.normalized, angleJoint1) * length1;
            joint2Pos_pointOnRight = joint1Pos + joint1ToJoint2_pointOnRight;

            FastFileLog.LogManager.Log(GameObject.Find("log1"), $"Pre, ({joint2PosPre.x}, {joint2PosPre.y})");
            FastFileLog.LogManager.Log(GameObject.Find("log1"), $"Current, Left, ({joint2Pos_pointOnLeft.x}, {joint2Pos_pointOnLeft.y}), Right, ({joint2Pos_pointOnRight.x}, {joint2Pos_pointOnRight.y})");
            float distanceLeft = (joint2Pos_pointOnLeft - joint2PosPre).magnitude;
            float distanceRight = (joint2Pos_pointOnRight - joint2PosPre).magnitude;
            float equalThreshold = 1f;
            if (Mathf.Abs(distanceLeft - distanceRight) <= equalThreshold)
            {
                FastFileLog.LogManager.Log(GameObject.Find("log1"), "Choose:Left");
                return joint2Pos_pointOnLeft;
            }
            else if (distanceLeft < distanceRight)
            {
                FastFileLog.LogManager.Log(GameObject.Find("log1"), "Choose:Left");
                return joint2Pos_pointOnLeft;
            }
            else
            {
                FastFileLog.LogManager.Log(GameObject.Find("log1"), "Choose:Right");
                return joint2Pos_pointOnRight;
            }
        }
        else
        {
            //非三角形
            //非三角形1.手和根，几乎重合
            if (MyTool.FloatEqual0p001(a, 0))
            {
                float radius = Mathf.Max(length1, length2);
                /*
                    当手和根几乎重合时，Joint2应该在哪比较好呢？
                    
                    目前想到最合适的方法是，根据 joint2PosPre 和 手的移动反向，确定一条线，取圆上里线最近的一点
                    
                    这样比较麻烦，因此，取圆上离joint2PosPre最近的一个点，也不错。（即从圆心向joint2PosPre作射线，射线与圆的交点）
                    
                    因为当手和根几乎重合时，在每步移动范围很小的情况下，joint2PosPre也是在圆上的，所以，取joint2PosPre即可。
                */
                return joint2PosPre;
            }
            //非三角形2.手和根离得的很远
            else
            {
                Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;
                Vector2 offset = Vector2.Lerp(new Vector2(), joint1ToFist, length1 / length);
                return joint1Pos + offset;
            }

        }
    }

    static void GetShapeRightOther (
        in Vector2 fistPos, in Vector2 joint1Pos, in Vector2 joint2Pos,
        in float joint1ScaleX, in float joint2ScaleX,
        in float line1ScaleX, in float line2ScaleX,
        in float length1, in float length2, in float length,
        out Vector2 line1Pos, out Quaternion line1Rotation,
        out Vector2 line2Pos, out Quaternion line2Rotation,
        out Quaternion fistRotation)
    {
        line1Pos = new Vector2();
        line1Rotation = new Quaternion();
        {
            Vector2 joint1ToJoint2 = new Vector2(joint2Pos.x, joint2Pos.y) - joint1Pos;
            Vector2 offsetLine1 = Vector2.Lerp(
                new Vector2(),
                joint1ToJoint2,
                (joint1ScaleX / 2 + line1ScaleX / 2) / length1 );
            line1Pos = joint1Pos + offsetLine1;
            float angleLine1 = Vector2.SignedAngle(new Vector2(1, 0), joint1ToJoint2);
            line1Rotation = Quaternion.Euler(0, 0, angleLine1);
        }
        line2Pos = new Vector2();
        line2Rotation = new Quaternion();
        fistRotation = new Quaternion();
        {
            Vector2 joint2ToFist = (
                new Vector2(fistPos.x, fistPos.y)
                - new Vector2(joint2Pos.x, joint2Pos.y) );
            Vector2 offsetLine2 = Vector2.Lerp(
                new Vector2(),
                joint2ToFist,
                (joint2ScaleX / 2 + line2ScaleX / 2) / length2 );
            line2Pos = joint2Pos + offsetLine2;
            float angleLine2 = Vector2.SignedAngle(new Vector2(1, 0), joint2ToFist);
            line2Rotation = Quaternion.Euler(0, 0, angleLine2);
            fistRotation = line2Rotation;
        }
    }
}