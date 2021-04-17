using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ParameterForFistVelocity
{
    public bool anyKeyHold;
    public Vector2 keyDir;
    public Vector2 moveDir;
    public FistState handState;
}

public class SpeedParameter
{
    public bool keyHoldState;
    public bool keyHoldStatePre;
    public float keyReleaseTime;
    public float speedBeforeKeyRelease;

    public bool preIsTwoHand;
    public bool nowIsTwoHand;
    public float noTwoHandTime;
    public float lastTwoHandGrabedSpeed;
    

    public SpeedParameter()
    {
        keyReleaseTime = 0f;
        speedBeforeKeyRelease = 0f;
        keyHoldState = false;
        keyHoldStatePre = false;
        
        noTwoHandTime = 0f;
        lastTwoHandGrabedSpeed = 0f;
        preIsTwoHand = false;
        nowIsTwoHand = false;
    }

    public void RefreshOneHand(bool newKeyHoldState, float speed)
    {
        nowIsTwoHand = false;
        keyHoldState = newKeyHoldState;

        RefreshSpeedBeforeKeyRelease(speed);
        RefreshTime();
        RefreshLastTwoHandGrabedSpeed(speed);
        RefreshNoTwoHandTime();

        keyHoldStatePre = keyHoldState;
        preIsTwoHand = nowIsTwoHand;
    }

    public void RefreshTwoHand(bool newKeyHoldState, float speed)
    {
        nowIsTwoHand = true;
        keyHoldState = newKeyHoldState;

        RefreshSpeedBeforeKeyRelease(speed);
        RefreshTime();
        RefreshLastTwoHandGrabedSpeed(speed);
        RefreshNoTwoHandTime();

        keyHoldStatePre = keyHoldState;
        preIsTwoHand = nowIsTwoHand;
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

    private void RefreshLastTwoHandGrabedSpeed(float speed)
    {
        if (!nowIsTwoHand && preIsTwoHand)
            lastTwoHandGrabedSpeed = speed;
        if (nowIsTwoHand)
            lastTwoHandGrabedSpeed = -1f;
    }


    private void RefreshNoTwoHandTime()
    {
        if (nowIsTwoHand)
        {
            noTwoHandTime = -1f;
        }
        if (!nowIsTwoHand && !preIsTwoHand)
        {
            noTwoHandTime += Time.fixedDeltaTime;
        }
        if (!nowIsTwoHand && preIsTwoHand)
        {
            noTwoHandTime = 0f;
        }
    }
}

public class FistVelocity : Velocity
{
    /* ------------------------------------------------------
        在单手情况下，FistVelocity类的行为是比较完备的。
        有构造函数。
        然后每次更新时调用 RefreshSpeedOneFist()
        各方面都得到了妥善的处理

        在双手情况下，FistVelocity自身本就无法处理。
        必须借助 TwoFistVelocity 来完成。
        因此，FistVelocity提供一个接口 AccelerateSpeedTwoFistGrabed（）
        来执行现挂能操作。
        但因为自己也无法控制很多情况，所以这个结构只是完成功能。
        需要 TwoFistVelocity 来考虑调用的环境情况。
    ------------------------------------------------------ */
    private float speedPre;

    private SpeedParameter speedParameter;
    private float length;
    private float speedStart;
    private float speedMaxOneFist;
    private float speedMaxTwoFist;

    public FistVelocity(float length)
    {
        direction = new Vector2();
        speed = 0f;

        speedPre = 0f;

        this.length = length;
        speedParameter = new SpeedParameter();
        speedStart = 0.5f * length;
        speedMaxOneFist = 4f * length;
        speedMaxTwoFist = 5.6f * length;
    }

    /* ------------------------------------------------------
                主要部分，一只手移动用到
    ------------------------------------------------------ */
    #region 主要部分，一只手移动用到
    public void OffsetZeroOneHand()
    {
        speed = speedPre;
        /*
            在修改speed时，并不需要考虑对speedParameter.lastTwoHandGrabedSpeed进行修改。
            因为，lastTwoHandGrabedSpeed 是在每帧开始时，用上一帧的speed（即speedPre）设置的。
            所以，调用OffsetZero，只是在这一帧对速度赋值了两次，没有影响。
            下一帧的时候，才会根据这一帧最后的speed来搞lastTwoHandGrabedSpeed。
        */
    }

    public bool IsSpeedPreNoBiggerThanMaxOneFist()
    {
        return speedPre <= speedMaxOneFist;
    }

    public void RefreshSpeedOneFist(ParameterForFistVelocity parameter)
    {
        speedPre = speed;
        RefreshSpeedParameterOneHand(parameter);
        RefreshSpeedOneFistDueToSpeedParameter(parameter);

        direction = parameter.moveDir;
    }


