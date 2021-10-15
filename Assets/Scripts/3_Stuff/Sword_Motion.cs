using PlayerControlTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Ludo.Utility;

namespace Stuff
{
    public partial class Sword : Stuff
    {
        (Vector3 position, Quaternion rotation) Motion_HandAndFree(Vector2 handOffset, Vector2 positionOffset, GameObject fistObject)
        {
            Vector3 position = Motion_HandAndFree_Position(handOffset, positionOffset, fistObject);
            Quaternion rotation = Motion_HandAndFree_Rotation(handOffset);

            //History
            if (!(states.statePre == State.WithHand && states.withHandStatePre == WithHandState.Free))
            {
                handAndFreeOffsetHistory = new List<Vector2>();
                handAndFreeOffsetHistory.Add(handOffset);
            }
            else
            {
                handAndFreeOffsetHistory.Add(handOffset);
            }

            return (position, rotation);
        }

        Vector3 Motion_HandAndFree_Position(Vector2 handOffset, Vector2 positionOffset, GameObject fistObject)
        {
            Vector3 targetPosition = transform.position;
            targetPosition += (Vector3)positionOffset;
            if (holdPosition.transform.position != fistObject.transform.position)
            {
                Vector2 delta = (Vector2)(fistObject.transform.position - holdPosition.transform.position);
                float distance = delta.magnitude;
                float maxMove = toHolderPositionSpeed * Time.fixedDeltaTime;
                if (maxMove > distance)
                {
                    targetPosition += (Vector3)delta;
                }
                else
                {
                    targetPosition += (Vector3)(delta.normalized * maxMove);
                }
            }
            return targetPosition;
        }

        Quaternion Motion_HandAndFree_Rotation(Vector2 handOffset)
        {
            //准备工作1，如果上一帧不是HandAndFree，handAndFree_LastNonZeroHandOffset置为零。
            if (!(states.statePre == State.WithHand && states.withHandStatePre == WithHandState.Free))
            {
                handAndFree_LastNonZeroHandOffset = Vector2.zero;
            }

            //准备工作2
            Vector2 swordDirection = tip.transform.position - body.transform.position;
            handOffset = Tool.ArbitraryDirectionToNineDirection(handOffset);
            if (!FloatEqual_WithIn0p001(handOffset.magnitude, 0))
            {
                handAndFree_LastNonZeroHandOffset = handOffset;
            }
            /*
                1.
                根据手的运动方向，决定旋转方向
                    手的运动方向已被转为9向
                    如果手的运动方向为0，则使用handAndFree_LastNonZeroHandOffset来计算
                    （handAndFree_LastNonZeroHandOffset也可能为0）


                手没有运动
                    不旋转
                手运动
                    方向平行
                        不旋转
                    方向相反
                        逆时针旋转
                    有垂直方向的分量
                        根据力矩旋转

                ------------
                我这里好像有点傻。因为想的是力矩，所以就用叉乘，然后有了一系列操作。
                只要用Signed Angle，看handOffset和swordDirection夹角，看是正的还是负的，就可以....
            */
            float speedDirection;
            {
                if (FloatEqual_WithIn0p001(handOffset.magnitude, 0))
                {
                    handOffset = handAndFree_LastNonZeroHandOffset;
                }
                if (FloatEqual_WithIn0p001(handOffset.magnitude, 0))
                {
                    speedDirection = 0f;
                    goto AccelerationDirectionFinish;
                }

                Vector2 HONormalized = handOffset.normalized;
                Vector2 SDNormalized = swordDirection.normalized;
                float crossProduct = SDNormalized.x * HONormalized.y - SDNormalized.y * HONormalized.x;
                /*
                    crossProduct是叉积，根据叉积，确定旋转方向。

                    Unity中，z为正，是逆时针

                    我试了一下，offset为（1,0）,direction为(0,1)，
                        应该顺时针旋转(旋转方向为负)
                        算出来的crossProduct 为 -1

                    所以，根据crossProduct求旋转方向，旋转方向和crossProduct的正负号相同
                */
                if (FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 0))
                {
                    speedDirection = 0f;
                    goto AccelerationDirectionFinish;
                }
                if (FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 180))
                {
                    speedDirection = 1f;
                    goto AccelerationDirectionFinish;
                }
                speedDirection = Mathf.Sign(crossProduct);

