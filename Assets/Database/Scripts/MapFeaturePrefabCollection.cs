using UnityEngine;

namespace Assets.Database
{
    [DisallowMultipleComponent]
    public sealed class MapFeaturePrefabCollection : MonoBehaviour
    {
        // custom indexers for convenience
        public GameObject this[BuildingType type] => _buildings[(int)type];
        public GameObject this[VehicleType type] => _vehicles[(int)type];

        [Header("Prefabs must match the order of the values in the corresponding enumerator.")]
        [SerializeField] GameObject[] _buildings;
        [SerializeField] GameObject[] _vehicles;
    }
}
