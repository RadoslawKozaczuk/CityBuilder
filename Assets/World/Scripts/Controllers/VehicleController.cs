using Assets.World.DataModels;
using UnityEngine;

namespace Assets.World.Controllers
{
    public class VehicleController : MonoBehaviour
    {
        const float OUTLINE_VISIBLE_VALUE = 0.5f;
        const float OUTLINE_NOT_VISIBLE_VALUE = 0.0f;

        public Renderer MeshRenderer;
        internal Vehicle Vehicle;

        public void SelectMe()
        {
            Vehicle.Selected = !Vehicle.Selected;
            MeshRenderer.material.SetFloat("_OutlineWidth", Vehicle.Selected ? OUTLINE_VISIBLE_VALUE : OUTLINE_NOT_VISIBLE_VALUE);
        }
    }
}
