using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Ludo.Extensions;
using Ludo.Types;
using Ludo.TwoHandsWar.Circumstance;

public partial class ShurikenExplode : MonoBehaviour
{
    public abstract class State
    {
        protected ShurikenExplode shuriken;
        public bool ended = false;

        public State(ShurikenExplode shuriken)
        {
            this.shuriken = shuriken;
        }

        public abstract void StateStart();

        public abstract void StateManuallyEnd();

        public abstract State NextState();

        public override string ToString()
        {
            return SeperateWords(this.GetType().Name);
        }

        /// <summary>
        /// 把string中的单词，区分开：
        ///     1. 把大写字母开头的一段看作一个单词,
        ///     2. 在单词前加空格,
        ///     3. 如果第一个字母是大写字母，不在第一个字母前加空格。
        /// TIP：
        ///     只适用于alphanumeric。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string SeperateWords(string str)
        {
            List<string> l = new List<string>();

            int start = 0;
            int i = 0;
            for (i = 0; i < str.Length; i++)
            {
                char a = str[i];
                if (i != start)
                {
                    if (Char.IsUpper(a))
                    {
                        l.Add(str.Substring(start, i - start));
                        start = i;
                    }
                }
            }
            l.Add(str.Substring(start, str.Length - start));

            return string.Join(" ", l);
        }

    }

    public interface IStateFixedUpdate
    {
        void StateFixedUpdate();
    }

    public interface ILateFixedUpdate
    {
        void StateLateFixedUpdate();
    }

    public interface IOutsideCheck
    {
        bool OutsideCheck();
    }

    public class BeforeTriggerFire : State
    {
        //in
        Rigidbody2D rigidbody2D;
        float idleDistance;
        Vector2 direction;
        Vector3 initPosition;

        public BeforeTriggerFire(ShurikenExplode shuriken,
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

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return null;
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

        //runtime
        Tweener tw = null;

        public BeforeShowState(ShurikenExplode shuriken,
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
                    tw = null;
                    ended = true;
                }

                rigidbody2D.simulated = false;

                Vector2 idleDirection = -direction.normalized;
                shuriken.transform.position = initPosition +
                                              (Vector3)(idleDirection * idleDistance);

                yield return new WaitForSeconds(idleTime);

                tw = DOTween.To(
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
                tw = null;
                ended = true;
            }

            rigidbody2D.simulated = false;
            tw = DOTween.To(
                () => shuriken.transform.position,
                (p) => shuriken.transform.position = p,
                initPosition,
                flyTime);
            tw.OnComplete(End);
        }

        public override void StateManuallyEnd()
        {
            if (tw != null)
            {
                tw.Pause();
                tw.Kill();
            }
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
            AudioClipPlus audioClip = shuriken.acStart;

            float currentAngle = shuriken.currentAngleInitForPrepare;
            return new PrepareFlyState(shuriken,
                standStillTime, backwardTime, forwardTime,
                colorString, offsetRaw, angleOffsetRaw, scaleMultiple,
                direction, frontToBack, bottomToTop,
                rigidbody2D, sprite, audioClip,
                currentAngleInit: currentAngle);
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
        AudioClipPlus audioClip;

        float currentAngle; //In And Runtime

        //runtime
        Color colorInit;
        Vector2 scaleInit;
        Tweener tweenPosition = null;
        Tweener tweenRotation = null;
        Tweener tweenScale = null;
        Tweener tweenColor = null;
        Coroutine coroutine = null;
        

        public PrepareFlyState(ShurikenExplode shuriken,
            float standStillTime, float backwardTime, float forwardTime,
            string colorString, Vector2 offsetRaw, float angleOffsetRaw, Vector2 scaleMultiple,
            Vector2 direction, Vector3 frontToBack, Vector3 bottomToTop,
            Rigidbody2D rigidbody2D, SpriteRenderer sprite, AudioClipPlus audioClip,
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
            this.audioClip = audioClip;

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
            AudioPool.PlayClip(audioClip);
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

        public override void StateManuallyEnd()
        {
            tweenPosition.Pause();
            tweenPosition.Kill();
            tweenRotation.Pause();
            tweenRotation.Kill();
            tweenScale.Pause();
            tweenScale.Kill();
            tweenColor.Pause();
            tweenColor.Kill();
            shuriken.StopCoroutine(coroutine);

            shuriken.transform.localScale = scaleInit;
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
                rigidbody2D: rigidbody2D);
        }
    }

