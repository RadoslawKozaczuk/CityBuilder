using UnityEngine;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    class MaterialManager : MonoBehaviour
    {
        static MaterialManager _instance;
        [SerializeField] Material[] _commonMaterials;

        void Awake() => _instance = this;

        public static Material GetMaterial(CommonMaterialType type) => _instance._commonMaterials[(int)type];
    }
}
