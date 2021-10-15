using UnityEngine;

public partial class ShurikenExplode : MonoBehaviour
{
    public class StateController
    {
        private State state;
        private ShurikenExplode shuriken;

        public StateController(ShurikenExplode shuriken)
        {
            state = null;
            this.shuriken = shuriken;
        }

        public State GetState()
        {
            return state;
        }

        #region Start，两种
        public void ChangeStateOnTriggerStart()
        {
            state = new BeforeTriggerFire(shuriken,
                shuriken.initPosition, shuriken.beforeShowIdleDistance, shuriken.direction, shuriken.rigidbody2D);
            state.StateStart();
        }

        public void ChangeStateOnAutoStart()
        {
            state = new BeforeShowState(shuriken,
                shuriken.initPosition, shuriken.beforeShowIdleTime, shuriken.beforeShowFlyTime,
                shuriken.beforeShowIdleDistance, shuriken.direction, shuriken.rigidbody2D);
            state.StateStart();
        }
        #endregion


        #region TriggerFire
        public void ChangeStateOnTriggerFire()
        {
            if (state != null && state.ended != true)
            {
                state.StateManuallyEnd();
            }
            state = new BeforeShowState(shuriken,
                shuriken.initPosition, shuriken.beforeShowIdleTime, shuriken.beforeShowFlyTime,
                shuriken.beforeShowIdleDistance, shuriken.direction, shuriken.rigidbody2D);
            (state as BeforeShowState).StateStartByTrigger();
        }
        #endregion

        #region 各种End
        public void ChangeStateOnNaturalEnd()
        {
            State newState = state.NextState();
            state = newState;
            if (state != null) state.StateStart();
        }

        public void ChangeStateOnCollision()
        {
            if (state != null && state.ended != true)
            {
                state.StateManuallyEnd();
            }
            state = new FlyState(shuriken, shuriken.flySpeed, shuriken.direction, shuriken.rigidbody2D);
            (state as FlyState).StateStartOnCollision();
        }

        public void ChangeStateOnKillPlayer()
        {
            if (state != null && state.ended != true)
            {
                state.StateManuallyEnd();
            }
            state = new KillPlayerState(shuriken, shuriken.killPlayerStateGoOnFlyingCount, shuriken.rigidbody2D);
            (state as KillPlayerState).StateStartOnKillPlayer();
        }

        public void ChangeStateOnDestroy()
        {
            if (state != null && state.ended != true)
            {
                state.StateManuallyEnd();
            }
            state = null;
        }
        #endregion
    }
}
