using UnityEngine;

namespace Assets.Scripts
{
    class Explosion : MonoBehaviour
    {
        const float LIFE_TIME = 10f;

        public Material Material;

        [SerializeField] ParticleSystem _smoke;
        [SerializeField] Light _pointLight;

        float _lifeTime = 0;
        float _climpRange = 1f;

        void Update()
        {
            _lifeTime += Time.deltaTime;

            _climpRange -= Time.deltaTime / 4f;
            Material.SetFloat("_ClipRange", _climpRange);

            if (_lifeTime > LIFE_TIME)
                Destroy(gameObject);
        }
    }
}
