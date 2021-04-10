using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeVelocityWhileJumping : Velocity
{
    public class Params
    {
        public FistStatePlus leftFistState { get; set; }
        public FistStatePlus rightFistState { get; set; }
        public FootStatePlus footState { get; set; }
    }

    private float gravity;
    private float friciton;
    private float speedDownPartMax;
    public WholeVelocityWhileJumping(float gravity, float friction, float speedDownPartMax)
    {
        this.gravity = gravity;
        this.friciton = friction;
        this.speedDownPartMax = speedDownPartMax;
    }

    public void StartJump(Velocity velocityBeforeJump)
    {
        speed = velocityBeforeJump.speed;
        direction = velocityBeforeJump.direction;
    }
    public void FixedUpdateManually(Params @params)
    {
        FistStatePlus leftFistState = @params.leftFistState;
        FistStatePlus rightFistState = @params.rightFistState;
        FootStatePlus footState = @params.footState;
        if (leftFistState == FistState.GrabEnv || rightFistState == FistState.GrabEnv)
        {
            Debug.LogError("WholeVelocityWhileJumping should not be called when fist grabing");
            return;
        }

        //1.应用重力加速度
        float deltaSpeed = gravity * Time.fixedDeltaTime;
        Vector2 deltaVelocity = deltaSpeed * Vector2.down;
        Vector2 velocityNew = velocity + deltaVelocity;
        float velocityNewRightPart = Vector2.Dot(velocityNew, Vector2.right);
        float velocityNewDownPart = Vector2.Dot(velocityNew, Vector2.down);
        if (velocityNewDownPart >=0 && velocityNewDownPart > speedDownPartMax)
        {
            velocityNewDownPart = speedDownPartMax;
        }
        velocityNew = velocityNewRightPart * Vector2.right + velocityNewDownPart * Vector2.down;
        
        speed = velocityNew.magnitude;
        direction = velocityNew.normalized;


        //2 根据速度方向和footState来更改速度
        float velocityDownPart = Vector2.Dot(velocity, Vector2.down);
        //2.1 向上运动
        if (velocityDownPart < 0)
        {
            return;
        }
        //2.2 向下或水平运动
        if (footState == FootState.Surface || footState == FootState.EnvGround)
        {
            //变为水平运动
            float velocityRightPart = Vector2.Dot(velocity, Vector2.right);
            speed = velocityRightPart;
            direction = Vector2.right;
            //水平方向摩擦力减速
            float speedNew = speed - friciton * Time.fixedDeltaTime;
            speed = Mathf.Clamp(speedNew, 0f, float.MaxValue);
        }
        else if (footState == FootState.Air)
        {

        }
        else if (footState == FootState.EnvRock)
        {

        }
    }
}