#define TEST_RIGHT_LINE
#undef TEST_RIGHT_LINE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRepresent
{
    public GameObject rightJoint1;
    public GameObject rightJoint2;
    public GameObject rightLine1;
    public GameObject rightLine2;
    public GameObject rightFist;
    public GameObject leftJoint1;
    public GameObject leftJoint2;
    public GameObject leftLine1;
    public GameObject leftLine2;
    public GameObject leftFist;

#if TEST_RIGHT_LINE
    public GameObject rightJoint2p;
    public GameObject rightLine1p;
    public GameObject rightLine2p;
#endif

    float length1;
    float length2;
    float length;
    Joint2State rightJoint2State;
    Joint2State leftJoint2State;
    Joint2State rightJoint2StatePre;
    Joint2State leftJoint2StatePre;
    Vector2 rightFistPosPre;
    Vector2 leftFistPosPre;

    public HandRepresent(GameObject whole)
    {
        rightJoint1 = whole.transform.Find("RHJoint1").gameObject;
        rightJoint2 = whole.transform.Find("RHJoint2").gameObject;
        rightLine1 = whole.transform.Find("RHLine1").gameObject;
        rightLine2 = whole.transform.Find("RHLine2").gameObject;
        rightFist = whole.transform.Find("RHFist").gameObject;
        leftJoint1 = whole.transform.Find("LHJoint1").gameObject;
        leftJoint2 = whole.transform.Find("LHJoint2").gameObject;
        leftLine1 = whole.transform.Find("LHLine1").gameObject;
        leftLine2 = whole.transform.Find("LHLine2").gameObject;
        leftFist = whole.transform.Find("LHFist").gameObject;

#if TEST_RIGHT_LINE
        rightJoint2p = whole.transform.Find("RHJoint2 (1)").gameObject;
        rightLine1p = whole.transform.Find("RHLine1 (1)").gameObject;
        rightLine2p = whole.transform.Find("RHLine2 (1)").gameObject;
        rightJoint2p.SetActive(true);
        rightLine1p.SetActive(true);
        rightLine2p.SetActive(true);
#endif

        length1 = rightJoint1.transform.localScale.x / 2 + rightLine1.transform.localScale.x + rightJoint2.transform.localScale.x / 2;
        length2 = rightJoint2.transform.localScale.x / 2 + rightLine2.transform.localScale.x + rightFist.transform.localScale.x / 2;
        length = length1 + length2;

        rightJoint2State = Joint2State.Line;
        leftJoint2State = Joint2State.Line;
        rightJoint2StatePre = Joint2State.Line;
        leftJoint2State = Joint2State.Line;
        rightFistPosPre = rightFist.transform.localPosition;
        leftFistPosPre = leftFist.transform.localPosition;
    }

    public void Refresh()
    {
        rightJoint2StatePre = rightJoint2State;
        leftJoint2StatePre = leftJoint2State;

        RefreshRight();
        RefreshLeft();

        rightFistPosPre = rightFist.transform.localPosition;
        leftFistPosPre = leftFist.transform.localPosition;
    }
    public static bool currentLR = false;
    void RefreshRight()
    {
        currentLR = true;
        Vector2 joint2PosPre = rightJoint2.transform.localPosition;
        Vector2 joint2Pos;
        Vector2 line1Pos;
        Quaternion line1Rotation;
        Vector2 line2Pos;
        Quaternion line2Rotation;
        Quaternion fistRotation;
        CalulateShapeRight(
            rightFist.transform.localPosition, rightFistPosPre,
            rightJoint1.transform.localPosition, 
            joint2PosPre, rightJoint2StatePre,
            rightJoint1.transform.localScale.x, rightJoint2.transform.localScale.x,
            rightLine1.transform.localScale.x, rightLine2.transform.localScale.x,
            length1, length2, length,
            out joint2Pos, out rightJoint2State,
            out line1Pos, out line1Rotation, 
            out line2Pos, out line2Rotation,
            out fistRotation
            );

        rightJoint2.transform.localPosition = joint2Pos;
        rightLine1.transform.localPosition = line1Pos;
        rightLine1.transform.localRotation = line1Rotation;
        rightLine2.transform.localPosition = line2Pos;
        rightLine2.transform.localRotation = line2Rotation;
        rightFist.transform.localRotation = fistRotation;
#if TEST_RIGHT_LINE
        CalulateShapeRight2(
            rightFist.transform.localPosition, rightFistPosPre,
            rightJoint1.transform.localPosition, 
            joint2PosPre, rightJoint2StatePre,
            rightJoint1.transform.localScale.x, rightJoint2.transform.localScale.x,
            rightLine1.transform.localScale.x, rightLine2.transform.localScale.x,
            length1, length2, length,
            out joint2Pos, out rightJoint2State,
            out line1Pos, out line1Rotation, 
            out line2Pos, out line2Rotation,
            out fistRotation
            );
        rightJoint2p.transform.localPosition = joint2Pos;
        rightLine1p.transform.localPosition = line1Pos;
        rightLine1p.transform.localRotation = line1Rotation;
        rightLine2p.transform.localPosition = line2Pos;
        rightLine2p.transform.localRotation = line2Rotation;

        Yurowm.DebugTools.DebugPanel.Log("Join2 Dis", "Distance", (rightJoint2.transform.localPosition - rightJoint2p.transform.localPosition).magnitude);
#endif
    }

    void RefreshLeft()
    {
        currentLR = false;
        //左手需要通过镜像的方式计算
        Vector2 lhFistPosMirred = new Vector2(-leftFist.transform.localPosition.x, leftFist.transform.localPosition.y);
        Vector2 leftFistPosPreMirred = new Vector2(-leftFistPosPre.x, leftFistPosPre.y);
        Vector2 lhJoint1PosMirred = new Vector2(-leftJoint1.transform.localPosition.x, leftJoint1.transform.localPosition.y);
        Vector2 lhJoint2PosPreMirred = new Vector2(-leftJoint2.transform.localPosition.x, leftJoint2.transform.localPosition.y);


        Vector2 joint2PosMirred;
        Vector2 line1PosMirred;
        Quaternion line1RotationMirred;
        Vector2 line2PosMirred;
        Quaternion line2RotationMirred;
        Quaternion fistRotationMirred;

        CalulateShapeRight(
            lhFistPosMirred, leftFistPosPreMirred,
            lhJoint1PosMirred,
            lhJoint2PosPreMirred, leftJoint2StatePre,
            leftJoint1.transform.localScale.x, leftJoint2.transform.localScale.x,
            leftLine1.transform.localScale.x, leftLine2.transform.localScale.x,
            length1, length2, length,
            out joint2PosMirred, out leftJoint2State,
            out line1PosMirred, out line1RotationMirred, 
            out line2PosMirred, out line2RotationMirred,
            out fistRotationMirred
            );
        leftJoint2.transform.localPosition = new Vector2(-joint2PosMirred.x, joint2PosMirred.y);
        leftLine1.transform.localPosition = new Vector2(-line1PosMirred.x, line1PosMirred.y);
        Vector3 axis;
        float angle;
        line1RotationMirred.ToAngleAxis(out angle, out axis);
        leftLine1.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
        leftLine2.transform.localPosition = new Vector2(-line2PosMirred.x, line2PosMirred.y);
        line2RotationMirred.ToAngleAxis(out angle, out axis);
        leftLine2.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
        fistRotationMirred.ToAngleAxis(out angle, out axis);
        leftFist.transform.localRotation = Quaternion.AngleAxis(-angle, axis);
    }

    static void CalulateShapeRight(
        in Vector2 fistPos, in Vector2 fistPosPre, 
        in Vector2 joint1Pos,
        in Vector2 joint2PosPre, in Joint2State joint2StatePre,
        in float joint1ScaleX, in float joint2ScaleX,
        in float line1ScaleX, in float line2ScaleX,
        in float length1, in float length2, in float length,
        out Vector2 joint2Pos, out Joint2State joint2State,
        out Vector2 line1Pos, out Quaternion line1Rotation,
        out Vector2 line2Pos, out Quaternion line2Rotation,
        out Quaternion fistRotation
    )
    {
        GetShapeRightJoint2Pos_OptimizedAlwaysLeft(
            fistPos, fistPosPre, 
            joint1Pos,
            joint2PosPre, joint2StatePre,
            length1, length2, length,
            out joint2Pos, out joint2State);

        GetShapeRightOther(
            fistPos, joint1Pos, joint2Pos,
            joint1ScaleX, joint2ScaleX,
            line1ScaleX, line2ScaleX,
            length1, length2, length,
            out line1Pos, out line1Rotation,
            out line2Pos, out line2Rotation,
            out fistRotation);
    }

    static void CalulateShapeRight2(
    in Vector2 fistPos, in Vector2 fistPosPre,
    in Vector2 joint1Pos,
    in Vector2 joint2PosPre, in Joint2State joint2StatePre,
    in float joint1ScaleX, in float joint2ScaleX,
    in float line1ScaleX, in float line2ScaleX,
    in float length1, in float length2, in float length,
    out Vector2 joint2Pos, out Joint2State joint2State,
    out Vector2 line1Pos, out Quaternion line1Rotation,
    out Vector2 line2Pos, out Quaternion line2Rotation,
    out Quaternion fistRotation
)
    {
        GetShapeRightJoint2Pos_SimpleAlwaysLeft(
            fistPos, fistPosPre,
            joint1Pos,
            joint2PosPre, joint2StatePre,
            length1, length2, length,
            out joint2Pos, out joint2State);

        GetShapeRightOther(
            fistPos, joint1Pos, joint2Pos,
            joint1ScaleX, joint2ScaleX,
            line1ScaleX, line2ScaleX,
            length1, length2, length,
            out line1Pos, out line1Rotation,
            out line2Pos, out line2Rotation,
            out fistRotation);
    }

    static void GetShapeRightJoint2Pos_Reflect (
        in Vector2 fistPos, in Vector2 fistPosPre,
        in Vector2 joint1Pos,
        in Vector2 joint2PosPre, in Joint2State joint2StatePre,
        in float length1, in float length2, in float length,
        out Vector2 joint2Pos, out Joint2State joint2State)
    {
        //现在算法还不太对



        //the triangle
        /*
            现在 b 和 c必须相等，否则三角形判断有问题，还需要增加一些处理。
            不是很有必要增加处理。所以，就让b和c相等吧。
        */
        float a = (new Vector2(fistPos.x, fistPos.y) - joint1Pos).magnitude;
        float b = length1;
        float c = length2;
        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"---------------------");
        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"a == {a}");
        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"b == {b}");
        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"c == {c}");
        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"b + c > a && !MyTool.FloatEqual0p001(b+c, a) == {b + c > a && !MyTool.FloatEqual0p001(b + c, a)}");
        //posJoint2
        /*
            已经放弃了选择更小距离的算法。
            因为，在极值点和直线状态，统一起来比较复杂。
            （本来觉得，用“选择更小距离的算法”，在极值点和直线状态，可以统计解决）


            分成3种状态：
            极值点状态
            直线状态
            三角形状态

            在三角形状态时，计算需注意。
            如果上一个状态也是三角形，则左右不变。
            如果上一个状态是直线状态，那切换到三角状态时，一定选择左。
                可能存在上一个状态是三角，当前也是三角，但中间经过了直线状态的情况。
                就目前的情况来看，在这种情况下总是选左，表现也不会有问题。
            如果上一个状态是极值状态
                经过极值状态的切换，也要考虑距离。如果距离变大，没必要切换。比如右手一直在身体右侧运动的情况。
                
                可能存在上一个状态是三角，当前也是三角，但中间经过了极值状态的情况。
                这种情况也算作是上一个状态是极值状态。
                (虽然，极值状态的距离定义，结合长度和速度，可以回避这种情况。但，回避这种情况的算法似乎比较麻烦，所以，我就不回避了。)
                
                如果经过极值状态，则三角形的Joint2位置，会考虑改变(根据距离，可能改可能不改)。

            极值状态的距离需要定义。
            这个定义，与计算Joint2位置的极值状态定义，应该可以不同。
            Joint2位置的极值状态定义，似乎是越小越好。奥，不读，其实不用定义大小，就是dis==0，在极值点定义即可。我定义大小，只是源于float精度的问题。
            极值状态的距离定义，则需要取得平衡。在变化足够连贯，不用改变偏移方向的时候，不改变。在变化连贯时，改变。
            这个距离，其实取决于手的长度和速度。但是，关系不容易理清楚，所以，把这个距离拿出来，由我手动调整。

            最后，在算法实现上。由于在极值状态，还是画三角形（除了源于float精度的极值点状态外）。
            所以，是在三角形部分加一些判断，而判断的极值点状态，是float精度的极值点状态。
            但是，在三角形内部算法中，是否经过，则用的是一个范围的极值点状态。(用一个距离来评价经过时是否需要考虑改状态，但是否进过其实还是判断的那个单点的极值点)
        */
        float singularityRadius_joint2Position = 0.77f;
        float singularityRadius_ifPassSigularity = 0.3f;
        //非三角形1，且手和根，几乎重合
        if ( a <= singularityRadius_joint2Position )
        {
            /*
                当手和根几乎重合时，Joint2应该在哪比较好呢？
                目前想到最合适的方法是，根据 joint2PosPre 和 手的移动反向，确定一条线，取圆上里线最近的一点
                这样比较麻烦，因此，取圆上离joint2PosPre最近的一个点，也不错。（即从圆心向joint2PosPre作射线，射线与圆的交点）
                因为当手和根几乎重合时，在每步移动范围很小的情况下，joint2PosPre也是在圆上的，所以，取joint2PosPre即可。
            */
            joint2Pos = joint2PosPre;
            joint2State = Joint2State.Singularity;
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), "Current == Sigularity");
        }
        //三角形
        else if (b + c > a && !MyTool.FloatEqual0p001(b+c, a))
        {

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
            float distanceLeft = (joint2Pos_pointOnLeft - joint2PosPre).magnitude;
            float distanceRight = (joint2Pos_pointOnRight - joint2PosPre).magnitude;
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"Current == Triangle");
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"Pre:({joint2PosPre.x},{joint2PosPre.y})");
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"Left:({joint2Pos_pointOnLeft.x},{joint2Pos_pointOnLeft.y})");
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"Right:({joint2Pos_pointOnRight.x},{joint2Pos_pointOnRight.y})");

            joint2Pos = new Vector2();
            //如果上一个状态是Line，就直接选左
            if (joint2StatePre == Joint2State.Line)
            {
                joint2Pos = joint2Pos_pointOnLeft;
                if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), "Pre == Line");
            }
            /*
                如果上一个状态是Singularity，就根据距离来选
                如果奇点之后和奇点之前的移动方向是同向 或 反向，那根据距离来选即可
                如果奇点之后和奇点之前的移动方向不同，那其实还有个优先选左的问题
                
                如果考虑到小的偏移，这个优先选左，又需要加上一个阈值来让左优先。
                如果让这个阈值较大的话，可以通过手长来判断。
            */
            else if (joint2StatePre == Joint2State.Singularity)
            {
                float bigThreshold = length/2 * 1;
                if (Mathf.Abs(distanceLeft - distanceRight) <= bigThreshold)
                {
                    joint2Pos = joint2Pos_pointOnLeft;
                }
                else if (distanceLeft < distanceRight)
                {
                    joint2Pos = joint2Pos_pointOnLeft;
                }
                else
                {
                    joint2Pos = joint2Pos_pointOnRight;
                }

                if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), "Pre == Singularity");

            }
            else if (joint2StatePre.IsTriangle())
            {
                /*
                    如果前后未经过极值状态，则保持方向不变。
                    如果经过了，则选择距离较近的点。经过，则一定前后的运动是同向，就不用考虑普遍偏左的阈值问题，只要比较距离大小即可。
                    这里涉及到极值状态的范围。
                    先拍脑门定一个。
                */
                if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), "Pre == Triangle");
                /*
                    如何判断是否经过极值状态？
                    从手的根点，向两点所成直线作垂线。（若两点重合，则一定不经过）
                    垂线和直线有一个交点。
                    如果交点到手根点的距离大于radius，则一定不经过。
                    如果交点到手根点的距离小于radius，再判断交点是否在两点之间。(用(交点-startPoint)/(endPoint-startPoint),看是不是在0~1即可。)
                    (奥，还有个简单的做法，就是说 交点到2个点的向量反向，dot<=0，就说明交点在2个点之间)
                 
                */
                bool byPassSingularity = false;
                if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"MyTool.FloatEqual0p001((joint2Pos - joint2PosPre).magnitude == {MyTool.FloatEqual0p001((joint2Pos - joint2PosPre).magnitude,0)}");
                if (MyTool.FloatEqual0p001((joint2Pos - joint2PosPre).magnitude, 0))
                {
                    byPassSingularity = false;
                }
                else
                {
                    Vector2 nearestPoint = MyTool.FindNearestPointOnLine(fistPosPre, fistPos - fistPosPre, joint1Pos);
                    if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"nearestPoint == {nearestPoint}");
                    if ((joint1Pos - nearestPoint).magnitude <= singularityRadius_ifPassSigularity)
                    {
                        Vector2 line1 = nearestPoint - fistPos;
                        Vector2 line2 = nearestPoint - fistPosPre;
                        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"line1 == {line1}");
                        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"line2 == {line2}");
                        if (Vector2.Dot(line1, line2) <= 0)
                        {
                            byPassSingularity = true;
                        }
                        else
                        {
                            byPassSingularity = false;
                        }
                    }
                    else
                    {
                        byPassSingularity = false;
                    }
                }

                if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"byPassSingularity == {byPassSingularity}");

                if (byPassSingularity == false)
                {
                    if (joint2StatePre == Joint2State.TriangleLeft)
                        joint2Pos = joint2Pos_pointOnLeft;
                    else
                        joint2Pos = joint2Pos_pointOnRight;
                }
                else
                {
                    if (distanceLeft < distanceRight)
                    {
                        joint2Pos = joint2Pos_pointOnLeft;
                    }
                    else
                    {
                        joint2Pos = joint2Pos_pointOnRight;
                    }
                    if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"distanceLeft:{distanceLeft}");
                    if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), $"distanceRight:{distanceRight}");
                }

            }
            if (joint2Pos == joint2Pos_pointOnLeft)
            {
                joint2State = Joint2State.TriangleLeft;
            }
            else
            {
                joint2State = Joint2State.TriangleRight;
            }

        }
        //非三角形，手和根离得的很远
        else
        {
            Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;
            Vector2 offset = Vector2.Lerp(new Vector2(), joint1ToFist, length1 / length);
            joint2Pos = joint1Pos + offset;
            joint2State = Joint2State.Line;
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogJoint2"), "Current == Line");
        }
    }

    static void GetShapeRightJoint2Pos_SimpleAlwaysLeft(
        in Vector2 fistPos, in Vector2 fistPosPre,
        in Vector2 joint1Pos,
        in Vector2 joint2PosPre, in Joint2State joint2StatePre,
        in float length1, in float length2, in float length,
        out Vector2 joint2Pos, out Joint2State joint2State)
    {

        float a = (new Vector2(fistPos.x, fistPos.y) - joint1Pos).magnitude;
        float b = length1;
        float c = length2;

        if (a <= 0.001f)
        {
            joint2Pos = joint2PosPre;
            joint2State = Joint2State.Singularity;
        }
        //三角形
        else if (b + c > a && !MyTool.FloatEqual0p001(b + c, a))
        {
            float cosJoint1 = (a * a + b * b - c * c) / (2 * a * b);
            float angleJoint1 = Mathf.Acos(cosJoint1);
            Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;

            Vector2 joint2Pos_pointOnLeft;
            Vector2 joint1ToJoint2_pointOnLeft = MyTool.Vec2Rotate(joint1ToFist.normalized, -angleJoint1) * length1;
            joint2Pos_pointOnLeft = joint1Pos + joint1ToJoint2_pointOnLeft;

            joint2Pos = joint2Pos_pointOnLeft;
            joint2State = Joint2State.TriangleLeft;
        }
        else
        {
            Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;
            Vector2 offset = Vector2.Lerp(new Vector2(), joint1ToFist, length1 / length);
            joint2Pos = joint1Pos + offset;
            joint2State = Joint2State.Line;

        }
    }

    static void GetShapeRightJoint2Pos_OptimizedAlwaysLeft(
        in Vector2 fistPos, in Vector2 fistPosPre,
        in Vector2 joint1Pos,
        in Vector2 joint2PosPre, in Joint2State joint2StatePre,
        in float length1, in float length2, in float length,
        out Vector2 joint2Pos, out Joint2State joint2State)
    {
        float radius = 1f;
        float a = (new Vector2(fistPos.x, fistPos.y) - joint1Pos).magnitude;
        float b = length1;
        float c = length2;

        if (a <= 0.001f)
        {
            joint2Pos = joint2PosPre;
            joint2State = Joint2State.Singularity;
        }
        //三角形
        else if (b + c > a && !MyTool.FloatEqual0p001(b + c, a))
        {
            //如果在radius范围内，则在计算Joint2的位置时，把移动到圆的边上。
            Vector2 newFistPos = fistPos;
            if (a < radius)
            {
                Vector2 joint1ToFistDirection = (new Vector2(fistPos.x, fistPos.y) - joint1Pos).normalized;
                newFistPos = joint1Pos + joint1ToFistDirection * radius;
                a = (new Vector2(newFistPos.x, newFistPos.y) - joint1Pos).magnitude;
            }

            float cosJoint1 = (a * a + b * b - c * c) / (2 * a * b);
            float angleJoint1 = Mathf.Acos(cosJoint1);
            Vector2 joint1ToFist = new Vector2(newFistPos.x, newFistPos.y) - joint1Pos;

            Vector2 joint2Pos_pointOnLeft;
            Vector2 joint1ToJoint2_pointOnLeft = MyTool.Vec2Rotate(joint1ToFist.normalized, -angleJoint1) * length1;
            joint2Pos_pointOnLeft = joint1Pos + joint1ToJoint2_pointOnLeft;

            joint2Pos = joint2Pos_pointOnLeft;
            joint2State = Joint2State.TriangleLeft;
        }
        else
        {
            Vector2 joint1ToFist = new Vector2(fistPos.x, fistPos.y) - joint1Pos;
            Vector2 offset = Vector2.Lerp(new Vector2(), joint1ToFist, length1 / length);
            joint2Pos = joint1Pos + offset;
            joint2State = Joint2State.Line;

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
        if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogGetShapeRightOther"), $"joint2Pos:({joint2Pos.x},{joint2Pos.y})");
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
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogGetShapeRightOther"), $"joint1ToJoint2:({joint1ToJoint2.x},{joint1ToJoint2.y})");
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogGetShapeRightOther"), $"angleLine1:{angleLine1}");
            line1Rotation = Quaternion.Euler(0, 0, angleLine1);
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogGetShapeRightOther"), $"line1Rotation:{line1Rotation}");
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
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogGetShapeRightOther"), $"joint2ToFist:({joint2ToFist.x},{joint2ToFist.y})");
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogGetShapeRightOther"), $"angleLine2:{angleLine2}");
            line2Rotation = Quaternion.Euler(0, 0, angleLine2);
            if (currentLR) FastFileLog.LogManager.Log(GameObject.Find("LogGetShapeRightOther"), $"line1Rotation:{line2Rotation}");
            fistRotation = line2Rotation;
        }
    }
}