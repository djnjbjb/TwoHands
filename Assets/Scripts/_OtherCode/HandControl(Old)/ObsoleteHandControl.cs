using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludo.Extensions;
using Ludo;

public class ObsoleteHandControl : MonoBehaviour
{
    //void FU_Main_0TwoHandsAltPressed()
    //{
    //    /*
    //    用【下】表示绕着那只手旋转
    //        身体旋转，像是2指手握着方向盘，然后用力，方向盘发生旋转，人跟着旋转

    //    1.如果两只手都按着同一个方向，则向那个方向移动
    //        双倍速度
    //    2.如果一只手按着下，另一只手按别的，则旋转并配合另一只手移动
    //        2.1如果是下 + 无，则只是旋转
    //        2.2如果是下 + 其他，则旋转 + 移动
    //    3.如果两只手按着不同方向，且没有一个是下，则不响应
    //    */
    //    Vector2 rhDirection = -HKey.rMvDir;
    //    Vector2 lhDirection = -HKey.lMvDir;
    //    if (rhDirection == lhDirection)
    //    {
    //        /*
    //        同时移动时，需要考量两只手的移动距离。
    //            想象一只手已经移动到尽头，另一只手没到的情况。

    //        如果两只手都能以2倍速移动，则以2倍速移动
    //        如果一只手不行，则
    //            两只手可共同移动的长度，两只手以双倍速移动
    //            剩下的部分，那只手以但倍速移动

    //        设2倍速距离为dis2。
    //        首先，求出两只手在方向上可移动的距离。l1，r1
    //        （由于求l1,r1较为复杂，可以先尝试移动dis2，然后Clamp。这样得到的l1、r1，为 l1 = min(dis2,l1),r1=min(dis2,r1)，也能用于后面的计算，不影响）
    //        取得l1,r1中较小min1，另一个是max1。
    //        与dis2比较。
    //        如果大于等于，则
    //            手移动2倍速距离。
    //            身体移动2倍速距离。
    //        如果小于，则
    //            较小的那个手，移动min1
    //            另一个手，移动 min(   max1, min1+(dis2-min1)/2  )
    //            身体，移动 min(   max1, min1+(dis2-min1)/2  )
    //        */

    //        float dis2x = (fistMoveSpeed * 2) * Time.fixedDeltaTime;
    //        Vector2 mvVector = rhDirection * (fistMoveSpeed * 2) * Time.fixedDeltaTime;
    //        Vector2 rhFistPosOld = rhFist.transform.localPosition;
    //        Vector2 rhFistPosNew = RightFistMove(rhFistPosOld, mvVector);
    //        Vector2 lhFistPosOld = lhFist.transform.localPosition;
    //        Vector2 lhFistPosNew = LeftFistMove(lhFistPosOld, mvVector);
    //        float rhDis = (rhFistPosNew - rhFistPosOld).magnitude;
    //        float lhDis = (lhFistPosNew - lhFistPosOld).magnitude;
    //        float smallerDis = 0;
    //        float biggerDis = 0;
    //        GameObject smallerFist = rhFist;
    //        GameObject biggerFist = rhFist;
    //        if (rhDis > lhDis)
    //        {
    //            smallerDis = lhDis;
    //            smallerFist = lhFist;
    //            biggerDis = rhDis;
    //            biggerFist = rhFist;
    //        }
    //        else
    //        {
    //            smallerDis = rhDis;
    //            smallerFist = rhFist;
    //            biggerDis = lhDis;
    //            biggerFist = lhFist;
    //        }

    //        if (Ludo.Utility.FloatEqual0p001(smallerDis, dis2x) || smallerDis >= dis2x)
    //        {
    //            rhFist.transform.localPosition = rhFistPosNew;
    //            lhFist.transform.localPosition = lhFistPosNew;

    //            Vector2 offset = rhFistPosNew - rhFistPosOld;
    //            offset = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale) * (Vector3)offset;
    //            Vector2 wholePersonPosNew = (Vector2)wholePerson.transform.localPosition - offset;
    //            wholePerson.transform.localPosition = wholePersonPosNew;
    //        }
    //        else
    //        {
    //            Vector2 smallerMove = smallerDis * rhDirection;
    //            biggerDis = Mathf.Min(biggerDis, smallerDis + (dis2x - smallerDis) / 2);
    //            Vector2 biggerMove = biggerDis * rhDirection;

