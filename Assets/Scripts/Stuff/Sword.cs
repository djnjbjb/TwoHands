using Ludo.Extensions;
using System.Collections.Generic;
using UnityEngine;
using static Ludo.Utility;


namespace Stuff
{
    public class Sword : Stuff
    {
        public enum State
        {
            NoHand = 0,
            HandAndFree = 1,
            HandAndEnv = 2
        }

        public enum NoHandState
        {
            Flying = 0,
            Dropping = 1,
            Env = 2
        }

        public enum WithHandState
        {

        }

        class StatePlus
        {
            public State state { get; private set; }
            public State pre { get; private set; }

            public static implicit operator State(StatePlus stateIn) => stateIn.state;
            public static implicit operator StatePlus(State stateIn) => new StatePlus(stateIn);

            public StatePlus(State stateIn)
            {
                state = stateIn;
                pre = stateIn;
            }

            public void FixedUpdateManually(string lastFist, GameObject tip, GameObject body)
            {
                /*
                    没有手的时候，一定是NoHand
                    
                    有手时，需根据pre判断。
                        如果pre是 HandAndFree。        那现在是HandAndFree
                        如果pre是 HandAndEnv 或 NoHand。那就就要判断有无collider。
                 */

                if (lastFist == null)
                {
                    pre = state;
                    state = State.NoHand;
                    return;
                }

                if (pre == State.HandAndFree)
                {
                    pre = state;
                    state = State.HandAndFree;
                    return;
                }
                else
                {
                    LayerMask env = LayerMask.GetMask("EnvGround", "EnvRock", "EnvRoundRock");
                    ContactFilter2D filter = new ContactFilter2D();
                    filter.SetLayerMask(env);
                    Collider2D[] result = new Collider2D[1];

                    var tipCollider = tip.GetComponent<Collider2D>();
                    var bodyCollider = body.GetComponent<Collider2D>();
                    if (Physics2D.OverlapCollider(tipCollider, filter, result) > 0 || Physics2D.OverlapCollider(bodyCollider, filter, result) > 0)
                    {
                        pre = state;
                        state = State.HandAndEnv;
                        return;
                    }
                    else
                    {
                        pre = state;
                        state = State.HandAndFree;
                        return;
                    }
                }
            }
        }

        class NoHandMotion
        {
            public float speed;
            public Vector2 direction;
            public NoHandState state;
        }

        float angleSpeed = 0f;
        float toHolderPositionSpeed = 0.87f / 0.3f;
        GameObject tip;
        Collider2D tipEnvCollider;
        GameObject tipEnvVisual;
        GameObject body;
        Transform rayPoint1;
        Transform rayPoint2;
        Transform rayPoint3;
        Transform centralPoint;
        List<Collider2D> bodyEnvColliderList;
        List<GameObject> bodyEnvVisualList;
        GameObject gem;
        GameObject holdPosition;
        Vector3 startingPosition;
        Quaternion startingRotation;

        StatePlus state;
        Vector2 handAndFree_LastNonZeroHandOffset;
        NoHandMotion noHandMotion;
        const float noHand_stopedAngleSpeed = 22.5f / 0.3f;
        const float noHand_swordFly_fistToSwordSpeedRatio = 22f / 6.8f;
        const float noHand_swordFly_minSpeed = 12f;
        const float noHand_swordFly_deceleration = 0f;
        const float noHand_swordDrop_downMax = 22f;
        const float noHand_swordDrop_acceleration = 30f;
        const float noHandEnvSpeedThreshold = 20f;
        const float nohandEnvMoveDistance = 0.2f;

        public override void AddAssociatedFist(string fist)
        {
            if (fist != "LeftFist" && fist != "RightFist")
                throw new System.Exception("fist should be LeftFist or RightFist");
            foreach (var fistElement in fists)
            {
                if (fistElement == fist)
                {
                    throw new System.Exception("fist already exist");
                }
            }
            fists.Add(fist);
        }
        public override void RemoveAssociatedFist(string fist)
        {
            if (fist != "LeftFist" && fist != "RightFist")
                throw new System.Exception("fist should be LeftFist or RightFist");
            int fistCount = -1;
            for (int i = 0; i < fists.Count; i++)
            {
                if (fists[i] == fist)
                {
                    fistCount = i;
                }
            }
            if (fistCount == -1)
            {
                throw new System.Exception("fist not exist");
            }
            fists.RemoveAt(fistCount);
        }