    private void RefreshSpeedParameterOneHand(ParameterForFistVelocity parameter)
    {
        speedParameter.RefreshOneHand(parameter.anyKeyHold, speed);
    }

    private void RefreshSpeedOneFistDueToSpeedParameter(ParameterForFistVelocity parameter)
    {
        /*
            相比之前的代码，需要补充，如果手的速度超过了单手速度怎么办。
                超速有2种情况：
                第一种是本来就要减速。
                第二种是不用减速。
        */
        if (!speedParameter.keyHoldState)
        { 
            speed = Decelerate(speedParameter.speedBeforeKeyRelease, speedParameter.keyReleaseTime);
        }

        if (speedParameter.keyHoldState)
        {
            if (speed > speedMaxOneFist)
            {
                speed = Decelerate(speedParameter.lastTwoHandGrabedSpeed, speedParameter.noTwoHandTime);
                if (speed < speedMaxOneFist)
                    speed = speedMaxOneFist;
            }
            else
            {
                if (parameter.keyDir.magnitude != 0)
                    speed = Accelerate(speed);
                if (parameter.keyDir.magnitude == 0)
                    speed = speed;
            }
        }
    }

    private float Accelerate(float speedPre)
    {
        //
        //公式:   speed = 0.5*length + 223*(t^2.5)*length
        //       speed = speedStart + p * t^e * length
        //speed不超过max
        //
        const float p = 223f;
        const float e = 2.5f;

        if (speedPre < speedStart)
            return speedStart;

        float tPre = Mathf.Pow((speedPre - speedStart) /p/length, 1f/e);
        float t = tPre + Time.fixedDeltaTime;
        float speed = speedStart + p * Mathf.Pow(t, e) * length;
        speed = Mathf.Clamp(speed, speedStart, speedMaxOneFist);
        return speed;
    }

    private float Decelerate(float v0, float time)
    {
        /*
            v = ((v0/length)^2 - 780*t^2.35) ^ 0.5 * length
            v = ((v0/length)^2 - p*t^e) ^ 0.5 * length
            vo是松开按键前的速度
            t是松开按键的时间

            v < speedStart时，直接归0。
         */
        float p = 780f;
        float e = 2.35f;
        v0 = v0 / length;

        float a = Mathf.Pow(v0, 2) - p * Mathf.Pow(time, e);

        if (a <= 0)
            return 0f;

        float speed = Mathf.Pow(a, 0.5f) * length;
        if (speed < speedStart)
        {
            speed = 0f;
        }

        return speed;
    }
#endregion

    /* ------------------------------------------------------
                    附加部分
                        1.两只手移动
                        2.Offset为0时，speed的设置
     ------------------------------------------------------ */
                #region 附加部分，两只手移动才用到
    public void OffsetZeroTwoHand()
    {
        speed = speedPre;
        /*
            在修改speed时，并不需要考虑对speedParameter.lastTwoHandGrabedSpeed进行修改。
            因为，lastTwoHandGrabedSpeed 是在每帧开始时，用上一帧的speed（即speedPre）设置的。
            所以，调用OffsetZero，只是在这一帧对速度赋值了两次，没有影响。
            下一帧的时候，才会根据这一帧最后的speed来搞lastTwoHandGrabedSpeed。
        */
    }

    public void AccelerateSpeedTwoFistGrabed(ParameterForFistVelocity parameter, float speedBigger)
    {
        speedPre = speed;

        RefreshSpeedParameterTwoHand(parameter);
        speed = speedBigger;
        direction = parameter.moveDir;
        AccelerateSpeedTwoFistGrabedDueToSpeedParameter();
    }

    private void RefreshSpeedParameterTwoHand(ParameterForFistVelocity parameter)
    {
        speedParameter.RefreshTwoHand(parameter.anyKeyHold, speed);
    }

    private void AccelerateSpeedTwoFistGrabedDueToSpeedParameter()
    {
        speed = AccelerateTwoFistGrabed(speed);
    }
    private float AccelerateTwoFistGrabed(float speedPre)
    {
        //
        //公式:   speed = 0.5*length + 223*1.4*(t^2.5)*length
        //       speed = speedStart + p * t^e * length
        //speed不超过max
        //
        const float p = 223f * 1.4f;
        const float e = 2.5f;

        if (speedPre < speedStart)
            return speedStart;

        float tPre = Mathf.Pow((speedPre - speedStart) /p/length, 1f/e);
        float t = tPre + Time.fixedDeltaTime;
        float speed = speedStart + p * Mathf.Pow(t, e) * length;
        speed = Mathf.Clamp(speed, speedStart, speedMaxTwoFist);
        return speed;
    }



                #endregion
}
