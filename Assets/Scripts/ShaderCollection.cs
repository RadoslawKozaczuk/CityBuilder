using UnityEngine;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public sealed class ShaderCollection : MonoBehaviour
    {
        static ShaderCollection _instance;
        readonly Shader[] _commonShaders = new Shader[3];

        void Awake() => _instance = this;

        void Start()
        {
            _commonShaders[(int)CommonShaders.Standard] = Shader.Find("Standard");
            _commonShaders[(int)CommonShaders.OutlineAlways] = Shader.Find("Custom/Outline_Always");
            _commonShaders[(int)CommonShaders.OutlineNormal] = Shader.Find("Custom/Outline_Normal");
        }

        public static Shader GetShader(CommonMaterials type) => _instance._commonShaders[(int)type];
    }
}
