using Assets.Database;
using Assets.World.Interfaces;
using UnityEngine;

namespace Assets.World.DataModels
{
    // this is a Vehicle model
    public sealed class Vehicle : MonoBehaviour, IMapObject
    {
        const float OUTLINE_VISIBLE_VALUE = 0.5f;
        const float OUTLINE_NOT_VISIBLE_VALUE = 0.0f;

        [SerializeField] Renderer _meshRenderer;

        public VehicleType Type;
        /// <summary>
        /// Game map's coordinates.
        /// </summary>
        public Vector2Int Position { get; internal set; }

        bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                GameMap.Instance.SelectedVehicle = value ? (this) : null; // for now only one can be selected

                if (value)
                    TurnOutlineOn();
                else
                    TurnOutlineOff();
            }
        }

        [HideInInspector] public float Speed; // for now not readonly we will see it we want to change it 

        public void SetData(VehicleType type, Vector2Int position)
        {
            Type = type;
            Position = position;

            Speed = GameMap.DB[type].Speed;
            transform.position = GameMap.GetMiddlePointWithOffset(position, type);
        }

        public void ToggleSelection() => Selected = !Selected;

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
