using UnityEngine;

namespace Assets.Scripts
{
    class MaterialManager : MonoBehaviour
    {
        public static MaterialManager Instance { get; private set; }

        [SerializeField] Material[] _commonMaterials;

        void Awake() => Instance = this;

        public Material GetMaterial(CommonMaterialType type) => _commonMaterials[(int)type];
    }
}
