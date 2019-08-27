using UnityEngine;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    class ExplosionManager : MonoBehaviour
    {
        static ExplosionManager _instance;

        [SerializeField] GameObject _explosionPrefab;

        void Awake() => _instance = this;

        public static void SpawnRandomExplosion()
        {
            // instantiate random explosion
            GameObject explosion = Instantiate(_instance._explosionPrefab);
            explosion.transform.position = new Vector3(
                Random.Range(0f, 80f), // TODO: make proportional to grid
                Random.Range(5f, 10f),
                Random.Range(0f, 80f));

            var script = explosion.GetComponent<Explosion>();

            // we make a separate copy of a material for each instance to apply slightly different parameters
            Material material = new Material(script.MeshRenderer.sharedMaterial);
            material.SetFloat("_RampOffset", Random.Range(-0.25f, -0.15f));
            material.SetFloat("_Amount", Random.Range(0.7f, 0.9f));
            material.SetFloat("_TimeOffset", GetTimeOffset());

            // attach the copy back to the renderer
            script.MeshRenderer.material = material;
        }

        /// <summary>
        /// Returns the explosion time animation offset in seconds.
        /// </summary>
        static float GetTimeOffset()
        {
            float totalTime = Time.time;
            float animDur = 6.28f; // 2pi = 6.28rad = 6.28sec
            float c = Mathf.Floor(totalTime / animDur);
            float timeOfsset = totalTime - c * animDur;

            return timeOfsset;
        }
    }
}