        new void Start()
        {
            base.Start();
            tip = transform.Find("Tip").gameObject;
            tipEnvCollider = tip.transform.LudoFind("Collider1", includeInactive: true).gameObject.GetComponent<Collider2D>();
            tipEnvVisual = tip.transform.LudoFind("Visual1", includeInactive: true).gameObject;
            body = transform.Find("Body").gameObject;
            bodyEnvColliderList = new List<Collider2D>();
            for (int i = 1; i <= 5; i++)
            {
                bodyEnvColliderList.Add(body.transform.LudoFind("Collider" + i.ToString(), includeInactive: true).gameObject.GetComponent<Collider2D>());
            }
            bodyEnvVisualList = new List<GameObject>();
            for (int i = 1; i <= 5; i++)
            {
                bodyEnvVisualList.Add(body.transform.LudoFind("Visual" + i.ToString(), includeInactive: true).gameObject);
            }
            gem = transform.LudoFind("Gem", includeInactive: true).gameObject;
            holdPosition = transform.Find("HoldPosition").gameObject;
            rayPoint1 = transform.Find("RayCastPoint1");
            rayPoint2 = transform.Find("RayCastPoint2");
            rayPoint3 = transform.Find("RayCastPoint3");
            centralPoint = transform.Find("CentralPoint");
            startingPosition = this.transform.position;
            startingRotation = this.transform.rotation;

            //
            state = new StatePlus(State.NoHand);
            noHandMotion = new NoHandMotion();
            noHandMotion.speed = 0f;
            noHandMotion.direction = Vector2.zero;
            noHandMotion.state = NoHandState.Env;

        }

        void FixedUpdate()
        {
            StateFixedUpdate();
            Visual();
            Motion();
            CheckAndRemoveFist();
        }

        void StateFixedUpdate()
        {
            state.FixedUpdateManually(firstFist, tip, body);
        }

        void Visual()
        {
            /*
                为3个东西设置Visiual
                
                1
                根据是否有手握着，更改显示图层
                    有手，High
                    没手，Env
                2
                根据手是否完全握着，更改Gem的显示
                
                3
                根据手是否完全握着，是否有交叠，更改部分的显示
            */

            //1. 根据是否有手握着，更改显示图层
            if (state == State.HandAndFree || state == State.HandAndEnv)
            {
                var sprites = gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
                foreach (var sprite in sprites)
                {
                    sprite.sortingLayerName = "High";
                }
            }
            else
            {
                var sprites = gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
                foreach (var sprite in sprites)
                {
                    sprite.sortingLayerName = "Env";
                }
            }

            //2. 根据手是否完全握着，更改Gem的显示
            if (state == State.HandAndFree)
            {
                gem.SetActive(true);
            }
            else
            {
                gem.SetActive(false);
            }


            //3. 根据手是否完全握着，是否有交叠，更改部分的显示
            if (state != State.HandAndFree)
            {
                LayerMask env = LayerMask.GetMask("EnvGround", "EnvRock", "EnvRoundRock");
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(env);
                Collider2D[] result = new Collider2D[1];

                if (Physics2D.OverlapCollider(tipEnvCollider, filter, result) > 0)
                {
                    tipEnvVisual.SetActive(true);
                }
                else
                {
                    tipEnvVisual.SetActive(false);
                }

                for (int i = 0; i < bodyEnvColliderList.Count; i++)
                {

                    var bodyEnvCollider = bodyEnvColliderList[i];
                    var bodyEnvVisual = bodyEnvVisualList[i];
                    if (Physics2D.OverlapCollider(bodyEnvCollider, filter, result) > 0)
                    {
                        bodyEnvVisual.SetActive(true);
                    }
                    else
                    {
                        bodyEnvVisual.SetActive(false);
                    }
                }
            }
            else
            {
                tipEnvVisual.SetActive(false);
                foreach (var bodyEnvVisual in bodyEnvVisualList)
                {

                    bodyEnvVisual.SetActive(false);
                }
            }
        }

        void Motion()
        {
            //
            if (state == State.HandAndFree || state == State.HandAndEnv)
            {
                /*
                    位移，使用wholeOffset+handOffset
                    旋转，则只使用handOffset
                */
                Vector2 handOffset = new Vector2();
                Vector2 positionOffset = new Vector2();
                GameObject fistObject = null;
                {
                    if (firstFist != null)
                    {
                        HandControl.ValueForSword value = HandControl.handControl.ForSword_GiveValue(firstFist);
                        handOffset = value.handOffset;
                        positionOffset = value.handOffset + value.wholeOffset;
                        fistObject = value.fistObject;
                    }
                }

                if (state == State.HandAndFree)
                {
                    gameObject.transform.position += (Vector3)positionOffset;
                    if (holdPosition.transform.position != fistObject.transform.position)
                    {
                        Vector2 delta = (Vector2)(fistObject.transform.position - holdPosition.transform.position);
                        float distance = delta.magnitude;
                        float maxMove = toHolderPositionSpeed * Time.fixedDeltaTime;
                        if (maxMove > distance)
                        {
                            gameObject.transform.position += (Vector3)delta;
                        }
                        else
                        {
                            gameObject.transform.position += (Vector3)(delta.normalized * maxMove);
                        }
                    }

                    Motion_HandAndFree_SwordRotate(handOffset);
                }

                if (state == State.HandAndEnv)
                {
                    Motion_HandAndEnv(positionOffset);
                }

            }
            if (state == State.NoHand)
            {
                Motion_NoHand();
            }
        }

