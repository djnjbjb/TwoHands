using UnityEngine;
using Ludo;
public class HCAudio
{
    AudioSource leftGrip;
    AudioSource leftRelease;
    AudioSource leftMove;
    AudioSource rightGrip;
    AudioSource rightRelease;
    AudioSource rightMove;
    AudioSource death;
    AudioSource lightUpBody;
    AudioSource lightUpLeft;
    AudioSource lightUpRight;
    AudioSource doorOpen;

    public HCAudio()
    {
        leftGrip     =  GameObject.Find("Audio").transform.Find("LeftGrip").GetComponent<AudioSource>();
        leftRelease  = GameObject.Find("Audio").transform.Find("LeftRelease").GetComponent<AudioSource>();
        leftMove     = GameObject.Find("Audio").transform.Find("LeftMove").GetComponent<AudioSource>();
        rightGrip    = GameObject.Find("Audio").transform.Find("RightGrip").GetComponent<AudioSource>();
        rightRelease = GameObject.Find("Audio").transform.Find("RightRelease").GetComponent<AudioSource>();
        rightMove    = GameObject.Find("Audio").transform.Find("RightMove").GetComponent<AudioSource>();
        death = GameObject.Find("Audio").transform.Find("Death").GetComponent<AudioSource>();
        lightUpBody = GameObject.Find("Audio").transform.Find("LightUpBody").GetComponent<AudioSource>();
        lightUpLeft = GameObject.Find("Audio").transform.Find("LightUpLeft").GetComponent<AudioSource>();
        lightUpRight = GameObject.Find("Audio").transform.Find("LightUpRight").GetComponent<AudioSource>();
        doorOpen = GameObject.Find("Audio").transform.Find("DoorOpen").GetComponent<AudioSource>();
    }

    public void PlayHandRelated(FistState leftState, FistState leftStatePre, 
                   FistState rightState, FistState rightStatePre,
                   Vector2 leftOffset, Vector2 rightOffset )
    {
        if (leftState == FistState.GrabEnv && leftStatePre == FistState.Free)
        {
            leftGrip.Play();
        }
        else if (leftState == FistState.Free && leftStatePre == FistState.GrabEnv)
        {
            leftRelease.Play();
        }

        if (rightState == FistState.GrabEnv && rightStatePre == FistState.Free)
        {
            rightGrip.Play();
        }
        else if (rightState == FistState.Free && rightStatePre == FistState.GrabEnv)
        {
            rightRelease.Play();
        }

        if ( !Utility.FloatEqual_WithIn0p001(leftOffset.magnitude, 0f) )
        {
            if (!leftMove.isPlaying)
            {
                leftMove.Play();
            }
        }
        else
        {
            if (leftMove.isPlaying)
            {
                leftMove.Stop();
            }
        }

        if (!Utility.FloatEqual_WithIn0p001(rightOffset.magnitude, 0f))
        {
            if (!rightMove.isPlaying)
            {
                rightMove.Play();
            }
        }
        else
        {
            if (rightMove.isPlaying)
            {
                rightMove.Stop();
            }
        }

    }

    public void PlayDeath()
    {
        death.Play();
    }

    public void PlayLightUpBody()
    {
        lightUpBody.Play();
    }

    public void PlayLightUpLeft()
    {
        lightUpLeft.Play();
    }

    public void PlayLightUpRight()
    {
        lightUpRight.Play();
    }

    public void PlayDoorOpen()
    {
        doorOpen.Play();
    }
}
