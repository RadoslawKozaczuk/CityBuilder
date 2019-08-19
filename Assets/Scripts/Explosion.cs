using UnityEngine;

namespace Assets.Scripts
{
    class Explosion : MonoBehaviour
    {
        const float LIFE_TIME = 5f;

        public Material Material;
        public float ShaderDisappearnceThreshold = 1.5f;

        [SerializeField] ParticleSystem _smoke;
        [SerializeField] Light _pointLight;

        float _lifeTime = 0;

        void Update()
        {
            _lifeTime += Time.deltaTime;

            if (_lifeTime > ShaderDisappearnceThreshold)
            {
                // temporary solution
                Material.SetFloat("_ClipRange", 0f);
                _pointLight.gameObject.SetActive(false);
            }

            if (_lifeTime > LIFE_TIME)
                Destroy(gameObject);
        }
    }
}
