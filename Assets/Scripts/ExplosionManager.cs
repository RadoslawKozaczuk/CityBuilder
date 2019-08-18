using UnityEngine;

namespace Assets.Scripts
{
    class ExplosionManager : MonoBehaviour
    {
        public static ExplosionManager Instance { get; private set; }

        [SerializeField] GameObject _explosionPrefab;

        void Awake() => Instance = this;

        // this should be moved to a separate singleton
        public void SpawnRandomExplosion()
        {
            // instantiate random explosion
            GameObject explosion = Instantiate(_explosionPrefab) as GameObject;
            explosion.transform.position = new Vector3(
                Random.Range(0f, 80f), // make proportional to grid
                Random.Range(5f, 10f),
                Random.Range(0f, 80f));

            // we make a separate copy of a material for each instance to apply slightly different parameters
            Renderer renderer = explosion.GetComponent<Renderer>();
            Material material = new Material(renderer.sharedMaterial);

            var script = explosion.GetComponent<Explosion>();
            script.Material = material;

            material.SetFloat("_RampOffset", Random.Range(-0.25f, -0.15f));

            float period = Random.Range(0.75f, 0.85f);
            material.SetFloat("_Period", period);
            script.ShaderDisappearnceThreshold = Utils.Map(1.4f, 1.6f, 0.75f, 0.85f, period);

            material.SetFloat("_Amount", Random.Range(0.7f, 0.9f));
            renderer.material = material;
        }
    }
}
