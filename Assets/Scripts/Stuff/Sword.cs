using System.Collections.Generic;
using UnityEngine;
using static Ludo.Utility;
using Ludo.Extensions;


namespace Stuff
{

    public class Sword : Stuff
    {
        enum State
        {
            NoHand = 0,
            HandAndFree = 1,
            HandAndEnv = 2
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

            public void FixedUpdateManually(GameObject fist, GameObject tip, GameObject body)
            {
                /*
                    没有手的时候，一定是NoHand
                    
                    有手时，需根据pre判断。
                        如果pre是 HandAndFree，那现在也是HandAndFree
                        如果pre 是 HandAndEnv 或 NoHand。那就就要判断有无collid。
                 */

                if (fist == null)
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

        float angleSpeed = 0f;
        float toHolderPositionSpeed = 0.87f / 0.3f;
        GameObject tip;
        Collider2D tipEnvCollider;
        GameObject tipEnvVisual;
        GameObject body;
        List<Collider2D> bodyEnvColliderList;
        List<GameObject> bodyEnvVisualList;
        GameObject gem;
        GameObject holdPosition;
        StatePlus state;

        void Start()
        {
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
            state = new StatePlus(State.NoHand);
        }

        void FixedUpdate()
        {
            state.FixedUpdateManually(fist, tip, body);
            Y.DebugPanel.Log("SwordState", "Sword", (State)state);
            Visual();
            Motion();
        }

        void Visual()
        {
            /*
                根据是否有手握着，更改显示图层
                    有手，High
                    没手，Env
                
                根据手是否完全握着，更改Gem的显示

                根据手是否完全握着，是否有交叠，更改部分的显示
            */
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

            if (state == State.HandAndFree)
            {
                gem.SetActive(true);
            }
            else
            {
                gem.SetActive(false);
            }

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
            /*剑的位移，使用wholeOffset+handOffset，旋转，则只使用handOffset*/
            HandControl.ValueForSword value = HandControl.handControl.GetValueForSword(fist);
            Vector2 handOffset_ = value.handOffset;
            Vector2 positionOffset_ = value.handOffset + value.wholeOffset;
            if (state == State.HandAndFree)
            {
                gameObject.transform.position += (Vector3)positionOffset_;
                if (holdPosition.transform.position != fist.transform.position)
                {
                    Vector2 delta = (Vector2)(fist.transform.position - holdPosition.transform.position);
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

                HandAndFree_SwordRotate(handOffset_);
            }
            if (state == State.HandAndEnv)
            {
                //需要处理有一定夹角的情况
                HandAndEnv(positionOffset_);
            }
            if (state == State.NoHand)
            {

            }

            //----------------------//

            void HandAndFree_SwordRotate(Vector2 handOffset)
            {
                Vector2 swordDirection = tip.transform.position - body.transform.position;
                /*
                    1.
                    根据手的运动方向，决定旋转方向

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
                float speedDirection = 0f;
                {
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
                    不过这里，我就直接匀速了。
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
                    if ( !FloatEqual_WithIn0p001(Vector2.Angle(swordDirectionNew, handOffset), 180)
                         && !FloatEqual_WithIn0p001(Vector2.Angle(swordDirection, handOffset), 180) )
                    {
                        transform.rotation *= Quaternion.Euler(0, 0, Vector2.SignedAngle(swordDirectionNew, handOffset));
                    }
                    
                }

            }

            void HandAndEnv(Vector2 positionOffset)
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
        }//void Motion()
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


