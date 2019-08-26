using Assets.Database;
using Assets.World.Controllers;
using UnityEngine;

namespace Assets.World.DataModels
{
    internal sealed class Vehicle : IMapObject
    {
        public readonly VehicleType Type;
        public GameObject Instance { get; }

        internal bool Selected;

        internal Vehicle(VehicleType type, Vector2Int position)
        {
            Type = type;

            var instance = Object.Instantiate(GameMap.BuildingPrefabCollection[type]);

            // we have make a separate copy of a material for each instance to apply slightly different parameters
            var vc = instance.GetComponent<VehicleController>();
            vc.MeshRenderer.material = new Material(vc.MeshRenderer.sharedMaterial);
            vc.Vehicle = this;

            // put in a correct place on the map
            instance.transform.position = GameMap.GetMiddlePointWithOffset(position, type);

            Instance = instance;
        }
    }
}