    //            smallerFist.transform.localPosition += (Vector3)smallerMove;
    //            biggerFist.transform.localPosition += (Vector3)biggerMove;

    //            Vector3 offset = Matrix4x4.TRS(wholePerson.transform.localPosition, wholePerson.transform.localRotation, wholePerson.transform.localScale) * (Vector3)biggerMove;
    //            wholePerson.transform.localPosition += -offset;
    //        }
    //    }
    //    else if (Ludo.Utility.FloatEqual0p001(rhDirection.y, 1) || Ludo.Utility.FloatEqual0p001(lhDirection.y, 1))
    //    {
    //        if (Ludo.Utility.FloatEqual0p001(rhDirection.y, 1))
    //        {
    //            /*
    //                如果只是旋转，则旋转。
    //                如果旋转和移动结合，其实是个积分。
    //                    如果想算得好一点，等于(先旋转一半)→(再移动)→(再旋转)。
    //                    这里就凑活算了。让它等于：先移动，再旋转。
    //                综合而言，两种情况都需要旋转。不同是，需要移动时，先移动一下。

    //                所以，代码写成
    //                1.如果需要移动，先移动。
    //                2.然后，旋转。
    //            */
    //            if (!Ludo.Utility.FloatEqual0p001(lhDirection.magnitude, 0))
    //            {
    //                FU_Fist_Grab_Left();
    //            }

    //            float angle_Deg = fistRotateSpeed_Degree * Time.fixedDeltaTime;
    //            Vector2 offset;
    //            float angleWholePerson;
    //            FU_Main_1RotateRoundRight_Params(angle_Deg, rhFist, wholePerson, out offset, out angleWholePerson);

    //            wholePerson.transform.localPosition += (Vector3)offset;
    //            wholePerson.transform.Rotate(new Vector3(0, 0, angleWholePerson), Space.Self);
    //        }
    //        else if (Ludo.Utility.FloatEqual0p001(lhDirection.y, 1))
    //        {

    //            if (!Ludo.Utility.FloatEqual0p001(rhDirection.magnitude, 0))
    //            {
    //                FU_Fist_Grab_Right();
    //            }

    //            //沿左手旋转，需要根据右手做镜像
    //            float angle_Deg = fistRotateSpeed_Degree * Time.fixedDeltaTime;
    //            Vector2 mirredOffset;
    //            float mirredAngleWholePerson;
    //            Vector2 rhOldPos = rhFist.transform.localPosition;
    //            Vector2 lhMirredPos = new Vector2(-lhFist.transform.localPosition.x, lhFist.transform.localPosition.y);
    //            rhFist.transform.localPosition = lhMirredPos;
    //            FU_Main_1RotateRoundRight_Params(angle_Deg, rhFist, wholePerson, out mirredOffset, out mirredAngleWholePerson);
    //            rhFist.transform.localPosition = rhOldPos;

    //            //向量的mirror，要根据wholePerson的中轴来做
    //            Vector2 wholeAxis = (wholePerson.transform.localRotation * new Vector3(0, 1, 0)).normalized;
    //            Vector2 offset = -Vector2.Reflect(mirredOffset, wholeAxis);
    //            wholePerson.transform.localPosition += (Vector3)offset;
    //            wholePerson.transform.Rotate(new Vector3(0, 0, -mirredAngleWholePerson), Space.Self);
    //        }
    //    }
    //    else
    //    {
    //        //do nothing
    //    }
    //}

