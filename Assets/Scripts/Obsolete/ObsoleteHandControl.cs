using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //        if (MyTool.FloatEqual0p001(smallerDis, dis2x) || smallerDis >= dis2x)
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
    //    else if (MyTool.FloatEqual0p001(rhDirection.y, 1) || MyTool.FloatEqual0p001(lhDirection.y, 1))
    //    {
    //        if (MyTool.FloatEqual0p001(rhDirection.y, 1))
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
    //            if (!MyTool.FloatEqual0p001(lhDirection.magnitude, 0))
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
    //        else if (MyTool.FloatEqual0p001(lhDirection.y, 1))
    //        {

    //            if (!MyTool.FloatEqual0p001(rhDirection.magnitude, 0))
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
}
