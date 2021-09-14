using DG.Tweening;
using System.Collections;
using UnityEngine;
using Ludo.Extensions;

public partial class Shuriken : MonoBehaviour
{
    /*
        文档：
        https://perfect-dive-450.notion.site/Shuriken-80d1294873f24e908ba5f30d348cc8f3
    */
    public abstract class State
    {
        protected Shuriken shuriken;
        public bool ended = false;

        public State(Shuriken shuriken)
        {
            this.shuriken = shuriken;
        }

        public abstract void StateStart();

        public abstract void StateFixedUpdate();

        public abstract void StateLateFixedUpdate();

        public abstract void StateManuallyEnd();

        public abstract State NextState();

    }

    public class BeforeTriggerFire : State
    {
        //in
        Rigidbody2D rigidbody2D;
        float idleDistance;
        Vector2 direction;
        Vector3 initPosition;

        public BeforeTriggerFire(Shuriken shuriken,
            Vector3 initPosition, float idleDistance, Vector2 direction, Rigidbody2D rigidbody2D)
            : base(shuriken)
        {
            
            this.direction = direction;
            this.idleDistance = idleDistance;
            this.rigidbody2D = rigidbody2D;
            this.initPosition = initPosition;
        }

        public override void StateStart()
        {
            rigidbody2D.simulated = false;
            Vector2 idleDirection = -direction.normalized;
            shuriken.transform.position = initPosition +
                                          (Vector3)(idleDirection * idleDistance);
        }

        public override void StateFixedUpdate()
        {

        }

        public override void StateLateFixedUpdate()
        {

        }

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return null;
        }

