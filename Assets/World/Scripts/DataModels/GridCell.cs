using Assets.World.Interfaces;
using UnityEngine;

namespace Assets.World.DataModels
{
    public struct GridCell
    {
        // for convenience
        public bool IsOccupied => MapObject != null;
        public bool IsOccupiedByBuilding => MapObject != null && MapObject is Building;
        public bool IsOccupiedByVehicle => MapObject != null && MapObject is Vehicle;

        public IMapObject MapObject { get; internal set; }

        public readonly Vector2Int Coordinates;

        public GridCell(int x, int y)
        {
            Coordinates = new Vector2Int(x, y);
            MapObject = null;
        }
    }
}

