using UnityEngine;

namespace Assets.Database
{
    [DisallowMultipleComponent]
    public sealed class BuildingPrefabCollection : MonoBehaviour
    {
        // custom indexers for convenience
        public GameObject this[BuildingType type] => _buildings[(int)type];

        public GameObject this[int id] => _buildings[id];

        [Header("Prefabs must match the values in BuildType enum.")]
        [SerializeField] GameObject[] _buildings;
    }
}
