using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Ludo.TwoHandsWar.Circumstance
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField]private AudioSetting setting;
        static private AudioManager singleton;

        private void Awake()
        {
            setting = Resources.Load<AudioSetting>("AudioSetting");
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void Born()
        {
            var gameObject = new GameObject();
            gameObject.name = "AudioManager";
            gameObject.AddComponent<AudioManager>();
        }

        public static void Play(string audioName)
        {
            if (!singleton.setting.soundEffect.ContainsKey(audioName))
            {
                throw new System.Exception("No AudioName");
            }
            UnityObject clip = singleton.setting.soundEffect[audioName];


            if (clip is AudioClip)
            {
                AudioPool.PlayClip(clip as AudioClip);
            }
            else if (clip is Ludo.Types.AudioClipPlus)
            {
                AudioPool.PlayClip(clip as Ludo.Types.AudioClipPlus);
            }
            else
            {
                throw new System.Exception("Type Wrong");
            }
        }

    }
}