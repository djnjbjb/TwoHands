using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludo.Extensions;
using Ludo.TwoHandsWar;

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
    //-----------
    float swordSearchRadius = 1f;

    //实现用，外部引入，变量
    [SerializeField] LevelManager levelManager;
    THWController _controller;
    GameObject player;
    GameObject playerBody;
    GameObject playerLeft;
    GameObject playerRight;
    GameObject paperCut;
    GameObject paperBody;
    GameObject paperLeft;
    GameObject paperRight;
    GameObject paperCutSword;
    GameObject door;
    GameObject doorIn;
    GameObject ledBody;
    GameObject ledLeft;
    GameObject ledRight;
    GameObject ledSword;
    GameObject colorBodyObject;
    GameObject colorFistObject;
    Color colorBody;
    Color colorFist;
    Color colorSword;
    Color colorNot;
    

    //实现用，过程中，变量
    LedShineTime ledShineTimeBody;
    LedShineTime ledShineTimeLeft;
    LedShineTime ledShineTimeRight;
    LedShineTime ledShineTimeSword;
    bool ledJustShineBody;
    bool ledJustShineLeft;
    bool ledJustShineRight;
    bool ledJustShineSword;
    bool win = false;

    void Start()
    {
        _controller = THWController.singleton;
        player = _controller.gameObject;
        playerBody =  player.transform.Find("Body").gameObject;
        playerLeft =  player.transform.Find("LHFist").gameObject;
        playerRight = player.transform.Find("RHFist").gameObject;

        paperCut = this.transform.Find("PaperCut").gameObject;
        paperBody = paperCut.transform.Find("Body").gameObject;
        paperLeft = paperCut.transform.Find("LHFist").gameObject;
        paperRight = paperCut.transform.Find("RHFist").gameObject;

        door = this.transform.Find("Door").gameObject;
        doorIn = door.transform.Find("DoorIn").gameObject;
        ledBody = door.transform.Find("DoorLedBody").gameObject;
        ledLeft = door.transform.Find("DoorLedLeft").gameObject;
        ledRight = door.transform.Find("DoorLedRight").gameObject;

        colorBodyObject = this.transform.LudoFind("ColorBody", includeInactive: true).gameObject;
        colorFistObject = this.transform.LudoFind("ColorFist", includeInactive: true).gameObject;
        colorBody = colorBodyObject.GetComponent<SpriteRenderer>().color;
        colorFist = colorFistObject.GetComponent<SpriteRenderer>().color;
        ColorUtility.TryParseHtmlString("#FFFFFF", out colorNot);

        ledShineTimeBody = new LedShineTime();
        ledShineTimeLeft = new LedShineTime();
        ledShineTimeRight = new LedShineTime();
        ledJustShineBody = false;
        ledJustShineLeft = false;
        ledJustShineRight = false;


        paperCutSword =  null;
        if (this.transform.Find("PaperCutSword") != null)
        {
            paperCutSword = this.transform.Find("PaperCutSword").gameObject;
        }
        if (paperCutSword != null)
        {
            ledSword = door.transform.Find("DoorLedSword").gameObject;
            colorSword = this.transform.LudoFind("ColorSword", includeInactive: true).GetComponent<SpriteRenderer>().color;
            ledShineTimeSword = new LedShineTime();
            ledJustShineSword = false;
        }
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
            if (paperCutSword != null)
            {
                ledSword.transform.Find("In").GetComponent<SpriteRenderer>().color = colorSword;
                ledSword.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorSword;
            }
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
                _controller.hcAudio.PlayLightUpBody();
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
                _controller.hcAudio.PlayLightUpLeft();
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
                _controller.hcAudio.PlayLightUpRight();
            }
        }
        else
        {
            ledJustShineRight = false;
        }

        if (paperCutSword == null)
        {
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
        else
        {
            Stuff.Sword nearestSword = null;
            {
                LayerMask LMStuff = LayerMask.GetMask("Stuff");
                var colliders = Physics2D.OverlapCircleAll((Vector2)paperCutSword.transform.position,
                        swordSearchRadius, LMStuff);
                var swordList = new List<Stuff.Sword>();
                //一个sword有多个collider，所以，需要判定重复。
                foreach (var collider in colliders)
                {
                    GameObject colliderObject = collider.gameObject;
                    GameObject stuffObject = null;
                    Stuff.Stuff stuff = null;
                    while (true)
                    {
                        stuff = colliderObject.GetComponent<Stuff.Stuff>();
                        if (stuff == null)
                        {
                            var transformParent = colliderObject.transform.parent;
                            if (transformParent == null)
                                break;
                            colliderObject = transformParent.gameObject;
                        }
                        else
                        {
                            stuffObject = stuff.gameObject;
                            break;
                        }
                    }
                    if (stuff != null)
                    {
                        if (stuff is Stuff.Sword)
                        {
                            Stuff.Sword sword = stuff as Stuff.Sword;
                            if (!swordList.Contains(sword))
                            {
                                swordList.Add(sword);
                            }
                        }
                    }
                }
                float minDistance = float.MaxValue;
                foreach (var s in swordList)
                {
                    float dis = Vector2.Distance(paperCutSword.transform.position, s.transform.position);
                    if (dis < minDistance)
                    {
                        nearestSword = s;
                    }
                }
                
                Y.DebugPanel.Log(_message: colliders.Length, _name: "collidersLength", _category: "Sword");
                Y.DebugPanel.Log(_message: swordList.Count, _name: "SwordListCount", _category: "Sword");
            }

            
            bool swordSign = false;
            if (nearestSword != null)
            {
                //需要判定位置 和 方向
                Vector2 directionPaperCutSword = paperCutSword.transform.Find("Tip").position - paperCutSword.transform.Find("Body").position;
                Vector2 directionNearestSword = nearestSword.transform.Find("Tip").position - nearestSword.transform.Find("Body").position;
                swordSign =
                    (Vector2.Distance(nearestSword.transform.position, paperCutSword.transform.position) <= positionDiffThreshold)
                    && (Vector2.Angle(directionNearestSword, directionPaperCutSword) <= 10f);
            }
            ledShineTimeSword.Update(swordSign);
            float timeSword = ledShineTimeSword.shineTime > positionToSustainTime ? positionToSustainTime : ledShineTimeSword.shineTime;

            if (timeSword == positionToSustainTime)
            {
                if (!ledJustShineSword)
                {
                    ledJustShineSword = true;
                    _controller.hcAudio.PlayLightUpSword();
                }
            }
            else
            {
                ledJustShineSword = false;
            }

            if (timeBody == positionToSustainTime
                && timeLeft == positionToSustainTime
                && timeRight == positionToSustainTime
                && timeSword == positionToSustainTime)
            {
                ledBody.transform.Find("In").GetComponent<SpriteRenderer>().color = colorBody;
                ledLeft.transform.Find("In").GetComponent<SpriteRenderer>().color = colorFist;
                ledRight.transform.Find("In").GetComponent<SpriteRenderer>().color = colorFist;
                ledBody.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorBody;
                ledLeft.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
                ledRight.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorFist;
                ledSword.transform.Find("In").GetComponent<SpriteRenderer>().color = colorSword;
                ledSword.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorSword;
                StartCoroutine(WinProcedure());
            }
            else
            {
                ledBody.transform.Find("In").GetComponent<SpriteRenderer>().color = Color.Lerp(colorNot, colorBody, timeBody / positionToSustainTime);
                ledLeft.transform.Find("In").GetComponent<SpriteRenderer>().color = Color.Lerp(colorNot, colorFist, timeLeft / positionToSustainTime);
                ledRight.transform.Find("In").GetComponent<SpriteRenderer>().color = Color.Lerp(colorNot, colorFist, timeRight / positionToSustainTime);
                ledSword.transform.Find("In").GetComponent<SpriteRenderer>().color = Color.Lerp(colorNot, colorSword, timeSword / positionToSustainTime);
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
                if (timeSword == positionToSustainTime)
                    ledSword.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorSword;
                else
                    ledSword.transform.Find("Out").GetComponent<SpriteRenderer>().color = colorNot;
            }
        }
        
    }

    IEnumerator WinProcedure()
    {
        win = true;

        _controller.hcAudio.BackGroundMusicFadeDown(backGroundMusicFadeDownTime, this);
        _controller.hcAudio.PlayDoorOpen();

        SpriteRenderer[] paperCutSpriteRenderers = paperCut.transform.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        List<SpriteRenderer> paperCutSpriteRendererList = new List<SpriteRenderer>(paperCutSpriteRenderers);
        if (paperCutSword != null)
        {
            paperCutSpriteRendererList.AddRange(paperCutSword.transform.GetComponentsInChildren<SpriteRenderer>(includeInactive: true));
        }

        StartCoroutine(DoorOpen());
        StartCoroutine(PaperCutFade(paperCutSpriteRendererList));

        yield return new WaitForSeconds(timeBeforePlayerFade);
        _controller.handFade.Fade(this, playerFadeTime);
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

    IEnumerator PaperCutFade(List<SpriteRenderer> renderList)
    {
        float startTime = Time.time;
        float alpha;

        while (Time.time - startTime < paperCutFadeTime)
        {
            alpha = 1 - (Time.time - startTime) / paperCutFadeTime;
            foreach (var render in renderList)
            {
                render.color = new Color(render.color.r, render.color.g, render.color.b, alpha);
            }
            yield return null;
        }

        alpha = 0f;
        foreach (var render in renderList)
        {
            render.color = new Color(render.color.r, render.color.g, render.color.b, alpha);
        }
    }


}