    public class FlyState : State, IStateFixedUpdate, IOutsideCheck
    {
        float flySpeed;
        Vector3 direction;
        Rigidbody2D rigidbody2D;

        public FlyState(ShurikenExplode shuriken,
            float flySpeed, Vector2 direction,
            Rigidbody2D rigidbody2D)
           : base(shuriken)
        {
            this.flySpeed = flySpeed;
            this.direction = direction;
            this.rigidbody2D = rigidbody2D;
        }

        public override void StateStart()
        {
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            rigidbody2D.velocity = direction * flySpeed;
        }


        public void StateStartOnCollision()
        {
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            
            if (rigidbody2D.angularVelocity <= 360f)
            {
                float sign = Mathf.Sign(rigidbody2D.angularVelocity);
                if (Mathf.Abs(rigidbody2D.angularVelocity) <= 1f)
                {
                    sign = (UnityEngine.Random.Range(0, 2) > 0) ? 1f : -1f;
                }
                rigidbody2D.angularVelocity = (180f * UnityEngine.Random.Range(2, 5)) * sign;
            }
            
        }

        void IStateFixedUpdate.StateFixedUpdate()
        {
            //常规情况，速度控制在flySpeed
            if (rigidbody2D.velocity.magnitude >= flySpeed)
            {
                rigidbody2D.velocity = rigidbody2D.velocity.normalized * flySpeed;
            }

            bool collidingWithEnv;
            {
                var collider2D = shuriken.GetComponent<Collider2D>();
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(LayerMaskExtension.GetMaskInTwoHandsWar("Env"));
                Collider2D[] result = new Collider2D[1];
                collidingWithEnv = Physics2D.OverlapCollider(collider2D, filter, result) > 0;
            }

            if (collidingWithEnv)
            {
                ended = true;
            }
        }

        bool IOutsideCheck.OutsideCheck()
        {
            return !(shuriken.bounds.Intersects(LevelManager.onlyInstance.bounds));
        }
        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return new ExplodeState(shuriken, shuriken.rigidbody2D, shuriken.explodeParticle, shuriken.acCollideWall);
        }

    }

    public class ExplodeState: State
    {
        Rigidbody2D rigidbody2D;
        GameObject particle;
        AudioClipPlus audioClip;

        public ExplodeState(ShurikenExplode shuriken, Rigidbody2D rigidbody2D, GameObject particle, AudioClipPlus audioClip)
            : base(shuriken)
        {
            this.rigidbody2D = rigidbody2D;
            this.particle = particle;
            this.audioClip = audioClip;
        }

        public override void StateStart()
        {
            rigidbody2D.simulated = false;
            rigidbody2D.bodyType = RigidbodyType2D.Static;

            if(shuriken.bounds.Intersects(CameraFollow.onlyInstance.bufferedViewBounds))
            {
                AudioPool.PlayClip(audioClip);
                ObjectExtensions.InstantiateParticleAutoDie(particle, shuriken.transform.position, shuriken.transform.rotation);
            }

            Destroy(shuriken.gameObject);
        }

       

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return null;
        }
    }
    public class KillPlayerState : State, IStateFixedUpdate
    {
        int goOnFlyingFrameCount;
        Rigidbody2D rigidbody2D;

        //runtime
        int fixedUpdateCount = 0;

        public KillPlayerState(ShurikenExplode shuriken,
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

        void IStateFixedUpdate.StateFixedUpdate()
        {
            fixedUpdateCount++;

            if (fixedUpdateCount <= goOnFlyingFrameCount)
            {
                //什么都不做，于是继续飞
            }
            else
            {
                rigidbody2D.simulated = true;
                rigidbody2D.bodyType = RigidbodyType2D.Static;
                ended = true;
            }

        }

        public override void StateManuallyEnd()
        {

        }

        public override State NextState()
        {
            return null;
        }
    }
}