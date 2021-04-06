using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ParameterForSpeed
{
    public bool anyKeyHold;
    public Vector2 keyDir;
    public HandState handState;
}
public class SpeedParameter
{
    public float speedBeforeKeyRelease;
    public bool keyHoldState;
    public bool keyHoldStatePre;
    public float keyReleaseTime;

    public SpeedParameter()
    {
        speedBeforeKeyRelease = 0f;
        keyHoldState = false;
        keyHoldStatePre = false;
        keyReleaseTime = 0f;
    }

    public void Refresh(bool newKeyHoldState, float speed)
    {
        keyHoldState = newKeyHoldState;
        RefreshSpeedBeforeKeyRelease(speed);
        RefreshTime();
        keyHoldStatePre = keyHoldState;
    }

    private void RefreshSpeedBeforeKeyRelease(float speed)
    {
        if (!keyHoldState && keyHoldStatePre)
            speedBeforeKeyRelease = speed;
        if (keyHoldState)
            speedBeforeKeyRelease = -1f;
    }

    private void RefreshTime()
    {
        if (keyHoldState)
        {
            keyReleaseTime = -1f;
        }
        else
        {
            if (!keyHoldStatePre)
                keyReleaseTime += Time.fixedDeltaTime;
            else
                keyReleaseTime = 0f;
        }
    }
}

public class FistVelocity
{
    public Vector2 direction { get; set; }
    public float speed { get; set; }

    private SpeedParameter speedParameter;
    private float speedStart;
    private float speedMaxOneFist;
    private float speedMaxTwoFist;

    public FistVelocity(float length)
    {
        direction = new Vector2();
        speed = 0f;

        speedParameter = new SpeedParameter();
        speedStart = 0.5f * length;
        speedMaxOneFist = 5f * length;
        speedMaxTwoFist = 7f * length;
    }

    public void RefreshSpeedOneFist(ParameterForSpeed parameter)
    {
        RefreshSpeedParameter(parameter);
        RefreshSpeedOneFistDueToSpeedParameter(parameter);
    }

    public void AccelerateSpeedTwoFistGrabed(ParameterForSpeed parameter, float speedBigger)
    {
        RefreshSpeedParameter(parameter);
        speed = speedBigger;
        AccelerateSpeedTwoFistGrabedDueToSpeedParameter();
    }

    void RefreshSpeedParameter(ParameterForSpeed parameter)
    {
        speedParameter.Refresh(parameter.anyKeyHold, speed);
    }

    void RefreshSpeedOneFistDueToSpeedParameter(ParameterForSpeed parameter)
    {
        /*
            相比之前的代码，需要补充，如果手的速度超过了单手速度怎么办。
                超速有2种情况：
                第一种是本来就要减速。
                第二种是不用减速。
            现在的方法：
                直接减速到单手速度上限。
                然后再根据手的情况处理。
        */
        if (speed > speedMaxOneFist) speed = speedMaxOneFist;
        if (speedParameter.keyHoldState && parameter.keyDir != new Vector2())
            speed = Accelerate(speed);
        if (speedParameter.keyHoldState && parameter.keyDir == new Vector2())
            speed = speed;
        if (!speedParameter.keyHoldState)
            speed = Decelerate(speedParameter.speedBeforeKeyRelease, speedParameter.keyReleaseTime);
    }

    void AccelerateSpeedTwoFistGrabedDueToSpeedParameter()
    {
        speed = AccelerateTwoFistGrabed(speed);
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
        speed = Mathf.Clamp(speed, speedStart, speedMaxOneFist);
        return speed;
    }

    float AccelerateTwoFistGrabed(float speedPre)
    {
        //
        //公式:   speed = 0.5 + 70t^2
        //       speed = speedStart + p * t^2
        //speed不超过max
        //
        const float p = 70f;

        if (speedPre < speedStart)
            return speedStart;

        float tPre = Mathf.Pow((speedPre - speedStart) / p, 0.5f);
        float t = tPre + Time.fixedDeltaTime;
        float speed = speedStart + p * Mathf.Pow(t, 2);
        speed = Mathf.Clamp(speed, speedStart, speedMaxTwoFist);
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
public class TwoFistVelocity
{
    public FistVelocity left;
    public FistVelocity right;

    public TwoFistVelocity(float length)
    {
        left = new FistVelocity(length);
        right = new FistVelocity(length);
    }

    public void RefreshSpeed(ParameterForSpeed leftParameter, ParameterForSpeed rightParameter)
    {
        /*
            如果双手都是GrabEnv，就要双手协同
            单手比较简单，用以前的方式。
        */
        if (leftParameter.handState == HandState.GrabEnv && rightParameter.handState == HandState.GrabEnv)
        {
            /*
                如果双手同向，则双手都按照RefreshSpeedTwoFist来加速。
                否则，有多种情况。
                    双手方向不同。
                    一只手有方向，一只手方向为0
                    双手方向都为0
                都根据一只手的情况计算就可以了。
             */
            if ( (leftParameter.keyDir == rightParameter.keyDir) && leftParameter.keyDir != new Vector2())
            {
                float speedBigger = Mathf.Max(left.speed, right.speed);
                left.AccelerateSpeedTwoFistGrabed(leftParameter, speedBigger);
                right.AccelerateSpeedTwoFistGrabed(rightParameter, speedBigger);
            }
            else
            {
                left.RefreshSpeedOneFist(leftParameter);
                right.RefreshSpeedOneFist(rightParameter);
            }
        }
        else
        {
            left.RefreshSpeedOneFist(leftParameter);
            right.RefreshSpeedOneFist(rightParameter);
        }
    }
}