        void Motion_HandAndFree_SwordRotate(Vector2 handOffset)
        {
            Ludo.LogFile.LogTemp($"raw handOffset: ({handOffset.x}, {handOffset.y})");
            //准备工作1，如果上一帧不是HandAndFree，handAndFree_LastNonZeroHandOffset置为零。
            if (state.pre != State.HandAndFree)
            {
                handAndFree_LastNonZeroHandOffset = Vector2.zero;
            }

            //准备工作2
            Vector2 swordDirection = tip.transform.position - body.transform.position;
            handOffset = HandControlTool.Tool.ArbitraryDirectionToNineDirection(handOffset);
            Ludo.LogFile.LogTemp($"handOffset in Nine: ({handOffset.x}, {handOffset.y})");
            if (!FloatEqual_WithIn0p001(handOffset.magnitude, 0))
            {
                handAndFree_LastNonZeroHandOffset = handOffset;
            }

            Ludo.LogFile.LogTemp($"LastNonZeroHandOffset: ({handAndFree_LastNonZeroHandOffset.x}, {handAndFree_LastNonZeroHandOffset.y})");
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

            Ludo.LogFile.LogTemp($"handOffset: ({handOffset.x}, {handOffset.y})");

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
            float deltaAngle = angleSpeed * speedDirection * Time.fixedDeltaTime;
            transform.rotation *= Quaternion.Euler(0, 0, deltaAngle);

            Vector2 swordDirectionNew = tip.transform.position - body.transform.position;

            /*
                什么时候旋转经过handOffset呢？
                想到的方法是，比较 swordDirectionNew 和 swordDirection 与offset的夹角。
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

        void Motion_HandAndEnv(Vector2 positionOffset)
        {
            Vector2 swordDirection = tip.transform.position - body.transform.position;
            Vector2 HONormalized = positionOffset.normalized;
            Vector2 SDNormalized = swordDirection.normalized;
            if (FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 0) ||
                FloatEqual_WithIn0p001(Vector2.Angle(HONormalized, SDNormalized), 180))
            {
                gameObject.transform.position += (Vector3)positionOffset;
            }
        }

        void Motion_NoHand()
        {
            if (noHandMotion.state == NoHandState.Env)
            {
                Motion_NoHand_Env();
            }

            if (noHandMotion.state == NoHandState.Flying)
            {
                Motion_NoHand_OutOfCameraRegion();
                Motion_NoHand_AdjustDirectionRotateByCentral();
                Motion_NoHand_Flying();
            }

            if (noHandMotion.state == NoHandState.Dropping)
            {
                Motion_NoHand_OutOfCameraRegion();
                Motion_NoHand_AdjustDirectionRotateByCentral();
                Motion_NoHand_Dropping();
            }
        }

        void Motion_NoHand_AdjustDirectionRotateByCentral()
        {
            Motion_NoHand_AdjustDirection("Central");
        }

        void Motion_NoHand_AdjustDirectionRotateByHoldPosition()
        {
            Motion_NoHand_AdjustDirection("HoldPosition");
        }

        void Motion_NoHand_AdjustDirection(string rotateBy)
        {
            /*
                swordDirection不可能为零。
                motionDirection可能为零。
                motionDirection为零时，不转向。
            */

            Vector2 rotatePoint = centralPoint.position;
            if (rotateBy == "Central")
            {
                rotatePoint = centralPoint.position;
            }
            else if (rotateBy == "HoldPosition")
            {
                rotatePoint = holdPosition.transform.position;
            }
            else
            {
                throw new System.Exception("rotateBy should be one of {Central, HoldPosition}");
            }

            Vector2 swordDirection = (Vector2)(tip.transform.position - body.transform.position).normalized;
            Vector2 motionDirection = noHandMotion.direction;
            if (!FloatEqual_WithIn0p001(Vector2.Angle(swordDirection, motionDirection), 0))
            {
                float angle = Vector2.SignedAngle(swordDirection, motionDirection);
                transform.RotateAround(rotatePoint, new Vector3(0, 0, 1), angle);
            }
        }

