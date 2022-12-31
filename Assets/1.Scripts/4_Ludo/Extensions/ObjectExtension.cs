using UnityEngine;

namespace Ludo.Extensions
{
    public static class ObjectExtensions
    {
        public static GameObject InstantiateParticleAutoDie(GameObject particle, Vector3 position, Quaternion rotation)
        {
            GameObject gameObject = Object.Instantiate(particle, position, rotation);
            if (gameObject.GetComponent<ParticleSystem>() == null)
            {
                throw new System.Exception("The gameObejct is not a ParticleSystem");
            }
            if (gameObject.GetComponent<ParticleAutoDie>() != null)
            {
                Debug.LogWarning("Component ParticleAutoDie is already on the gameObject");
            }
            else
            {
                gameObject.AddComponent<ParticleAutoDie>();
            }
            return gameObject;
        }
    }
}
