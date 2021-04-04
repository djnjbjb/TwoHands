using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistVelocity
{
    public class VelocityDetail
    {
        private class SpeedParameter
        {
            public float speedBeforeKeyRelease;
            public bool keyHoldState;
            public bool keyHoldStatePre;
            public float keyHoldTime;
            public float keyReleaseTime;

            public SpeedParameter()
            {
                speedBeforeKeyRelease = 0f;
                keyHoldState = false;
                keyHoldStatePre = false;
                keyHoldTime = -1f;
                keyReleaseTime = 0f;
            }

            public void RefreshSpeedBeforeKeyRelease(float speed)
            {
                if (!keyHoldState && keyHoldStatePre)
                    speedBeforeKeyRelease = speed;
                if (keyHoldState)
                    speedBeforeKeyRelease = -1f;
            }

            public void RefreshTime()
            {
                if (keyHoldState)
                {
                    if (keyHoldStatePre)
                        keyHoldTime += Time.fixedDeltaTime;
                    else
                        keyHoldTime = 0f;
                    keyReleaseTime = -1f;
                }
                else
                {
                    if (!keyHoldStatePre)
                        keyReleaseTime += Time.fixedDeltaTime;
                    else
                        keyReleaseTime = 0f;
                    keyHoldTime = -1f;
                }
            }

            public void Refresh(bool newKeyHoldState, float speed)
            {
                keyHoldState = newKeyHoldState;
                RefreshSpeedBeforeKeyRelease(speed);
                RefreshTime();
                keyHoldStatePre = keyHoldState;
            }
        }
        
        public Vector2 direction { get; set; }
        public float speed { get; set; }
        
        private SpeedParameter speedParameter;
        private float speedStart;
        private float speedMax;

        public VelocityDetail(float length)
        {
            direction = new Vector2();
            speed = 0f;

            speedParameter = new SpeedParameter();
            speedStart = 0.5f * length;
            speedMax = 5f * length;
        }

        public void RefreshSpeedAndRelated(bool newKeyHoldState)
        {
            speedParameter.Refresh(newKeyHoldState, speed);
            if (speedParameter.keyHoldState)
                speed = Accelerate(speed);
            if (!speedParameter.keyHoldState)
                speed = Decelerate(speedParameter.speedBeforeKeyRelease, speedParameter.keyReleaseTime);
        }

        float Accelerate(float speedPre)
        {
            //
            //公式:   speed = 0.5 + 50t^2
            //       speed = speedStart + p * t^2
            //speed不超过max
            //
            const float p = 50f;

            if (speedPre < speedStart)
                return speedStart;

            float tPre = Mathf.Pow((speedPre - speedStart) / p, 0.5f);
            float t = tPre + Time.fixedDeltaTime;
            float speed = speedStart + p * Mathf.Pow(t, 2);
            speed = Mathf.Clamp(speed, speedStart, speedMax);
            return speed;
        }

        float Decelerate(float v0, float time)
        {
            /*
                v = (v0^2 - 1379*t^2.9) ^ 0.5
                v = (v0^2 - p1*t^p2) ^ 0.5
                vo是松开按键前的速度
                t是松开按键的时间

                v < speedStart时，直接归0。
             */
            float p1 = 1379f;
            float p2 = 2.9f;

            float a = Mathf.Pow(v0, 2) - p1 * Mathf.Pow(time, p2);

            if (a <= 0)
                return 0f;

            float speed = Mathf.Pow(a, 0.5f);
            if (speed < speedStart)
            {
                speed = 0f;
            }

            return speed;
        }
    }

    public VelocityDetail left;
    public VelocityDetail right;

    public FistVelocity(float length)
    {
        left = new VelocityDetail(length);
        right = new VelocityDetail(length);
    }

    public void RefreshSpeed()
    {
        left.RefreshSpeedAndRelated(HKey.lDown || HKey.lUp || HKey.lRight || HKey.lLeft);
        right.RefreshSpeedAndRelated(HKey.rDown || HKey.rUp || HKey.rRight || HKey.rLeft);
    }
}