        void Motion_NoHand_OutOfCameraRegion()
        {
            //左右上 - 反弹，下 - 不反弹

            Camera camera = CameraFollow.instance.sceneCamera;
            float ratio = CameraFollow.screenRatio;
            float camera_half_height = camera.orthographicSize;
            float camera_half_width = camera_half_height * ratio;
            HandControlTool.Tool.AABB region = new HandControlTool.Tool.AABB();
            region.left = camera.transform.position.x - camera_half_width;
            region.right = camera.transform.position.x + camera_half_width;
            region.bottom = camera.transform.position.y - camera_half_height;
            region.top = camera.transform.position.y + camera_half_height;

            if (tip.transform.position.x < region.left)
            {
                if (noHandMotion.direction.x < 0)
                {
                    noHandMotion.direction.x = -noHandMotion.direction.x;
                }
            }

            if (tip.transform.position.x > region.right)
            {
                if (noHandMotion.direction.x > 0)
                {
                    noHandMotion.direction.x = -noHandMotion.direction.x;
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
                if (noHandMotion.direction.y > 0)
                {
                    noHandMotion.direction.y = -noHandMotion.direction.y;
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

        void Motion_NoHand_Env()
        {
            //Env状态下的移动
            {
                //首先，相信之前的判断。既然现在是Env，那就一定和环境有接触，不考虑和环境没有接触的情况。
                if (noHandMotion.speed > 0)
                {
                    Vector2 movement = nohandEnvMoveDistance * noHandMotion.direction;
                    this.transform.position += (Vector3)movement;

                    if (noHandMotion.speed <= noHandEnvSpeedThreshold)
                    {
                        noHandMotion.speed = 0f;
                    }
                    else
                    {
                        noHandMotion.speed = noHandEnvSpeedThreshold;
                    }
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
                    noHandMotion.state = NoHandState.Flying;
                }
            }

            //NoHandEnv，剑的方向，需要是9向
            if (noHandMotion.state == NoHandState.Env)
            {
                //如果是普通的Env，朝8向靠拢
                //如果在Ground，不能为水平方向，也就是朝6向靠拢。
                Vector2 direction = (Vector2)(tip.transform.position - body.transform.position).normalized;
                Vector2 properDir = HandControlTool.Tool.MostCloseNineDirection(direction);
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
                        transform.RotateAround(tip.transform.position, new Vector3(0, 0, 1), Vector2.SignedAngle(direction, properDir));
                    }
                    else
                    {
                        float angleSign = Mathf.Sign(Vector2.SignedAngle(direction, properDir));
                        float angle = angleSign * maxRotateAngle;
                        transform.RotateAround(tip.transform.position, new Vector3(0, 0, 1), angle);
                    }
                }
            }

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

        }

        void Motion_NoHand_Flying()
        {
            float speed = noHandMotion.speed;
            Vector2 direction = noHandMotion.direction;
            float a = noHand_swordFly_deceleration;
            float dt = Time.fixedDeltaTime;
            Vector2 movement = speed * direction * dt;

            bool collisionEnv = Motion_NoHand_RayCastEnv(ref movement);
            this.transform.position += (Vector3)movement;

            if (collisionEnv)
            {
                noHandMotion.state = NoHandState.Env;
            }
            else
            {
                noHandMotion.speed = Mathf.Max(0, speed - a * dt);
                float minSpeed = noHand_swordFly_minSpeed;
                if (noHandMotion.speed <= minSpeed)
                {
                    noHandMotion.state = NoHandState.Dropping;
                }
            }

        }

        void Motion_NoHand_Dropping()
        {
            float speed = noHandMotion.speed;
            Vector2 direction = noHandMotion.direction;
            float aHorizontal = noHand_swordFly_deceleration;
            float aVertical = noHand_swordDrop_acceleration;
            float dt = Time.fixedDeltaTime;
            Vector2 movement = speed * direction * dt;

            bool collisionEnv = Motion_NoHand_RayCastEnv(ref movement);
            this.transform.position += (Vector3)movement;

            if (collisionEnv)
            {
                noHandMotion.state = NoHandState.Env;
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
                noHandMotion.speed = v.magnitude;
                noHandMotion.direction = v.normalized;
            }


        }

        void CheckAndRemoveFist()
        {
            //检测非lastFist的手，看是否碰撞，不碰撞，就删掉
            List<string> deleteList = new List<string>();
            foreach (string fist in fists)
            {
                if (fist != firstFist)
                {
                    if (!HandControl.handControl.ForSword_CheckOverlap(fist, this.gameObject))
                    {
                        deleteList.Add(fist);
                    }
                }
            }
            foreach (string deleteFist in deleteList)
            {
                fists.Remove(deleteFist);
                HandControl.handControl.ForSword_UnSetGrabedStuff(deleteFist);
            }
        }

        public void ThrowSword(float speed, HandControlTool.DirectionOf9History offsetHistoryOfNineDir)
        {
            state = State.NoHand;
            /*
                先判断有误Collier
                由此决定State
            */
            LayerMask env = LayerMask.GetMask("EnvGround", "EnvRock", "EnvRoundRock");
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(env);
            Collider2D[] result = new Collider2D[1];
            var tipCollider = tip.GetComponent<Collider2D>();
            var bodyCollider = body.GetComponent<Collider2D>();
            if (Physics2D.OverlapCollider(tipCollider, filter, result) == 0 || Physics2D.OverlapCollider(bodyCollider, filter, result) == 0)
            {
                noHandMotion.state = NoHandState.Flying;
            }
            else
            {
                noHandMotion.state = NoHandState.Env;
            }

            if (noHandMotion.state == NoHandState.Flying)
            {
                /*
                    需要速度和方向
                    速度来自于FistSpeed即可。好像不用考虑历史。
                    方向来自于FistOffset。需要考虑历史。
                */
                //speed
                noHandMotion.speed = speed * noHand_swordFly_fistToSwordSpeedRatio;

                //direction
                Vector2 direction;
                HandControlTool.DirectionOf9History.Type type;
                offsetHistoryOfNineDir.GetDirectionOfNineFromHistory(out direction, out type);
                noHandMotion.direction = direction;

                //特殊情况
                if (direction == Vector2.zero)
                {
                    noHandMotion.speed = 0f;
                }

                //剑的方向和速度方向一致
                Motion_NoHand_AdjustDirectionRotateByHoldPosition();
            }
        }


        public class ValueForFist
        {
            public bool overlapWithEnv;
            public Vector2 direction;
        }
        public ValueForFist GetValueForFist_AtFUPre()
        {
            ValueForFist value = new ValueForFist();

            LayerMask env = LayerMask.GetMask("EnvGround", "EnvRock", "EnvRoundRock");
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(env);
            Collider2D[] result = new Collider2D[1];
            var tipCollider = tip.GetComponent<Collider2D>();
            var bodyCollider = body.GetComponent<Collider2D>();
            value.overlapWithEnv = (Physics2D.OverlapCollider(tipCollider, filter, result) > 0
                                    || Physics2D.OverlapCollider(bodyCollider, filter, result) > 0);

            value.direction = (Vector2)(tip.transform.position - body.transform.position);

            return value;
        }
    }//class Sword
}

//只是用来保存legacy代码
#if NoCondtionCanThisBePossible
        
