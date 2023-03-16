using UnityEngine;
using Ludo.Extensions;

public class ShurikenTrigger : MonoBehaviour
{
    [SerializeField] private ShurikenExplode[] shurikens;
    bool triggered = false;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        int layer = collider.gameObject.layer;
        int layerMask = LayerMaskExtension.GetMaskFromInt(layer);

        if (layerMask == LayerMask.GetMask("Player"))
        {
            FireShuriken();
        }
    }

    void FireShuriken()
    {
        if (!triggered)
        {
            foreach (var shuriken in shurikens)
            {
                shuriken.FireByTrigger();
            }
            triggered = true;
        }
        
    }
}
