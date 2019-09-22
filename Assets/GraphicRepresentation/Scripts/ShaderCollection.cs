using UnityEngine;

namespace Assets.GraphicRepresentation
{
    [DisallowMultipleComponent]
    public sealed class ShaderCollection : MonoBehaviour
    {
        static ShaderCollection _instance;
        readonly Shader[] _commonShaders = new Shader[3];

        void Awake() => _instance = this;

        void Start()
        {
            _commonShaders[(int)CommonShader.Standard] = Shader.Find("Standard");
            _commonShaders[(int)CommonShader.OutlineAlways] = Shader.Find("Custom/Outline_Always");
            _commonShaders[(int)CommonShader.OutlineNormal] = Shader.Find("Custom/Outline_Normal");
        }

        public static Shader GetShader(CommonMaterial type) => _instance._commonShaders[(int)type];
    }
}
