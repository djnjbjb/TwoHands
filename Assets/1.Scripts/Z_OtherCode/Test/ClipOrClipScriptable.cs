using UnityEngine;
using TwoHandsWar;
using NaughtyAttributes;

public class ClipOrClipScriptable : MonoBehaviour
{
    public AudioClip clip;
    [Label("ClipScriptable")]
    public AudioClipPlusScriptable clipScriptable;
    [Label("Clip or ClipScriptable")]
    public Object clipOrClipScriptable;
}
