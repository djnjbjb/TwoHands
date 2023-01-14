using UnityEngine;

public partial class ShurikenExplode : MonoBehaviour
{
    public enum DirectionType
    {
        AsPlaced,
        ToPlayer,
        ToTargetPoint
    }

    public enum StartType
    {
        Auto,
        Trigger
    }
}
