using UnityEngine;

namespace Ludo.Types
{
    [CreateAssetMenu(fileName = "AudioClipPlus", menuName = "ScriptableObjects/AudioClipPlus", order = 1)]
    public class AudioClipPlus : ScriptableObject
    {
        public AudioClip clip;
        public float pitch = 1;
        public float volume = 1;

        static public AudioClipPlus Create(AudioClip clip, float pitch = 1, float volume = 1)
        {
            AudioClipPlus clipPlus = CreateInstance<AudioClipPlus>();
            clipPlus.clip = clip;
            clipPlus.pitch = 1;
            clipPlus.volume = 1;
            return clipPlus;
        }
    }
}

