using UnityEngine;
using Plugins.SerializedCollections;
using UnityObject = UnityEngine.Object;

namespace Ludo.TwoHandsWar.Circumstance
{
    [CreateAssetMenu(fileName = "AudioSetting", menuName = "ScriptableObjects/AudioSetting", order = 2)]
    public class AudioSetting : ScriptableObject
    {
        //UnityEngine.Object 必须是 AudioClip 或 Ludo.Types.AudioClipPlus类型
        public SerializedDictionary<string, UnityEngine.Object> music;
        public SerializedDictionary<string, UnityEngine.Object> Sfx;
        public SerializedDictionary<string, UnityEngine.Object> soundEffect;


    }
}