                AccelerationDirectionFinish:
                ;
            }

            /*
                2.
                速度

                感觉比较更好一点的方法是，类似于移动。加速有个过程，瞬间停止。
                不过，必要性不大，就直接匀速了。
            */
            angleSpeed = 180f / 0.3f;

            /*
                3.
                最终旋转角度
                    如果旋转经过了handOffset方向，需要重置方向，等同于handOffset方向。
                    角度方向是-180~180。
            */
            Quaternion targetRotation = transform.rotation;
            if (speedDirection == 0f)
            {
                return targetRotation;
            }
            float deltaAngle = angleSpeed * speedDirection * Time.fixedDeltaTime;
            float destAngle = Vector2.SignedAngle(swordDirection, handOffset);
            if (Mathf.Abs(deltaAngle) > Mathf.Abs(destAngle))
            {
                deltaAngle = destAngle;
            }

            targetRotation *= Quaternion.Euler(0, 0, deltaAngle);
            return targetRotation;
        }

        (Vector3 position, Quaternion rotation) Motion_HandAndEnv(Vector2 positionOffset)
        {
            Vector3 targetPosition = transform.position;
            Quaternion targetRotation = transform.rotation;

            Vector2 swordDirection = tip.transform.position - body.transform.position;
            Vector2 HONormalized = positionOffset.normalized;
            Vector2 SDNormalized = swordDirection.normalized;
            if (FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 0) ||
                FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 180))
            {
                
                targetPosition += (Vector3)positionOffset;
            }

            return (targetPosition, targetRotation);
        }

        (Vector3 position, Quaternion rotation) Motion_NoHand()
        {
            if (states.noHandState == NoHandState.Env)
            {
                (Vector3 position, Quaternion rotation) = (transform.position, transform.rotation);
                return Motion_NoHand_Env(position, rotation);
            }

            if (states.noHandState == NoHandState.Fly)
            {
                Motion_NoHand_OutOfCameraRegion();
                (Vector3 position, Quaternion rotation) = Motion_NoHand_AdjustDirectionRotateByCenter();
                return Motion_NoHand_Flying(position, rotation);
            }

            if (states.noHandState == NoHandState.Drop)
            {
                Motion_NoHand_OutOfCameraRegion();
                (Vector3 position, Quaternion rotation) = Motion_NoHand_AdjustDirectionRotateByCenter();
                return Motion_NoHand_Dropping(position, rotation);
            }

            throw new System.Exception("In Motion_NoHand(), state is not NoHandState.*");
        }

        (Vector3 position, Quaternion rotation) Motion_NoHand_AdjustDirectionRotateByCenter()
        {
            return Motion_NoHand_AdjustDirection("Center");
        }

        (Vector3 position, Quaternion rotation) Motion_NoHand_AdjustDirectionRotateByHoldPosition()
        {
            return Motion_NoHand_AdjustDirection("HoldPosition");
        }

        (Vector3 position, Quaternion rotation) Motion_NoHand_AdjustDirection(string rotateBy)
        {
            Vector3 targetPosition = transform.position;
            Quaternion targetRotation = transform.rotation;
            
            /*
                swordDirection不可能为零。
                motionDirection可能为零。
                motionDirection为零时，不转向。
            */

            Vector2 rotatePoint = centerPoint.position;
            if (rotateBy == "Center")
            {
                rotatePoint = centerPoint.position;
            }
            else if (rotateBy == "HoldPosition")
            {
                rotatePoint = holdPosition.transform.position;
            }
            else
            {
                throw new System.Exception("rotateBy should be one of {Center, HoldPosition}");
            }

            Vector2 swordDirection = (Vector2)(tip.transform.position - body.transform.position).normalized;
            Vector2 motionDirection = noHand_direction;
            if (!FloatEqual_WithIn0p001(Vector2.Angle(swordDirection, motionDirection), 0))
            {
                float angle = Vector2.SignedAngle(swordDirection, motionDirection);


                (targetPosition, targetRotation) = 
                    Ludo.Utility.RotateRoundPoint2D(transform.position, transform.rotation, rotatePoint, angle);
            }

            return (targetPosition, targetRotation);
        }

        void Motion_NoHand_OutOfCameraRegion()
        {
            //左右上 - 反弹，下 - 不反弹

            Camera camera = CameraFollow.onlyInstance.sceneCamera;
            float ratio = CameraFollow.screenRatio;
            float camera_half_height = camera.orthographicSize;
            float camera_half_width = camera_half_height * ratio;
            Ludo.AABB region = new Ludo.AABB();
            region.left = camera.transform.position.x - camera_half_width;
            region.right = camera.transform.position.x + camera_half_width;
            region.bottom = camera.transform.position.y - camera_half_height;
            region.top = camera.transform.position.y + camera_half_height;

            if (tip.transform.position.x < region.left)
            {
                if (noHand_direction.x < 0)
                {
                    noHand_direction.x = -noHand_direction.x;
                }
            }

            if (tip.transform.position.x > region.right)
            {
                if (noHand_direction.x > 0)
                {
                    noHand_direction.x = -noHand_direction.x;
                }
            }

            /*
            if (transform.position.y < region.bottom)
            {
                if (noHandMotion.direction.y < 0)
                {
                    noHandMotion.direction.y = -noHandMotion.direction.y;
                }
            }
            */

            if (tip.transform.position.y > region.top)
            {
                if (noHand_direction.y > 0)
                {
                    noHand_direction.y = -noHand_direction.y;
                }
            }
        }

        bool Motion_NoHand_RayCastEnv(ref Vector2 movement)
        {
            LayerMask env = LayerMask.GetMask("EnvGround", "EnvRock", "EnvRoundRock");
            Vector3 direction = movement.normalized;
            float distance = movement.magnitude;
            RaycastHit2D hit1 = Physics2D.Raycast(rayPoint1.position, direction, distance, env);
            RaycastHit2D hit2 = Physics2D.Raycast(rayPoint2.position, direction, distance, env);
            RaycastHit2D hit3 = Physics2D.Raycast(rayPoint3.position, direction, distance, env);

            if (hit1.collider || hit2.collider || hit3.collider)
            {
                float dis1 = float.MaxValue;
                if (hit1.collider)
                {
                    dis1 = hit1.distance;
                }
                float dis2 = float.MaxValue;
                if (hit2.collider)
                {
                    dis2 = hit2.distance;
                }
                float dis3 = float.MaxValue;
                if (hit3.collider)
                {
                    dis3 = hit3.distance;
                }
                float dis = Mathf.Min(dis1, dis2, dis3);
                movement = direction * dis;
                return true;
            }
            return false;
        }

        (Vector3 position, Quaternion rotation) Motion_NoHand_Env(Vector3 positionIn, Quaternion rotationIn)
        {
            #region 函数内函数 → Vector2 BestDirectionInGround(Vector2 direction)
            Vector2 BestDirectionInGround(Vector2 direction)
            {
                //先判断是否为0，然后再判断和8向的夹角
                if (Ludo.Utility.FloatEqual_WithIn0p001(direction.x, 0) && Ludo.Utility.FloatEqual_WithIn0p001(direction.y, 0))
                {
                    return Vector2.zero;
                }

                List<Vector2> threeYNegativeDirections = new List<Vector2>();
                threeYNegativeDirections.Add(new Vector2(0, -1).normalized);
                threeYNegativeDirections.Add(new Vector2(1, -1).normalized);
                threeYNegativeDirections.Add(new Vector2(-1, -1).normalized);
                List<Vector2> threeYPositiveDirections = new List<Vector2>();
                threeYPositiveDirections.Add(new Vector2(0, 1).normalized);
                threeYPositiveDirections.Add(new Vector2(1, 1).normalized);
                threeYPositiveDirections.Add(new Vector2(-1, 1).normalized);

                //剑方向向下更好一些，YNegative有10°的优势
                float smallestAngle = 360;
                Vector2 theAngle = threeYNegativeDirections[0];
                foreach (Vector2 oneDir in threeYNegativeDirections)
                {
                    float angle = Vector2.Angle(direction, oneDir) - 10;
                    if (angle < smallestAngle)
                    {
                        smallestAngle = angle;
                        theAngle = oneDir;
                    }
                }
                foreach (Vector2 oneDir in threeYPositiveDirections)
                {
                    if (Vector2.Angle(direction, oneDir) < smallestAngle)
                    {
                        smallestAngle = Vector2.Angle(direction, oneDir);
                        theAngle = oneDir;
                    }
                }
                return theAngle;
            }
            #endregion 函数内函数

            Vector3 targetPosition = positionIn;
            Quaternion targetRotation = rotationIn;

            //Env状态下的移动
            //首先，相信之前的判断。既然现在是Env，那就一定和环境有接触，不考虑和环境没有接触的情况。
            if (noHand_speed > 0)
            {
                Vector2 movement = nohandEnvMoveDistance * noHand_direction;
                targetPosition = transform.position + (Vector3)movement;

                if (noHand_speed <= noHandEnvSpeedThreshold)
                {
                    noHand_speed = 0f;
                }
                else
                {
                    noHand_speed = noHandEnvSpeedThreshold;
                }

                //移动后，状态可能改变
                LayerMask env = LayerMask.GetMask("EnvGround", "EnvRock", "EnvRoundRock");
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(env);
                Collider2D[] result = new Collider2D[1];
                var tipCollider = tip.GetComponent<Collider2D>();
                var bodyCollider = body.GetComponent<Collider2D>();
                if (Physics2D.OverlapCollider(tipCollider, filter, result) == 0 && Physics2D.OverlapCollider(bodyCollider, filter, result) == 0)
                {
                    states.Set("NoHand", "Fly");
                }
            }

            //NoHandEnv，剑的方向，需要是9向
            if (states.noHandState == NoHandState.Env)
            {
                //如果是普通的Env，朝8向靠拢
                //如果在Ground，不能为水平方向，也就是朝6向靠拢。
                Vector2 direction = (Vector2)(tip.transform.position - body.transform.position).normalized;
                Vector2 properDir = Tool.MostCloseNineDirection(direction);
                {
                    LayerMask env = LayerMask.GetMask("EnvGround");
                    ContactFilter2D filter = new ContactFilter2D();
                    filter.SetLayerMask(env);
                    Collider2D[] result = new Collider2D[1];
                    var tipCollider = tip.GetComponent<Collider2D>();
                    var bodyCollider = body.GetComponent<Collider2D>();
                    if (Physics2D.OverlapCollider(tipCollider, filter, result) > 0 || Physics2D.OverlapCollider(bodyCollider, filter, result) > 0)
                    {
                        properDir = BestDirectionInGround(direction);
                    }
                }

                if (properDir == Vector2.zero)
                {
                    throw new System.Exception("properDir should not be Vector2.zero");
                }

                //如果角度不等，则转向
                if (!FloatEqual_WithIn0p001(Vector2.Angle(direction, properDir), 0))
                {
                    float maxRotateAngle = noHand_stopedAngleSpeed * Time.fixedDeltaTime;
                    if (Vector2.Angle(direction, properDir) <= maxRotateAngle)
                    {
                        Vector3 rotatePoint = tip.transform.position;
                        float angle = Vector2.SignedAngle(direction, properDir);
                        (targetPosition, targetRotation) =
                            Ludo.Utility.RotateRoundPoint2D(targetPosition, targetRotation, rotatePoint, angle);
                        
                    }
                    else
                    {
                        Vector3 rotatePoint = tip.transform.position;
                        float angleSign = Mathf.Sign(Vector2.SignedAngle(direction, properDir));
                        float angle = angleSign * maxRotateAngle;
                        (targetPosition, targetRotation) =
                            Ludo.Utility.RotateRoundPoint2D(targetPosition, targetRotation, rotatePoint, angle);
                    }
                }
            }

            return (targetPosition, targetRotation);

        }

        (Vector3 position, Quaternion rotation) Motion_NoHand_Flying(Vector3 positionIn, Quaternion rotationIn)
        {
            Vector3 targetPosition = positionIn;
            Quaternion targetRotation = rotationIn;

            float speed = noHand_speed;
            Vector2 direction = noHand_direction;
            float a = noHand_swordFly_deceleration;
            float dt = Time.fixedDeltaTime;
            Vector2 movement = speed * direction * dt;

            bool collisionEnv = Motion_NoHand_RayCastEnv(ref movement);
            targetPosition += (Vector3)movement;

            if (collisionEnv)
            {
                states.Set("NoHand", "Env");
            }
            else
            {
                noHand_speed = Mathf.Max(0, speed - a * dt);
                float minSpeed = noHand_swordFly_minSpeed;
                if (noHand_speed <= minSpeed)
                {
                    states.Set("NoHand", "Drop");
                }
            }
            return (targetPosition, targetRotation);
        }

        (Vector3 position, Quaternion rotation) Motion_NoHand_Dropping(Vector3 positionIn, Quaternion rotationIn)
        {
            Vector3 targetPosition = positionIn;
            Quaternion targetRotation = rotationIn;

            float speed = noHand_speed;
            Vector2 direction = noHand_direction;
            float aHorizontal = noHand_swordFly_deceleration;
            float aVertical = noHand_swordDrop_acceleration;
            float dt = Time.fixedDeltaTime;
            Vector2 movement = speed * direction * dt;

            bool collisionEnv = Motion_NoHand_RayCastEnv(ref movement);
            targetPosition += (Vector3)movement;

            if (collisionEnv)
            {
                states.Set("NoHand", "Env");
            }
            else
            {
                /*
                    非下降的部分，减速。
                    下降的部分，加速。
                */
                float rightDot = Vector2.Dot(speed * direction, Vector2.right);
                float upDot = Vector2.Dot(speed * direction, Vector2.up);
                float downMax = noHand_swordDrop_downMax;

                upDot -= aVertical * dt;
                if (upDot < -downMax)
                {
                    upDot = -downMax;
                }

                if (!FloatEqual_WithIn0p001(rightDot, 0))
                {
                    rightDot = Mathf.Sign(rightDot) * Mathf.Max(0, (Mathf.Abs(rightDot) - aHorizontal * dt));
                }
                else
                {
                    rightDot = 0f;
                }

                Vector2 v = rightDot * Vector2.right + upDot * Vector2.up;
                noHand_speed = v.magnitude;
                noHand_direction = v.normalized;
            }
            return (targetPosition, targetRotation);

        }
    }
}

