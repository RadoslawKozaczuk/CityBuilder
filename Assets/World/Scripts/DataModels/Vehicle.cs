using Assets.Database;
using Assets.World.Controllers;
using Assets.World.Interfaces;
using UnityEngine;

namespace Assets.World.DataModels
{
    // this is a Vehicle model
    public sealed class Vehicle : IMapObject
    {
        public readonly VehicleType Type;
        /// <summary>
        /// Game map's coordinates.
        /// </summary>
        public Vector2Int Position { get; internal set; }
        public GameObject Instance { get; }
        public VehicleController Controller;

        bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                GameMap.Instance.SelectedVehicle = value ? (this) : null; // for now only one can be selected

                if (value)
                    Controller.TurnOutlineOn();
                else
                    Controller.TurnOutlineOff();
            }
        }

        internal Vehicle(VehicleType type, Vector2Int position)
        {
            Type = type;
            Position = position;

            var instance = Object.Instantiate(GameMap.MapFeaturePrefabCollection[type]);

            // we have make a separate copy of a material for each instance to apply slightly different parameters
            var vc = instance.GetComponent<VehicleController>();
            Controller = vc;
            vc.Vehicle = this;

            // put in a correct place on the map
            instance.transform.position = GameMap.GetMiddlePointWithOffset(position, type);

            Instance = instance;
        }

        public void ToggleSelection() => Selected = !Selected;
    }
}