    void FU_WholeTemp()
    {
        //把手的部分中，whole可能用到的代码，先放到这里。


        //接下来是双手移动的代码
        /*
            注意，offset的方向，可能和fistVelocity不同。
            比如我斜向移动，但是到头了，于是结果可能是横向移动。
        */
        //float rhDis = rightFistOffset.magnitude;
        //float lhDis = leftFistOffset.magnitude;
        //float smallerDis = 0;
        //float biggerDis = 0;
        //GameObject smallerFist = rightFist;
        //GameObject biggerFist = rightFist;
        //if (rhDis > lhDis)
        //{
        //    smallerDis = lhDis;
        //    smallerFist = leftFist;
        //    biggerDis = rhDis;
        //    biggerFist = rightFist;
        //}
        //else
        //{
        //    smallerDis = rhDis;
        //    smallerFist = rightFist;
        //    biggerDis = lhDis;
        //    biggerFist = leftFist;
        //}
        //if (Ludo.Utility.FloatEqual0p001(biggerDis, 0))
        //{
        //    //如果双手移动都是0
        //    //位置都不动，手和身体都减速
        //    fistSpeed = SpeedDecelerate(fistSpeedPre);
        //    rightFistVelocity = fistSpeed * rhDirection;
        //    leftFistVelocity = fistSpeed * lhDirection;

        //    wholeGrabEnvOffset = new Vector2();
        //    wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
        //}
        //else if (Ludo.Utility.FloatEqual0p001(smallerDis, 0) && !Ludo.Utility.FloatEqual0p001(biggerDis, 0))
        //{
        //    //如果一只手的移动是0
        //    //身体和移动的那只手，等效于单手移动
        //    //另一只手减速
        //    if (biggerFist == rightFist)
        //    {
        //        FU_Fist_GrabEnv_Right();
        //        leftFistVelocity = SpeedDecelerate(leftFistVelocity.magnitude) * lhDirection;
        //    }
        //    else if (biggerFist == leftFist)
        //    {
        //        FU_Fist_GrabEnv_Left();
        //        rightFistVelocity = SpeedDecelerate(rightFistVelocity.magnitude) * rhDirection;
        //    }
        //}
        //else
        //{
        //    /*  
        //        双手移动都不是0
        //            其实分2种情况，一种是都可以移动到dis2x，一种是不行，但在计算时没有区别

        //        如果双手的移动方向一致：
        //        双手都移动到对应的dis。身体的offset是biggerDis。
        //        双手速度都增加到speed。身体速度则是-speed。

        //        注意，offset的方向，可能和fistVelocity不同。
        //        这时应如何计算呢？
        //        手的移动，依然是相应的dis。手的速度变化，比较难搞，就简单化，依然增加到speed。
        //        身体的移动，取决于2个dis，x方向上的最大分量和y方向的最大分量。身体的速度，也增加到speed，但反向。
        //    */
        //    Vector2 wholeOffset = new Vector2();
        //    Vector2 rhOffset = rhFistPosNew - rhFistPosOld;
        //    Vector2 lhOffset = lhFistPosNew - lhFistPosOld;
        //    if (Vector2.Dot(rhOffset, Vector2.right) >= Vector2.Dot(lhOffset, Vector2.right))
        //    {
        //        wholeOffset += Vector2.Dot(rhOffset, Vector2.right) * Vector2.right;
        //    }
        //    else
        //    {
        //        wholeOffset += Vector2.Dot(lhOffset, Vector2.right) * Vector2.right;
        //    }
        //    if (Vector2.Dot(rhOffset, Vector2.down) >= Vector2.Dot(lhOffset, Vector2.down))
        //    {
        //        wholeOffset += Vector2.Dot(rhOffset, Vector2.down) * Vector2.down;
        //    }
        //    else
        //    {
        //        wholeOffset += Vector2.Dot(lhOffset, Vector2.down) * Vector2.down;
        //    }

        //    //----------------------------------------------------
        //    rightFist.transform.localPosition = rhFistPosNew;
        //    leftFist.transform.localPosition = lhFistPosNew;
        //    rightFistVelocity = fistSpeed * rhDirection;
        //    leftFistVelocity = fistSpeed * lhDirection;

        //    wholeGrabEnvOffset = wholeMatrix * (-wholeOffset);
        //    /*
        //        如果双手移动同向，同时又和rhDirection不同，那么这里的速度会有问题
        //        应为 wholeMatrix * (-fistSpeed * wholeGrabEnvOffset.normal)
        //        暂时先不改
        //    */
        //    wholeVelocity = wholeMatrix * (-fistSpeed * rhDirection);
        //}

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
        Vector2 fistToWholeAfterRotate = Utility.Vec2Rotate(fistToWhole, -angle_Deg * Mathf.Deg2Rad);

        offset = fistToWholeAfterRotate - fistToWhole;
        angleWholePerson_Deg = -angle_Deg;
    }
}
