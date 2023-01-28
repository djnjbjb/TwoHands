using UnityEngine;

namespace Ludo.TwoHandsWar.Utilities.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AudioClipPlus", menuName = "ScriptableObjects/AudioClipPlus", order = 1)]
    public class AudioClipPlusScriptable : ScriptableObject
    {
        public AudioClip clip;
        public float pitch = 1;
        public float volume = 1;

        public AudioClipPlusScriptable(AudioClip clip, float pitch = 1, float volume = 1)
        {
            this.clip = clip;
            this.pitch = pitch;
            this.volume = volume;
        }

        public AudioClipPlusScriptable()
        {
            clip = null;
            pitch = 1;
            volume = 1;
        }

        public static implicit operator AudioClipPlusScriptable(AudioClip clip) => new AudioClipPlusScriptable(clip);
    }
}

