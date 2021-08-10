using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludo.Extensions;

public class JumpTip : MonoBehaviour
{
    [System.NonSerialized] public GameObject contentWindow;
    public void Start()
    {
        contentWindow = this.transform.LudoFind("Content", includeInactive: true).gameObject;
    }

    public void OpenOrClose()
    {
        contentWindow.SetActive(!contentWindow.activeSelf);
    }

    public void Close()
    {
        contentWindow.SetActive(false);
    }
}
