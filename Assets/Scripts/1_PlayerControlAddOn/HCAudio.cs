using System.Collections;
using UnityEngine;
using Ludo;
public class HCAudio
{
    AudioSource backGroundMusic;
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
    AudioSource lightUpSword;
    AudioSource doorOpen;
    AudioSource[] swordWhooshes;
    AudioSource throwSword;

    public HCAudio()
    {
        backGroundMusic = GameObject.Find("Audio").transform.Find("BackGround").GetComponent<AudioSource>();
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
        lightUpSword = GameObject.Find("Audio").transform.Find("LightUpSword").GetComponent<AudioSource>();
        doorOpen = GameObject.Find("Audio").transform.Find("DoorOpen").GetComponent<AudioSource>();
        swordWhooshes = GameObject.Find("Audio").transform.Find("SwordWhooshes").gameObject.GetComponents<AudioSource>();
        throwSword = GameObject.Find("Audio").transform.Find("ThrowSword").gameObject.GetComponent<AudioSource>();
    }

    public void PlayHandRelated(FistState leftState, FistState leftStatePre, 
                   FistState rightState, FistState rightStatePre,
                   Vector2 leftOffset, Vector2 rightOffset )
    {
        if (leftState.IsGrabingThings() && !leftStatePre.IsGrabingThings())
        {
            leftGrip.Play();
        }
        else if (!leftState.IsGrabingThings() && leftStatePre.IsGrabingThings())
        {
            leftRelease.Play();
        }

        if (rightState.IsGrabingThings() && !rightStatePre.IsGrabingThings())
        {
            rightGrip.Play();
        }
        else if (!rightState.IsGrabingThings() && rightStatePre.IsGrabingThings())
        {
            rightRelease.Play();
        }

        /*
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
        */

    }

    public void BackGroundMusicFadeDown(float time, MonoBehaviour mono)
    {
        mono.StartCoroutine(BackGroundMusicFadeDownCoroutine(time));
    }

    IEnumerator BackGroundMusicFadeDownCoroutine(float time)
    {
        float startTime = Time.fixedTime;
        while (Time.fixedTime - startTime <= time)
        {
            backGroundMusic.volume = 1 - (Time.fixedTime - startTime)/time;
            yield return new WaitForFixedUpdate();
        }
        backGroundMusic.volume = 0;
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

    public void PlayLightUpSword()
    {
        lightUpSword.Play();
    }

    public void PlayDoorOpen()
    {
        doorOpen.Play();
    }

    public void PlaySwordWhoosh(int num)
    {
        if (num >= swordWhooshes.Length)
        {
            throw new System.Exception("num >= swordWhooshes.Length");
        }
        swordWhooshes[num].Play();
    }
    public float GetSwordWhooshClipTime(int num)
    {
        if (num >= swordWhooshes.Length)
        {
            throw new System.Exception("num >= swordWhooshes.Length");
        }
        return swordWhooshes[num].clip.length;
    }

    public int GetSwordWhooshCount()
    {
        return swordWhooshes.Length;
    }

    public void PlayThrowSword()
    {
        throwSword.Play();
    }
}
