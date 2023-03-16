using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using Ludo.Types;

namespace Ludo.TwoHandsWar.Circumstance
{
    public class AudioPool : MonoBehaviour
    {
        List<AudioSource> available = new List<AudioSource>();
        List<AudioSource> inUse = new List<AudioSource>();
        int poolCount = 20;

        #region Unity Message
        private void Awake()
        {
            SetSingleton();
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < poolCount; i++)
            {
                var obj = new GameObject($"AudioSource{i}");
                var audio = obj.AddComponent<AudioSource>();
                obj.transform.parent = gameObject.transform;
                available.Add(audio);
            }
            StartCoroutine(UpdatePool());
        }

        private void Update()
        {
            string available = "";
            foreach (var a in this.available)
            {
                available += a.name + ",";
            }
            Y.DebugPanel.Log(_name: "【⭐Available】", _message: available, _category: "AudioPool");

            string unavailable = "";
            foreach (var a in inUse)
            {
                unavailable += a.name + ",";
            }

            Y.DebugPanel.Log(_name: "【⭐UnAvailable】", _message: unavailable, _category: "AudioPool");
        }
        #endregion

        IEnumerator UpdatePool()
        {
            while (true)
            {
                for (int i = 0; i < inUse.Count;)
                {
                    var audio = inUse[i];
                    if (!audio.isPlaying)
                    {
                        inUse.RemoveAt(i);
                        available.Add(audio);
                    }
                    else
                    {
                        i++;
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        #region Static 
        static AudioPool singleton;

        public void SetSingleton()
        {
            singleton = this;
        }
        #endregion

        #region Public
        public static void Born()
        {
            var gameObject = new GameObject();
            gameObject.name = "AudioPool";
            gameObject.AddComponent<AudioPool>();
        }

        public static void PlayClip(AudioClipPlus clip)
        {
            PlayClip(clip.clip, clip.pitch, clip.volume);
        }

        public static void PlayClip(AudioClip clip, float pitch = 1, float volume = 1)
        {
            if (singleton == null)
            {
                throw new Exception("No Singleton");
            }

            if (singleton.available.Count == 0)
            {
                throw new Exception("AudioPool exhausted.");
            }

            var audio = singleton.available[0];
            singleton.available.RemoveAt(0);
            singleton.inUse.Add(audio);

            audio.clip = clip;
            audio.pitch = pitch;
            audio.volume = volume;
            audio.Play();
        }
        #endregion


    }
}