using UnityEngine;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public sealed class MaterialCollection : MonoBehaviour
    {
        static MaterialCollection _instance;
        [SerializeField] Material[] _commonMaterials;

        void Awake() => _instance = this;

        public static Material GetMaterial(CommonMaterials type) => _instance._commonMaterials[(int)type];
    }
}
