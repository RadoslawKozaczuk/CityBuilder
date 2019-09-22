using UnityEngine;

namespace Assets.GraphicRepresentation
{
    class Explosion : MonoBehaviour
    {
        const float LIFE_TIME = 10f;

        public MeshRenderer MeshRenderer;
        [SerializeField] ParticleSystem _smoke;
        [SerializeField] Light _pointLight;

        float _lifeTime = 0;
        float _climpRange = 1f;

        void Update()
        {
            _lifeTime += Time.deltaTime;

            _climpRange -= Time.deltaTime / 4f;
            MeshRenderer.material.SetFloat("_ClipRange", _climpRange);

            if (_lifeTime > LIFE_TIME)
                Destroy(gameObject);
        }
    }
}
