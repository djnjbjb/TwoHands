using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Ludo.Extensions;

public partial class Shuriken : MonoBehaviour
{
    /*
        文档：
        https://perfect-dive-450.notion.site/Shuriken-80d1294873f24e908ba5f30d348cc8f3
        从Dynamic Rigidbody的角度，去认识Shuriken。
        在show之前，Shuriken是纯表现。
        之后，一直是dynamic rigidbody。

        我这里，
            把整个物理环节，叫Physics Update。
            Physics Update的第一个环节是Fixed Update。
            处理物理的那个环节，叫Internal Physics。
            之后有OnTrigger、OnCollision。
            最后是yield return new WaitForFixedUpdate()。
        这样，看问题时，可以从整个Physics Update的角度去看，选择忽视一些不重要的细节。
    
        Shuriken只和Stuff发生碰撞。
        它的运动，在internal physics，我无法控制。
            这和之前的一些代码不一样。之前的一些代码，我全部控制。
        但可以预测。只和Stuff发生碰撞。如果没有碰撞，就是按照预定速度运动了。如果发生了碰撞，我可以知道，也可以获得信息。
        如果我对internal physics的运动不满意，我可以在yield的部分修改。也可以在下一个FixedUpdate修改。但都有些不直观。在yield修改相对来说还好一些。
        
        现在的情景是，除了和Stuff的碰撞，1. 场景里还有墙壁，希望可以使物体减速，2. 还有玩家，碰撞后减速或停止移动（结束游戏）。
        但是这些，都无法影响internal physics部分。因为我无法控制。
        
        决定从internal physics部分进行切分。
        或者说，把和Stuff的碰撞，作为最高层划分树的东西，然后逐渐划分。
        剑的移动速度在10左右，Shuriken也在10左右。碰撞后，Shuriken速度最大可到30，超出了范围。
        但，单纯的碰撞那一帧，影响至少看起来还不大，不作处理。只是在下一帧对速度进行处理即可。
        物体在和Shuriken碰撞前是某个状态，在和Shuriken碰撞后可能切换成另一个状态。用这种方式来处理，似乎就可以。

        这样是从整个过程的角度考虑。没能很好地分成几个独立的系统。
        不过，目前看起来只能这样。我也感到，现在做有些事，先到细节，做出来。做了之后，再逐渐总结规律。
        
        
    */
    public enum DirectionType
    {
        AsPlaced,
        ToPlayer,
        ToTargetPoint
    }

    //Setting
    [SerializeField] DirectionType directionType;
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

    void ChangeStateOnStart()
    {
        _state = new BeforeShowState(this,
            beforeShowIdleTime, beforeShowFlyTime,
            beforeShowIdleDistance, direction, rigidbody2D);
        _state.StateStart();
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
        ChangeStateOnStart();

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
