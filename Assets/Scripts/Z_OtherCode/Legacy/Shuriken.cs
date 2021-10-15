using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Ludo.Extensions;


#if LegacyCode
public partial class Shuriken : MonoBehaviour
{
    /*
        文档：
        https://perfect-dive-450.notion.site/Shuriken-80d1294873f24e908ba5f30d348cc8f3
    */
    public enum DirectionType
    {
        AsPlaced,
        ToPlayer,
        ToTargetPoint
    }

    public enum StartType
    {
        Auto,
        Trigger
    }

    //Setting
    [SerializeField] DirectionType directionType;
    [SerializeField] StartType startType;
    //
    [SerializeField] public float beforeShowIdleTime = 5f;
    float beforeShowFlyTime = 0.35f;
    float beforeShowIdleDistance = 100f;
    //
    float prepareStandStillTime = 0.35f;
    float prepareBackwardTime = 0.5f;
    float prepareForwardTime = 0.15f;
    string prepareColorString = "#7E2020"; //"#a61b29" "#812f33"
    Vector2 prepareOffset = new Vector2(0.2f, 0.25f);//new Vector2(0.2f, 0.25f);
    float prepareAngleOffset = 15;
    Vector2 prepareScaleMultiple = new Vector2(0.9f, 1f);
    //
    float flySpeed = 13;
    float flyInEnvProtectTime = 0.35f;
    float flyInEnvDeceleration = 4f;
    int flyinEnvAngularDecelerationFrameCount = 2;
    //
    int killPlayerStateGoOnFlyingCount = 2;

    //runtime unchanged
    SpriteRenderer sprite;
    new Rigidbody2D rigidbody2D;
    Vector3 initPosition;
    Vector3 targetPointPosition;
    Vector3 frontToBack;
    Vector3 bottomToTop;
    Vector3 direction;
    float currentAngleInitForPrepare;

    //runtime OnChanging
    State _state = null;
    State state 
    {
        get { return _state; }
    }
    float lastOnCollisionFixedTime = -1;
    //thisRigidbody2D.simulated
    //HCAudio2.audioSource.PlayOneShot

    void ChangeStateOnTriggerStart()
    {
        _state = new BeforeTriggerFire(this,
            initPosition, beforeShowIdleDistance, direction, rigidbody2D);
        _state.StateStart();
    }

    void ChangeStateOnAutoStart()
    {
        _state = new BeforeShowState(this, 
            initPosition, beforeShowIdleTime, beforeShowFlyTime,
            beforeShowIdleDistance, direction, rigidbody2D);
        _state.StateStart();
    }

    void ChangeStateOnTriggerFire()
    {
        if (_state != null && _state.ended != true)
        {
            _state.StateManuallyEnd();
        }
        _state = new BeforeShowState(this,
            initPosition, beforeShowIdleTime, beforeShowFlyTime,
            beforeShowIdleDistance, direction, rigidbody2D);
        (_state as BeforeShowState).StateStartByTrigger();
    }

    void ChangeStateOnNaturalEnd()
    {
        State newState = _state.NextState();
        _state = newState;
        _state.StateStart();
    }

    void ChangeStateOnCollision()
    {
        if (_state != null && _state.ended != true)
        {
            _state.StateManuallyEnd();
        }
        _state = new FlyState(this, flySpeed, direction, 
            flyInEnvProtectTime:flyInEnvProtectTime, 
            flyInEnvDeceleration:flyInEnvDeceleration, 
            flyinEnvAngularDecelerationFrameCount: flyinEnvAngularDecelerationFrameCount,
            rigidbody2D);
        (_state as FlyState).StateStartOnCollision();
    }

    void ChangeStateOnKillPlayer()
    {
        if (_state != null && _state.ended != true)
        {
            _state.StateManuallyEnd();
        }
        _state = new KillPlayerState(this, killPlayerStateGoOnFlyingCount, rigidbody2D);
        (_state as KillPlayerState).StateStartOnKillPlayer();
    }

    private void Start()
    {
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
                Vector2 toPlayer = PlayerControl.playerControl.transform.position - transform.position;
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
        rigidbody2D.simulated = false;
        switch (startType)
        {
            case StartType.Auto:
                ChangeStateOnAutoStart();
                break;
            case StartType.Trigger:
                ChangeStateOnTriggerStart();
                break;
            default:
                throw new Exception("Invalid startType");
        }
        //Start LateFixedUpdate
        StartCoroutine(YieldReturnFixedUpdate());
    }

    void FixedUpdate()
    {

        if (state != null && state.ended == true)
        {
            ChangeStateOnNaturalEnd();
        }
        state.StateFixedUpdate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //一帧可能执行多次OnCollisionEnter2D。但，具体的事件只执行一次。
        if (Time.fixedTime != lastOnCollisionFixedTime)
        {
            lastOnCollisionFixedTime = Time.fixedTime;

            {
                HCAudio2.audioSource.PlayOneShot(HCAudio2.instance.acDang);
                ChangeStateOnCollision();
            }
        }
    }

    IEnumerator YieldReturnFixedUpdate()
    {
        while (true)
        {
            LateFixedUpdate();
            yield return new WaitForFixedUpdate();
        }
    }

    void LateFixedUpdate()
    {
        state.StateLateFixedUpdate();
    }

    public void KillPlayerEffect()
    {
        ChangeStateOnKillPlayer();
    }

    public void FireByTrigger()
    {
        ChangeStateOnTriggerFire();
    }

#region Editor
    void OnDrawGizmosSelected()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            Gizmos.color = new Color(1, 1, 1, 1f);
            if (directionType == DirectionType.AsPlaced)
            {
                direction = transform.Find("Front").position - transform.Find("Back").position;
                Gizmos.DrawLine(transform.position, transform.position + direction * 10);
            }
            if (directionType == DirectionType.ToTargetPoint)
            {
                Gizmos.DrawLine(transform.position, transform.Find("TargetPoint").position);
            }
        }
    }

    public string ForEditor_GetState()
    {
        if (state == null)
        {
            return "";
        }
        return state.ToString();
    }

    public string ForEditor_GetRelationWithEnv()
    {
        if (!(this.state is FlyState))
        {
            return "";
        }
        
        var state = this.state as FlyState;
        if (state.ForEditor_GetFrameCountSinceEnv() == 0)
        {
            return "NotInEnv";
        }
        else
        {
            return "InEnv, count " + state.ForEditor_GetFrameCountSinceEnv();
        }
    }


    public Vector3 ForEditor_GetDirection()
    {
        return direction;
    }
#endregion
}
#endif