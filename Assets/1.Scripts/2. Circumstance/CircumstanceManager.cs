using UnityEngine;

namespace Ludo.TwoHandsWar.Circumstance
{
    public class CircumstanceManager : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeInitialize()
        {
            Setting.Born();
            Logger.Born();
            PlayerInput.Born();
            AudioManager.Born();
            AudioPool.Born();
        }

    }
}