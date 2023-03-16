using System;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using Ludo.Extensions;
using Ludo.TwoHandsWar;
using Ludo.Types;
using Ludo.TwoHandsWar.Circumstance;

public partial class ShurikenExplode : MonoBehaviour
{
    /*
        文档：
        https://perfect-dive-450.notion.site/Shuriken-80d1294873f24e908ba5f30d348cc8f3
    */

    //Setting for inspector
    [Label("(Audio) Start")]
    [SerializeField] UnityEngine.Object acStartInspector;
    [Label("(Audio) Collider Sword")]
    [SerializeField] UnityEngine.Object acCollideSwordInspector;
    [Label("(Audio) Collider Wall")]
    [SerializeField] UnityEngine.Object acCollideWallInspector;

    //Setting
    [SerializeField] DirectionType directionType;
    [SerializeField] StartType startType;
    //OutSideCheck
    float outsideCheckInterval = 0.1f;
    //    State - Before Show
    [SerializeField] public float beforeShowIdleTime = 5f;
    float beforeShowFlyTime = 0.35f;
    float beforeShowIdleDistance = 100f;
    //     State - Prepare
    float prepareStandStillTime = 0.35f;
    float prepareBackwardTime = 0.5f;
    float prepareForwardTime = 0.15f;
    string prepareColorString = "#7E2020"; //"#a61b29" "#812f33"
    Vector2 prepareOffset = new Vector2(0.2f, 0.25f);//new Vector2(0.2f, 0.25f);
    float prepareAngleOffset = 15;
    Vector2 prepareScaleMultiple = new Vector2(0.9f, 1f);
    AudioClipPlus acStart;
    //    State - Fly
    float flySpeed = 13;
    AudioClipPlus acCollideSword;
    //    State -  KillPlayer
    int killPlayerStateGoOnFlyingCount = 2;
    //    State - Explode
    [SerializeField] GameObject explodeParticle;
    AudioClipPlus acCollideWall;
    

    //runtime const
    SpriteRenderer sprite;
    new Rigidbody2D rigidbody2D;
    Vector3 initPosition;
    Vector3 targetPointPosition;
    Vector3 frontToBack;
    Vector3 bottomToTop;
    Vector3 direction;
    float currentAngleInitForPrepare;

    //runtime variable
    StateController stateController;
    State state 
    {
        get { return stateController.GetState(); }
    }
    float lastOnCollisionFixedTime = -1;
    //thisRigidbody2D.simulated

    private void Start()
    {
        InitFields();

        //Start LateFixedUpdate
        StartCoroutine(LateFixedUpdateCoroutine());
        StartCoroutine(OutSideCheckCoroutine());
    }

    void FixedUpdate()
    {
        if (state != null && state.ended == true)
        {
            stateController.ChangeStateOnNaturalEnd();
        }

        if (state as IStateFixedUpdate != null)
        {
            (state as IStateFixedUpdate).StateFixedUpdate();
        }
    }

    void LateFixedUpdate()
    {
        if (state as ILateFixedUpdate != null)
        {
            (state as ILateFixedUpdate).StateLateFixedUpdate();
        }
    }

    void OutsideCheck()
    {
        if (state as IOutsideCheck != null)
        {
            if ((state as IOutsideCheck).OutsideCheck())
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //一帧可能执行多次OnCollisionEnter2D。但，具体的事件只执行一次。
        if (Time.fixedTime != lastOnCollisionFixedTime)
        {
            lastOnCollisionFixedTime = Time.fixedTime;

            {
                AudioPool.PlayClip(acCollideSword);
                stateController.ChangeStateOnCollision();
            }
        }
    }

    private void OnDestroy()
    {
        stateController.ChangeStateOnDestroy();
    }

    #region Level2 Function

    private void InitFields()
    {
        acStart = InitAudioClipPlus(acStartInspector);
        acCollideSword = InitAudioClipPlus(acCollideSwordInspector);
        acCollideWall = InitAudioClipPlus(acCollideWallInspector);

        //field: runtime unchanged
        sprite = GetComponent<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        initPosition = transform.position;
        targetPointPosition = transform.Find("TargetPoint").position;  //targetPoint是子节点。所以，为保证其位置正确，要在一开始记录。
        frontToBack = transform.Find("Back").position - transform.Find("Front").position;
        bottomToTop = transform.Find("Top").position - transform.Find("Bottom").position;
        {//direction
            if (directionType == DirectionType.AsPlaced)
            {
                //direction不变
            }
            else if (directionType == DirectionType.ToPlayer)
            {
                //调成Back到Front的方向，指向玩家
                Vector2 toPlayer = THWController.singleton.transform.position - transform.position;
                float angle = Vector2.SignedAngle(frontToBack, -toPlayer);
                transform.rotation *= Quaternion.Euler(0, 0, angle);
            }
            else if (directionType == DirectionType.ToTargetPoint)
            {
                Vector2 toTargetPoint = targetPointPosition - transform.position;
                float angle = Vector2.SignedAngle(frontToBack, -toTargetPoint);
                transform.rotation *= Quaternion.Euler(0, 0, angle);
            }
            direction = (transform.Find("Front").position - transform.Find("Back").position).normalized;
        }
        currentAngleInitForPrepare = transform.rotation.Angle2D();

        //field: runtime OnChanging
        stateController = new StateController(this);
        rigidbody2D.simulated = false;
        switch (startType)
        {
            case StartType.Auto:
                stateController.ChangeStateOnAutoStart();
                break;
            case StartType.Trigger:
                stateController.ChangeStateOnTriggerStart();
                break;
            default:
                throw new Exception("Invalid startType");
        }
    }

    #endregion

    #region Intermediate

    IEnumerator LateFixedUpdateCoroutine()
    {
        while (true)
        {
            LateFixedUpdate();
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator OutSideCheckCoroutine()
    {
        while (true)
        {
            OutsideCheck();
            yield return new WaitForSeconds(outsideCheckInterval);
        }
    }

    #endregion

    #region Tool
    private Bounds bounds
    {
        get
        {
            return GetComponent<Renderer>().bounds;
        }
    }
    

    private AudioClipPlus InitAudioClipPlus(UnityEngine.Object raw)
    {
        AudioClip ac = raw as AudioClip;
        if (ac != null)
        {
            return AudioClipPlus.Create(ac);
        }
        AudioClipPlus acp
            = raw as AudioClipPlus;
        if (acp != null)
        {
            return acp;
        }
        throw new Exception("Obejct for AudioClipPlus is neither AudioClip nor AudioClipPlusScriptable");
        return null;
    }

    #endregion
}
