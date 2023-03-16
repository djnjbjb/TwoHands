using UnityEngine;
using Ludo.Extensions;

namespace Ludo.TwoHandsWar.UI.Tutorial
{
    public class JumpTip : MonoBehaviour
    {
        [System.NonSerialized] public GameObject contentWindow;
        public void Start()
        {
            contentWindow = transform.LudoFind("Content", includeInactive: true).gameObject;
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
}