        //由于手的移动很快，阻尼运动不可行，所以这段就不用了
        //会改成只是让剑运动到和手同样的角度
        const float angleSpeedMax = 720f;
        const float angleAcceleration = 3600f;
        const float dampRatio = 5;
        const float angleCut = 6f;
        const float speedCut = 500f;
        private void SwordRotate(Vector2 handOffset, Vector2 swordDirection)
        {
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
            float accelerationDirection = Mathf.Sign(crossProduct);
            if (Ludo.Utility.FloatEqual0p001(crossProduct, 0)) 
                accelerationDirection = 0f;
            float angleSpeedOld = angleSpeed;


            //力矩变速
            angleSpeed += accelerationDirection * angleAcceleration * Time.fixedDeltaTime;
            //阻尼变速
            float dampAcce = angleSpeedOld * dampRatio * -1f;
            angleSpeed += dampAcce * Time.fixedDeltaTime;
            //速度Clamp
            angleSpeed = Mathf.Clamp(angleSpeed, -1 * angleSpeedMax, angleSpeedMax);
            //设置位置
            transform.rotation *= Quaternion.Euler(0, 0, angleSpeed * Time.fixedDeltaTime);

            //如果位置在中间，且速度比较小，停止震动，让物体停在中间
            Vector2 directionNew = tip.transform.position - body.transform.position;
            float angleNew = Vector2.Angle(handOffset, directionNew);
            if (Mathf.Abs(angleSpeed) <= speedCut && angleNew <= angleCut)
            {
                transform.rotation *= Quaternion.Euler(0, 0, Vector2.SignedAngle(directionNew, handOffset));
                angleSpeed = 0f;
            }
        }
#endif


