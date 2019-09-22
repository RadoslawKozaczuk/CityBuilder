using Assets.Database;
using Assets.GameLogic.Interfaces;
using Assets.GameLogic.Tasks;
using UnityEngine;

namespace Assets.GameLogic.DataModels
{
    // this is a Vehicle model
    internal sealed class Vehicle : MonoBehaviour, IMapObject
    {
        const float OUTLINE_VISIBLE_VALUE = 0.5f;
        const float OUTLINE_NOT_VISIBLE_VALUE = 0.0f;

        #region Properties
        internal VehicleType Type { get; private set; }

        /// <summary>
        /// Game map's coordinates.
        /// </summary>
        internal Vector2Int Position { get; set; }

        bool _selected = false;
        internal bool Selected
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
        #endregion

        [HideInInspector] internal float Speed; // for now not readonly, we will see if we want to change it 
        [SerializeField] Renderer _meshRenderer;
        internal MoveTask ScheduledTask; // for now let's assume vehicle can only be given a move task

        void Awake()
        {
            _meshRenderer.material = new Material(_meshRenderer.sharedMaterial);
            TurnOutlineOff();
        }

        internal void SetData(VehicleType type, Vector2Int position)
        {
            Type = type;
            Position = position;
            Speed = GameMap.DB[type].Speed;
            transform.position = GameMap.GetMiddlePointWithOffset(position, type);
        }

        internal void ToggleSelection() => Selected = !Selected;

        internal void TurnOutlineOn() => _meshRenderer.material.SetFloat("_OutlineWidth", OUTLINE_VISIBLE_VALUE);

        internal void TurnOutlineOff() => _meshRenderer.material.SetFloat("_OutlineWidth", OUTLINE_NOT_VISIBLE_VALUE);
    }
}
