using UnityEngine;

public partial class ShurikenExplode : MonoBehaviour
{
    public void KillPlayerEffect()
    {
        stateController.ChangeStateOnKillPlayer();
    }

    public void FireByTrigger()
    {
        stateController.ChangeStateOnTriggerFire();
    }
}
