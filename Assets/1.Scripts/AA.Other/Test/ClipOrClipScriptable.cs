using UnityEngine;
using NaughtyAttributes;
using Ludo.Types;

public class ClipOrClipScriptable : MonoBehaviour
{
    public AudioClip clip;
    [Label("ClipScriptable")]
    public AudioClipPlus clipScriptable;
    [Label("Clip or ClipScriptable")]
    public Object clipOrClipScriptable;
}
