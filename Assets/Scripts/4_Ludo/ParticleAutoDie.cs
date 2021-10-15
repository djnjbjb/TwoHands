using UnityEngine;

public class ParticleAutoDie : MonoBehaviour
{
    private ParticleSystem p = null;
    private void Start()
    {
        p = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (!p.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
