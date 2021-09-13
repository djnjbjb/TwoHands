using System;
using UnityEngine;

public class HCAudio2 : MonoBehaviour
{
    static public AudioSource audioSource;
    static public HCAudio2 instance;
    [SerializeField] public AudioClip acShootArrow;
    [SerializeField] public AudioClip acSwordClash;
    [SerializeField] public AudioClip acDang;
    [SerializeField] public AudioClip acWhoosh;

    private void Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }


}
