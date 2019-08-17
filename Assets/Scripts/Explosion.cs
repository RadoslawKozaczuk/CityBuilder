using UnityEngine;

namespace Assets.Scripts
{
    class Explosion : MonoBehaviour
    {
        const float LIFE_TIME = 5f;

        [SerializeField] ParticleSystem _smoke;
        public Material Material;

        public float ShaderDisappearnceThreshold = 1.5f;

        float _lifeTime = 0;

        void Update()
        {
            _lifeTime += Time.deltaTime;

            if (_lifeTime > ShaderDisappearnceThreshold)
            {
                // temporary solution
                Material.SetFloat("_ClipRange", 0f);
            }

            if (_lifeTime > LIFE_TIME)
            {
                Destroy(gameObject);
            }
        }
    }
}
