using Assets.World.DataModels;
using UnityEngine;

namespace Assets.World.Controllers
{
    // this is a vehicle controller
    public class VehicleController : MonoBehaviour
    {
        const float OUTLINE_VISIBLE_VALUE = 0.5f;
        const float OUTLINE_NOT_VISIBLE_VALUE = 0.0f;

        [SerializeField] Renderer _meshRenderer;
        internal Vehicle Vehicle;

        void Awake()
        {
            _meshRenderer.material = new Material(_meshRenderer.sharedMaterial);
            TurnOutlineOff();
        }

        public void TurnOutlineOn()
        {
            _meshRenderer.material.SetFloat("_OutlineWidth", OUTLINE_VISIBLE_VALUE);
        }

        public void TurnOutlineOff()
        {
            _meshRenderer.material.SetFloat("_OutlineWidth", OUTLINE_NOT_VISIBLE_VALUE);
        }
    }
}
