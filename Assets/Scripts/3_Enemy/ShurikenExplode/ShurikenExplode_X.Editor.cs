using UnityEngine;
using NaughtyAttributes;
public partial class ShurikenExplode : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            Gizmos.color = new Color(1, 1, 1, 1f);
            if (directionType == DirectionType.AsPlaced)
            {
                direction = transform.Find("Front").position - transform.Find("Back").position;
                Gizmos.DrawLine(transform.position, transform.position + direction * 10);
            }
            if (directionType == DirectionType.ToTargetPoint)
            {
                Gizmos.DrawLine(transform.position, transform.Find("TargetPoint").position);
            }
        }
    }

    public string ForEditor_GetState()
    {
        if (state == null)
        {
            return "";
        }
        return state.ToString();
    }

    public Vector3 ForEditor_GetDirection()
    {
        return direction;
    }

    [ShowNativeProperty]
    public Bounds Bounds_Editor
    {
        get
        {
            if (Application.isPlaying)
            {
                return bounds;
            }
            else
            {
                return new Bounds(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            }
        }
    }
}