        public override string ToString()
        {
            return "Before Tirgger Fire";
        }
    }

    public class BeforeShowState : State
    {
        //in
        Vector3 initPosition;
        float idleTime;
        float flyTime;
        float idleDistance;
        Vector2 direction;
        Rigidbody2D rigidbody2D;

        public BeforeShowState(Shuriken shuriken,
            Vector3 initPosition, float idleTime, float flyTime, float idleDistance, Vector2 direction, Rigidbody2D rigidbody2D)
            : base(shuriken)
        {
            this.initPosition = initPosition;
            this.idleTime = idleTime;
            this.flyTime = flyTime;
            this.direction = direction;
            this.idleDistance = idleDistance;
            this.rigidbody2D = rigidbody2D;

        }

        public override void StateStart()
        {
            IEnumerator StateStartCoroutine()
            {
                void End()
                {
                    ended = true;
                }

                rigidbody2D.simulated = false;

                Vector2 idleDirection = -direction.normalized;
                shuriken.transform.position = initPosition +
                                              (Vector3)(idleDirection * idleDistance);

                yield return new WaitForSeconds(idleTime);

                Tweener tw = DOTween.To(
                    () => shuriken.transform.position,
                    (p) => shuriken.transform.position = p,
                    initPosition,
                    flyTime);
                tw.OnComplete(End);
            }

            shuriken.StartCoroutine(StateStartCoroutine());
        }

        public void StateStartByTrigger()
        {
            void End()
            {
                ended = true;
            }

            rigidbody2D.simulated = false;
            Tweener tw = DOTween.To(
                () => shuriken.transform.position,
                (p) => shuriken.transform.position = p,
                initPosition,
                flyTime);
            tw.OnComplete(End);
        }
        

        public override void StateFixedUpdate()
        {

        }

        public override void StateLateFixedUpdate()
        {

        }

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            float standStillTime = shuriken.prepareStandStillTime;
            float backwardTime = shuriken.prepareBackwardTime;
            float forwardTime = shuriken.prepareForwardTime;
            string colorString = shuriken.prepareColorString;
            Vector2 offsetRaw = shuriken.prepareOffset;
            float angleOffsetRaw = shuriken.prepareAngleOffset;
            Vector2 scaleMultiple = shuriken.prepareScaleMultiple;

            Vector2 direction = shuriken.direction;
            Vector3 frontToBack = shuriken.frontToBack;
            Vector3 bottomToTop = shuriken.bottomToTop;

            Rigidbody2D rigidbody2D = shuriken.rigidbody2D;
            SpriteRenderer sprite = shuriken.sprite;

            float currentAngle = shuriken.currentAngleInitForPrepare;
            return new PrepareFlyState(shuriken,
                standStillTime, backwardTime, forwardTime,
                colorString, offsetRaw, angleOffsetRaw, scaleMultiple,
                direction, frontToBack, bottomToTop,
                rigidbody2D, sprite,
                currentAngleInit: currentAngle);
        }

        public override string ToString()
        {
            return "Before Show";
        }
    }

    public class PrepareFlyState : State
    {
        //In
        float standStillTime;
        float backwardTime;
        float forwardTime;

        string colorString;
        Vector2 offsetRaw;
        float angleOffsetRaw;
        Vector2 scaleMultiple;

        Vector2 direction;
        Vector3 frontToBack;
        Vector3 bottomToTop;

        Rigidbody2D rigidbody2D;
        SpriteRenderer sprite;

        float currentAngle; //In And Runtime

        //runtime
        Color colorInit;
        Vector2 scaleInit;
        Tweener tweenPosition = null;
        Tweener tweenRotation = null;
        Tweener tweenScale = null;
        Tweener tweenColor = null;
        Coroutine coroutine = null;
        //HCAudio2.audioSource

        public PrepareFlyState(Shuriken shuriken,
            float standStillTime, float backwardTime, float forwardTime,
            string colorString, Vector2 offsetRaw, float angleOffsetRaw, Vector2 scaleMultiple,
            Vector2 direction, Vector3 frontToBack, Vector3 bottomToTop,
            Rigidbody2D rigidbody2D, SpriteRenderer sprite,
            float currentAngleInit)
            : base(shuriken)
        {
            this.standStillTime = standStillTime;
            this.backwardTime = backwardTime;
            this.forwardTime = forwardTime;

            this.colorString = colorString;
            this.offsetRaw = offsetRaw;
            this.angleOffsetRaw = angleOffsetRaw;
            this.scaleMultiple = scaleMultiple;

            this.direction = direction;
            this.frontToBack = frontToBack;
            this.bottomToTop = bottomToTop;


            this.rigidbody2D = rigidbody2D;
            this.sprite = sprite;

            this.currentAngle = currentAngleInit;
            this.colorInit = sprite.color;
            this.scaleInit = shuriken.transform.localScale;
        }

        public override void StateStart()
        {
            coroutine = shuriken.StartCoroutine(StateStartCoroutine());
        }

        IEnumerator StateStartCoroutine()
        {
            //PrepareFlyState
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;


            //1. StandStill
            yield return new WaitForSeconds(standStillTime);

            //2. Backward
            /*
                offset的x方向，是frontToBack的方向
                offset的y方向，是bottomToTop的方向
            */
            Vector3 offset = frontToBack.normalized * offsetRaw.x + bottomToTop.normalized * offsetRaw.y;
            /*
                angleOffset的大小是prepareAngleOffset
                方向（正负号），是frontToBack到bottomToTop的角度方向
            */
            float angleOffset = angleOffsetRaw * Mathf.Sign(Vector2.SignedAngle(frontToBack, bottomToTop));
            Color color;
            ColorUtility.TryParseHtmlString(colorString, out color);

            //tweener
            tweenPosition = rigidbody2D.DOMove(shuriken.transform.position + (Vector3)offset, backwardTime);
            tweenRotation = rigidbody2D.DORotate(currentAngle + angleOffset, backwardTime);
            Vector2 ToScale = new Vector2(shuriken.transform.localScale.x * scaleMultiple.x, shuriken.transform.localScale.y * scaleMultiple.y);
            tweenScale = shuriken.transform.DOScale(ToScale, backwardTime);
            tweenColor = DOTween.To(
                () => sprite.color,
                x => sprite.color = x,
                color,
                backwardTime);

            tweenPosition.OnKill(() => tweenPosition = null);
            tweenRotation.OnKill(() => tweenRotation = null);
            tweenScale.OnKill(() => tweenScale = null);
            tweenColor.OnKill(() => tweenColor = null);

            while (true)
            {
                if (tweenPosition == null && tweenRotation == null && tweenScale == null && tweenColor == null)
                {
                    break;
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            currentAngle += angleOffset;

            //3.Forward
            /*
                angleOffset的大小是prepareAngleOffset
                方向（正负号），是frontToBack到bottomToTop的角度方向
            */
            HCAudio2.audioSource.PlayOneShot(HCAudio2.instance.acShootArrow);
            tweenPosition = rigidbody2D.DOMove(shuriken.transform.position - (Vector3)offset, forwardTime).SetEase(Ease.InCirc);
            tweenRotation = rigidbody2D.DORotate(currentAngle - angleOffset, forwardTime).SetEase(Ease.InCirc);
            tweenScale = shuriken.transform.DOScale(scaleInit, forwardTime);
            tweenColor = DOTween.To(
                () => sprite.color,
                x => sprite.color = x,
                colorInit,
                forwardTime);
            tweenPosition.OnKill(() => tweenPosition = null);
            tweenRotation.OnKill(() => tweenRotation = null);
            tweenScale.OnKill(() => tweenScale = null);
            tweenColor.OnKill(() => tweenColor = null);
            while (true)
            {
                if (tweenPosition == null && tweenRotation == null && tweenScale == null && tweenColor == null)
                {
                    break;
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            currentAngle += angleOffset;
            ended = true;
        }

        public override void StateFixedUpdate()
        {

        }

        public override void StateLateFixedUpdate()
        {

        }

        public override void StateManuallyEnd()
        {
            tweenPosition.Pause();
            tweenPosition.Kill();
            tweenRotation.Pause();
            tweenRotation.Kill();
            tweenColor.Pause();
            tweenColor.Kill();
            shuriken.StopCoroutine(coroutine);

            sprite.color = colorInit;
        }

        public override State NextState()
        {
            float flySpeed = shuriken.flySpeed;
            Vector3 direction = shuriken.direction;
            Rigidbody2D rigidbody2D = shuriken.rigidbody2D;
            return new FlyState(shuriken,
                flySpeed: flySpeed,
                direction: direction,
                flyInEnvProtectTime: shuriken.flyInEnvProtectTime,
                flyInEnvDeceleration: shuriken.flyInEnvDeceleration,
                flyinEnvAngularDecelerationFrameCount: shuriken.flyinEnvAngularDecelerationFrameCount,
                rigidbody2D: rigidbody2D);
        }

        public override string ToString()
        {
            return "Prepare Fly";
        }
    }

    public class FlyState : State
    {
        float flySpeed;
        Vector3 direction;
        float flyInEnvProtectTime;
        float flyInEnvDeceleration;
        int flyinEnvAngularDecelerationFrameCount;

        Rigidbody2D rigidbody2D;

        //runtime
        float protectStartTime;
        /*  
            和Env相交的第一帧，这个值为1（而非0）。
            当这个值 大于 flyinEnvAngularDecelerationFrameCount时，就停止旋转。
            也就是说，如果flyinEnvAngularDecelerationFrameCount=2。就有2帧在旋转。
        */
        int frameCountSinceCollideWithEnv;
        Vector2 positionFixedUpdate;
        bool collidingWithEnv;
        bool collidingWithEnvInLateFixedUpdate;

        public FlyState(Shuriken shuriken,
            float flySpeed, Vector2 direction,
            float flyInEnvProtectTime, float flyInEnvDeceleration, int flyinEnvAngularDecelerationFrameCount,
            Rigidbody2D rigidbody2D)
           : base(shuriken)
        {
            this.flySpeed = flySpeed;
            this.direction = direction;
            this.flyInEnvProtectTime = flyInEnvProtectTime;
            this.flyInEnvDeceleration = flyInEnvDeceleration;
            this.flyinEnvAngularDecelerationFrameCount = flyinEnvAngularDecelerationFrameCount;

            this.rigidbody2D = rigidbody2D;
        }

        public override void StateStart()
        {
            //FlyState
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

            rigidbody2D.velocity = direction * flySpeed;
            protectStartTime = Time.fixedTime;
        }


        public void StateStartOnCollision()
        {
            //FlyState
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

            protectStartTime = Time.fixedTime;
        }

        public override void StateFixedUpdate()
        {
            positionFixedUpdate = shuriken.transform.position;

            collidingWithEnv = false;
            if (collidingWithEnvInLateFixedUpdate)
            {
                collidingWithEnv = true;
            }
            else if (Time.fixedTime - protectStartTime > flyInEnvProtectTime)
            {
                var collider2D = shuriken.GetComponent<Collider2D>();
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(LayerMaskExtension.GetMaskInTwoHandsWar("Env"));
                Collider2D[] result = new Collider2D[1];
                collidingWithEnv = Physics2D.OverlapCollider(collider2D, filter, result) > 0;
            }

            if (collidingWithEnv)
            {
                frameCountSinceCollideWithEnv += 1;
            }
            else
            {
                frameCountSinceCollideWithEnv = 0;
            }


            //velocity
            {
                //常规情况，速度控制在flySpeed
                if (rigidbody2D.velocity.magnitude >= flySpeed)
                {
                    rigidbody2D.velocity = rigidbody2D.velocity.normalized * flySpeed;
                }

                //和Env相交时，每帧减速直到为0。
                if (collidingWithEnv)
                {
                    if (rigidbody2D.velocity.magnitude > flyInEnvDeceleration)
                    {
                        rigidbody2D.velocity = rigidbody2D.velocity.normalized * (rigidbody2D.velocity.magnitude - flyInEnvDeceleration);
                    }
                    else
                    {
                        rigidbody2D.velocity = new Vector2(0, 0);
                    }
                }
            }
            //angularVelocity
            {
                //和Env相交时，减速
                if (collidingWithEnv)
                {
                    if (frameCountSinceCollideWithEnv > flyinEnvAngularDecelerationFrameCount)
                    {
                        rigidbody2D.angularVelocity = 0f;
                    }
                    else
                    {
                        rigidbody2D.angularVelocity = 
                            rigidbody2D.angularVelocity
                            * ( (float)(flyinEnvAngularDecelerationFrameCount - frameCountSinceCollideWithEnv + 1)
                                / (float)(flyinEnvAngularDecelerationFrameCount - frameCountSinceCollideWithEnv + 2));
                    }
                   
                }
            }
        }

        public override void StateLateFixedUpdate()
        {
            //LateFixedUpdate，只对不旋转的情况进行处理
            if (rigidbody2D.angularVelocity >= 1f)
            {
                return;
            }

            collidingWithEnvInLateFixedUpdate = false;
            if (Time.fixedTime - protectStartTime > flyInEnvProtectTime)
            {
                var collider2D = shuriken.GetComponent<Collider2D>();
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(LayerMaskExtension.GetMaskInTwoHandsWar("Env"));
                Collider2D[] result = new Collider2D[1];

                collidingWithEnvInLateFixedUpdate = Physics2D.OverlapCollider(collider2D, filter, result) > 0;
            }

            /*
                在FixedUpdate无碰撞，在LateFixedUpdate有碰撞：说明这一帧的移动产生碰撞。
                对位置进行处理。
            */
            if (collidingWithEnvInLateFixedUpdate && !collidingWithEnv)
            {
                BoxCollider2D collider = shuriken.GetComponent<BoxCollider2D>();

                Vector2 offset = collider.offset * shuriken.transform.localScale;
                offset = shuriken.transform.rotation * (Vector3)offset;
                Vector2 position = (Vector2)positionFixedUpdate + offset;
                Vector2 size = collider.size * shuriken.transform.localScale;
                size.x = Mathf.Abs(size.x);
                size.y = Mathf.Abs(size.y);
                float angle = QuaternionExtension.SignedAngle2D(Quaternion.identity, shuriken.transform.rotation);
                float distance = rigidbody2D.velocity.magnitude * Time.fixedDeltaTime;
                int layerMask = LayerMaskExtension.GetMaskInTwoHandsWar("Env");

                RaycastHit2D hit = Physics2D.BoxCast(position, size, angle, direction, distance, layerMask);
                if (hit.collider)
                {
                    shuriken.transform.position = (Vector3)hit.centroid - (Vector3)offset;
                    TransformSync.AddNeedSyncYieldReturnFixedUpdate();
                }

            }


            
        }

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return null;
        }

        public override string ToString()
        {
            return "Fly";
        }

        public int ForEditor_GetFrameCountSinceEnv()
        {
            return frameCountSinceCollideWithEnv;
        }

    }

    public class KillPlayerState : State
    {
        int goOnFlyingFrameCount;
        Rigidbody2D rigidbody2D;

        //runtime
        int fixedUpdateCount = 0;

        public KillPlayerState(Shuriken shuriken,
           int goOnFlyingFrameCount, Rigidbody2D rigidbody2D)
           : base(shuriken)
        {
            this.rigidbody2D = rigidbody2D;
            this.goOnFlyingFrameCount = goOnFlyingFrameCount;
        }

        public override void StateStart()
        {
            //KillPlayerState
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        }

        public void StateStartOnKillPlayer()
        {
            //KillPlayerState
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        }

        public override void StateFixedUpdate()
        {
            fixedUpdateCount++;

            if (fixedUpdateCount <= goOnFlyingFrameCount)
            {
                //什么都不做，于是继续飞
            }
            else
            {
                ended = true;
            }

        }

        public override void StateLateFixedUpdate()
        {

        }

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return new EndState(shuriken, rigidbody2D);
        }

        public override string ToString()
        {
            return "Kill";
        }
    }

    public class EndState : State
    {
        Rigidbody2D rigidbody2D;

        public EndState(Shuriken shuriken, Rigidbody2D rigidbody2D)
           : base(shuriken)
        {
            this.rigidbody2D = rigidbody2D;
        }

        public override void StateStart()
        {
            rigidbody2D.bodyType = RigidbodyType2D.Static;
        }

        public override void StateFixedUpdate()
        {

        }

        public override void StateLateFixedUpdate()
        {

        }

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return null;
        }

        public override string ToString()
        {
            return "End";
        }
    }
}