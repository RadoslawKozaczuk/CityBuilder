using Assets.Database;
using UnityEngine;

namespace Assets.World.DataModels
{
    public sealed class Vehicle : MonoBehaviour, IMapObject
    {
        public readonly VehicleType Type;
        public GameObject GameObject { get; set; }

        public Vehicle(VehicleType type, Vector2Int position)
        {
            Type = type;
            GameObject = Instantiate(GameMap.BuildingPrefabCollection[type]);
            GameObject.transform.position = GameMap.GetMiddlePointWithOffset(position, type);
        }
    }
}
