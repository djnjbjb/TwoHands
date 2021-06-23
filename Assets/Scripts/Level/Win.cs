using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{
    class LedShineTime
    {
        public float startTime;
        public float shineTime;

        public LedShineTime()
        {
            startTime = -1f;
            shineTime = 0f;
        }
        public void Update(bool sign)
        {
            if (!sign)
            {
                startTime = -1f;
            }
            else
            {
                if (startTime == -1f)
                {
                    startTime = Time.time;

                }
            }

            if (startTime <= 0f)
            {
                shineTime = 0f;
            }
            else
            {
                shineTime = Time.time - startTime;
            }
        }
    }

    //设计用变量
    float positionDiffThreshold = 0.15f;
    float positionToSustainTime = 0.3f;
    //------------
    float doorOpenProcedureTime = 2.5f;
    float paperCutFadeTime = 1.5f;
    float timeBeforePlayerFade = 1.5f;
    float playerFadeTime = 2.5f;
    float timeBeforeNextLevel = 2.7f;
    float backGroundMusicFadeDownTime = 1.5f;

    //实现用，外部引入，变量
    [SerializeField] LevelManager levelManager;
    [SerializeField] HandControl handcontrol;
    [SerializeField] GameObject player;
    GameObject playerBody;
    GameObject playerLeft;
    GameObject playerRight;
    [SerializeField] GameObject paperCut;
    GameObject paperBody;
    GameObject paperLeft;
    GameObject paperRight;
    [SerializeField] GameObject door;
    GameObject doorIn;
    GameObject ledBody;
    GameObject ledLeft;
    GameObject ledRight;
    [SerializeField] GameObject colorBodyObject;
    [SerializeField] GameObject colorFistObject;
    Color colorBody;
    Color colorFist;
    Color colorNot;

    //实现用，过程中，变量
    LedShineTime ledShineTimeBody;
    LedShineTime ledShineTimeLeft;
    LedShineTime ledShineTimeRight;
    bool ledJustShineBody;
    bool ledJustShineLeft;
    bool ledJustShineRight;
    bool win = false;

    void Start()
    {
        playerBody =  player.transform.Find("Body").gameObject;
        playerLeft =  player.transform.Find("LHFist").gameObject;
        playerRight = player.transform.Find("RHFist").gameObject;

        paperBody = paperCut.transform.Find("Body").gameObject;
        paperLeft = paperCut.transform.Find("LHFist").gameObject;
        paperRight = paperCut.transform.Find("RHFist").gameObject;

        doorIn = door.transform.Find("DoorIn").gameObject;
        ledBody = door.transform.Find("DoorLedBody").gameObject;
        ledLeft = door.transform.Find("DoorLedLeft").gameObject;
        ledRight = door.transform.Find("DoorLedRight").gameObject;

        colorBody = colorBodyObject.GetComponent<SpriteRenderer>().color;
        colorFist = colorFistObject.GetComponent<SpriteRenderer>().color;
        ColorUtility.TryParseHtmlString("#FFFFFF", out colorNot);

        ledShineTimeBody = new LedShineTime();
        ledShineTimeLeft = new LedShineTime();
        ledShineTimeRight = new LedShineTime();
        ledJustShineBody = false;
        ledJustShineLeft = false;
        ledJustShineRight = false;
    }

    void Update()
    {
        if (win)
        {
            ledBody.transform.Find("In").GetComponent<SpriteRenderer>().color = colorBody;
            ledLeft.transform.Find("In").GetComponent<SpriteRenderer>().color = colorFist;
            ledRight.transform.Find("In").GetComponent<SpriteRenderer>().color = colorFist;
            ledBody.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorBody;
            ledLeft.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
            ledRight.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
            return;
        }

        bool bodySign = Vector3.Distance(playerBody.transform.position, paperBody.transform.position) <= positionDiffThreshold;
        bool leftSign = Vector3.Distance(playerLeft.transform.position, paperLeft.transform.position) <= positionDiffThreshold;
        bool rightSign = Vector3.Distance(playerRight.transform.position, paperRight.transform.position) <= positionDiffThreshold;
        ledShineTimeBody.Update(bodySign);
        ledShineTimeLeft.Update(leftSign);
        ledShineTimeRight.Update(rightSign);

        float timeBody = ledShineTimeBody.shineTime > positionToSustainTime ? positionToSustainTime : ledShineTimeBody.shineTime;
        float timeLeft = ledShineTimeLeft.shineTime > positionToSustainTime ? positionToSustainTime : ledShineTimeLeft.shineTime;
        float timeRight = ledShineTimeRight.shineTime > positionToSustainTime ? positionToSustainTime : ledShineTimeRight.shineTime;
        

        if (timeBody == positionToSustainTime)
        {
            if (!ledJustShineBody)
            {
                ledJustShineBody = true;
                handcontrol.hcAudio.PlayLightUpBody();
            }
        }
        else
        {
            ledJustShineBody = false;
        }
        if (timeLeft == positionToSustainTime)
        {
            if (!ledJustShineLeft)
            {
                ledJustShineLeft = true;
                handcontrol.hcAudio.PlayLightUpLeft();
            }
        }
        else
        {
            ledJustShineLeft = false;
        }
        if (timeRight == positionToSustainTime)
        {
            if (!ledJustShineRight)
            {
                ledJustShineRight = true;
                handcontrol.hcAudio.PlayLightUpRight();
            }
        }
        else
        {
            ledJustShineRight = false;
        }

        if (timeBody == positionToSustainTime && timeLeft == positionToSustainTime && timeRight == positionToSustainTime)
        {
            ledBody.transform.Find("In").GetComponent<SpriteRenderer>().color = colorBody;
            ledLeft.transform.Find("In").GetComponent<SpriteRenderer>().color = colorFist;
            ledRight.transform.Find("In").GetComponent<SpriteRenderer>().color = colorFist;
            ledBody.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorBody;
            ledLeft.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
            ledRight.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
            StartCoroutine(WinProcedure());
        }
        else
        {
            ledBody.transform.Find("In").GetComponent<SpriteRenderer>().color = Color.Lerp(colorNot, colorBody, timeBody / positionToSustainTime);
            ledLeft.transform.Find("In").GetComponent<SpriteRenderer>().color = Color.Lerp(colorNot, colorFist, timeLeft / positionToSustainTime);
            ledRight.transform.Find("In").GetComponent<SpriteRenderer>().color = Color.Lerp(colorNot, colorFist, timeRight / positionToSustainTime);
            if (timeBody == positionToSustainTime)
                ledBody.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorBody;
            else
                ledBody.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorNot;
            if (timeLeft == positionToSustainTime)
                ledLeft.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
            else
                ledLeft.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorNot;
            if (timeRight == positionToSustainTime)
                ledRight.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
            else
                ledRight.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorNot;
        }
    }

    IEnumerator WinProcedure()
    {
        win = true;

        handcontrol.hcAudio.BackGroundMusicFadeDown(backGroundMusicFadeDownTime, this);
        handcontrol.hcAudio.PlayDoorOpen();

        SpriteRenderer[] paperCutSpriteRenderers = this.transform.Find("PaperCut").GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        StartCoroutine(DoorOpen());
        StartCoroutine(PaperCutFade(paperCutSpriteRenderers));

        yield return new WaitForSeconds(timeBeforePlayerFade);
        handcontrol.handFade.Fade(this, playerFadeTime);
        yield return new WaitForSeconds(timeBeforeNextLevel);
        levelManager.NextLevel();
    }

    IEnumerator DoorOpen()
    {
        float startTime = Time.time;
        while (Time.time - startTime < doorOpenProcedureTime)
        {
            float angle = 90f * (Time.time - startTime) / doorOpenProcedureTime;
            doorIn.transform.localRotation = Quaternion.Euler(new Vector3(0, angle, 0f));
            yield return null;
        }
        doorIn.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 0f));
    }

    IEnumerator PaperCutFade(SpriteRenderer[] renderers)
    {
        float startTime = Time.time;
        float alpha;

        while (Time.time - startTime < paperCutFadeTime)
        {
            alpha = 1 - (Time.time - startTime) / paperCutFadeTime;
            foreach (var render in renderers)
            {
                render.color = new Color(render.color.r, render.color.g, render.color.b, alpha);
            }
            yield return null;
        }

        alpha = 0f;
        foreach (var render in renderers)
        {
            render.color = new Color(render.color.r, render.color.g, render.color.b, alpha);
        }
    }


}
