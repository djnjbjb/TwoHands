using UnityEngine;
using NaughtyAttributes;
using Ludo.TwoHandsWar.Utilities.ScriptableObjects;

public class ClipOrClipScriptable : MonoBehaviour
{
    public AudioClip clip;
    [Label("ClipScriptable")]
    public AudioClipPlusScriptable clipScriptable;
    [Label("Clip or ClipScriptable")]
    public Object clipOrClipScriptable;
}
