using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeVelocityWhileJumping : Velocity
{
    private float gravity;
    private float speedDownPartMax;
    public WholeVelocityWhileJumping(float gravity, float speedDownPartMax)
    {
        this.gravity = gravity;
        this.speedDownPartMax = speedDownPartMax;
    }

    public void StartJump(Velocity velocityBeforeJump)
    {
        speed = velocityBeforeJump.speed;
        direction = velocityBeforeJump.direction;
    }
    public void FixedUpdateManually()
    {
        if (leftFistState == FistState.GrabEnv || rightFistState == FistState.GrabEnv)
        {
            Debug.LogError("WholeVelocityWhileJumping should not be called when fist grabing");
            return;
        }


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
    }
}
