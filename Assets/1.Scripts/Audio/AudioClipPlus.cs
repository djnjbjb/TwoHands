using UnityEngine;

namespace TwoHandsWar
{
    public class AudioClipPlus
    {
        public static AudioClipPlus FromClipOrScriptable(UnityEngine.Object clip)
        {
            AudioClip ac = clip as AudioClip;
            if (ac != null )
            {
                return new AudioClipPlus(ac);
            }
            AudioClipPlusScriptable acs = clip as AudioClipPlusScriptable;
            if (acs != null)
            {
                return new AudioClipPlus(acs);
            }
            return null;
        }

        public static AudioClipPlus FromClipOrScriptableWithExplanation(UnityEngine.Object clip,out bool isNull, out bool isAudioClip, out bool isAudioClipPlusScriptable)
        {
            isNull = false;
            isAudioClip = false;
            isAudioClipPlusScriptable = false;

            if (clip == null)
            {
                isNull = true;
            }
            AudioClip ac = clip as AudioClip;
            if (ac != null)
            {
                isAudioClip = true;
            }
            AudioClipPlusScriptable acs = clip as AudioClipPlusScriptable;
            if (acs != null)
            {
                isAudioClipPlusScriptable = true;
            }
            return FromClipOrScriptable(clip);
        }

        public AudioClip clip;
        public float pitch = 1;
        public float volume = 1;

        public AudioClipPlus(AudioClipPlusScriptable scriptable)
        {
            this.clip = scriptable.clip;
            this.pitch = scriptable.pitch;
            this.volume = scriptable.volume;
        }

        public AudioClipPlus(AudioClip clip, float pitch = 1, float volume = 1)
        {
            this.clip = clip;
            this.pitch = pitch;
            this.volume = volume;
        }

        public AudioClipPlus()
        {
            clip = null;
            pitch = 1;
            volume = 1;
        }

        public static implicit operator AudioClipPlus(AudioClip clip) => new AudioClipPlus(clip);
        public static implicit operator AudioClipPlus(AudioClipPlusScriptable scriptable) => new AudioClipPlus(scriptable);
    }
}
