using UnityEngine;

namespace Ludo.Other
{
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
}