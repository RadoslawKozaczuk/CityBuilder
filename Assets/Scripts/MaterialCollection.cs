using UnityEngine;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public sealed class MaterialCollection : MonoBehaviour
    {
        static MaterialCollection _instance;
        [SerializeField] Material[] _commonMaterials;

        void Awake() => _instance = this;

        public static Material GetMaterial(CommonMaterial type) => _instance._commonMaterials[(int)type];
    }
}
