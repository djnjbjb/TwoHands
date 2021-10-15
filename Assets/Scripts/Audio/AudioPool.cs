using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    List<AudioSource> availableAudio = new List<AudioSource>();
    List<AudioSource> unavailableAudio = new List<AudioSource>();
    static AudioPool instance = null;

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < 10; i++)
        {
            var obj = new GameObject($"AudioSource{i}");
            var audio = obj.AddComponent<AudioSource>();
            obj.transform.parent = this.gameObject.transform;
            availableAudio.Add(audio);
        }
        StartCoroutine(RefreshAvailableAudio());
    }

    IEnumerator RefreshAvailableAudio()
    {
        while (true)
        {
            for (int i = 0; i < unavailableAudio.Count; /**/ )
            {
                var audio = unavailableAudio[i];
                if (!audio.isPlaying)
                {
                    unavailableAudio.RemoveAt(i);
                    availableAudio.Add(audio);
                }
                else
                {
                    i++;
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    public static void PlayClip(TwoHandsWar.AudioClipPlus clip)
    {
        PlayClip(clip.clip, clip.pitch, clip.volume);
    }

    public static void PlayClip(AudioClip clip, float pitch = 1, float volume = 1)
    {
        if (instance == null)
        {
            throw new Exception("AudioPool no instance.");
        }

        if (instance.availableAudio.Count == 0)
        {
            throw new Exception("AudioPool exhausted.");
        }

        var audio = instance.availableAudio[0];
        instance.availableAudio.RemoveAt(0);
        instance.unavailableAudio.Add(audio);

        audio.clip = clip;
        audio.pitch = pitch;
        audio.volume = volume;
        audio.Play();
    }

    private void Update()
    {
        string available = "";
        foreach (var a in availableAudio)
        {
            available += (a.name + ",");
        }
        Y.DebugPanel.Log(_name: "【⭐Available】", _message: available, _category: "AudioPool");

        string unavailable = "";
        foreach (var a in unavailableAudio)
        {
            unavailable += (a.name + ",");
        }

        Y.DebugPanel.Log(_name: "【⭐UnAvailable】", _message: unavailable, _category: "AudioPool");
    }

    #region Test
    /*
    private void OnGUI()
    {
        if (GUI.Button(new Rect(200, 200, 100, 100), "P"))
        {
            PlayClip(HCAudio2.instance.acShurikenWallCollision, 1);
        }
    }
    */
    #endregion
}