//只是用来保存legacy代码
#if LegacyCode
void Motion_HandAndFree_Rotation(Vector2 handOffset)
{
    //准备工作1，如果上一帧不是HandAndFree，handAndFree_LastNonZeroHandOffset置为零。
    if (!(states.statePre == State.WithHand && states.withHandStatePre == WithHandState.Free))
    {
        handAndFree_LastNonZeroHandOffset = Vector2.zero;
    }

    //准备工作2
    Vector2 swordDirection = tip.transform.position - body.transform.position;
    handOffset = HandControlTool.Tool.ArbitraryDirectionToNineDirection(handOffset);
    if (!FloatEqual_WithIn0p001(handOffset.magnitude, 0))
    {
        handAndFree_LastNonZeroHandOffset = handOffset;
    }
    /*
        1.
        根据手的运动方向，决定旋转方向
            手的运动方向已被转为9向
            如果手的运动方向为0，则使用handAndFree_LastNonZeroHandOffset来计算
            （handAndFree_LastNonZeroHandOffset也可能为0）


        手没有运动
            不旋转
        手运动
            方向平行
                不旋转
            方向相反
                逆时针旋转
            有垂直方向的分量
                根据力矩旋转

        ------------
        我这里好像有点傻。因为想的是力矩，所以就用叉乘，然后有了一系列操作。
        只要用Signed Angle，看handOffset和swordDirection夹角，看是正的还是负的，就可以....
    */
    float speedDirection;
    {
        if (FloatEqual_WithIn0p001(handOffset.magnitude, 0))
        {
            handOffset = handAndFree_LastNonZeroHandOffset;
        }
        if (FloatEqual_WithIn0p001(handOffset.magnitude, 0))
        {
            speedDirection = 0f;
            goto AccelerationDirectionFinish;
        }

        Vector2 HONormalized = handOffset.normalized;
        Vector2 SDNormalized = swordDirection.normalized;
        float crossProduct = SDNormalized.x * HONormalized.y - SDNormalized.y * HONormalized.x;
        /*
            crossProduct是叉积，根据叉积，确定旋转方向。

            Unity中，z为正，是逆时针

            我试了一下，offset为（1,0）,direction为(0,1)，
                应该顺时针旋转(旋转方向为负)
                算出来的crossProduct 为 -1

            所以，根据crossProduct求旋转方向，旋转方向和crossProduct的正负号相同
        */
        if (FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 0))
        {
            speedDirection = 0f;
            goto AccelerationDirectionFinish;
        }
        if (FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 180))
        {
            speedDirection = 1f;
            goto AccelerationDirectionFinish;
        }
        speedDirection = Mathf.Sign(crossProduct);

        AccelerationDirectionFinish:
        ;
    }

    /*
        2.
        速度

        感觉比较更好一点的方法是，类似于移动。加速有个过程，瞬间停止。
        不过，必要性不大，就直接匀速了。
    */
    angleSpeed = 180f / 0.3f;

    /*
        3.
        最终旋转角度
            如果旋转经过了handOffset方向，需要重置方向，等同于handOffset方向。
    */
    if (speedDirection == 0f)
    {
        return;
    }
    Quaternion targetRotation = transform.rotation;
    float deltaAngle = angleSpeed * speedDirection * Time.fixedDeltaTime;
    transform.rotation *= Quaternion.Euler(0, 0, deltaAngle);

    Vector2 swordDirectionNew = tip.transform.position - body.transform.position;


    /*
        什么时候旋转经过handOffset呢？
        想到的方法是，比较(swordDirectionNew与offset的夹角) 和 (swordDirection与offset的夹角)。
        如果夹角方向不同，则说明经过了handOffset。

        但是，有一个特殊情况，之前忘记考虑了。
        如果有一个夹角是180度。那180可以是+180，也可以是-180。就会有问题。
        目前，一帧的旋转不可能超过180度。所以，在单独判一下180度就可以了。
    */
    if (Mathf.Sign(Vector2.SignedAngle(swordDirectionNew, handOffset))
            != Mathf.Sign(Vector2.SignedAngle(swordDirection, handOffset)))
    {
        if (!FloatEqual_WithIn0p001(Vector2.Angle(swordDirectionNew, handOffset), 180)
                && !FloatEqual_WithIn0p001(Vector2.Angle(swordDirection, handOffset), 180))
        {
            transform.rotation *= Quaternion.Euler(0, 0, Vector2.SignedAngle(swordDirectionNew, handOffset));
        }
    }
}
       
#endif