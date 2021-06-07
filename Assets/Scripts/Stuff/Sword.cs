using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stuff
{
    
    public class Sword : Stuff
    {
        const float angleSpeedMax = 720f;
        const float angleAcceleration = 3600f;
        const float dampRatio = 5;
        const float angleCut = 6f;
        const float speedCut = 500f;
        //这部分还需要修改

        float angleSpeed = 0f;
        GameObject tip;
        GameObject body;

        void Start()
        {
            tip = transform.Find("Tip").gameObject;
            body = transform.Find("Body").gameObject;
        }

        void FixedUpdate()
        {
            SetVisualLayer();
            if (fist != null)
            {
                HandControl.ValueForSword value = HandControl.handControl.GetValueForSword(fist);
                Vector2 handOffset = value.offset;
                gameObject.transform.position += (Vector3)handOffset;

                Vector2 swordDirection = tip.transform.position - body.transform.position;
                SwordRotate(handOffset, swordDirection);
            }
        }

        void SetVisualLayer()
        {
            if (fist != null)
            {
                var sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach(var sprite in sprites)
                {
                    sprite.sortingLayerName = "High";
                }
            }
            if (fist == null)
            {
                var sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sprite in sprites)
                {
                    sprite.sortingLayerName = "Env";
                }
            }
        }

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
    }
}


