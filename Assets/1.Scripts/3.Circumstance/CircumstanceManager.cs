using UnityEngine;

namespace Ludo.TwoHandsWar.Circumstance
{
    public class CircumstanceManager : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeInitialize()
        {
            SettingNamespace.Setting.Born();
            Log.Logger.Born();
            PlayerInput.UniformInput.Born();
        }

    }
}