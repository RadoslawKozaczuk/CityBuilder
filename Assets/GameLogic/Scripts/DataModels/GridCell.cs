using Assets.GameLogic.Interfaces;
using UnityEngine;

namespace Assets.GameLogic.DataModels
{
    public struct GridCell
    {
        #region Properties
        public bool IsOccupied => MapObject != null;
        public bool IsOccupiedByBuilding => MapObject != null && MapObject is Building;
        public bool IsOccupiedByVehicle => MapObject != null && MapObject is Vehicle;
        #endregion

        /// <summary>
        /// The object that occupies that cell. It may be anything from a car, through a tree to a building.
        /// </summary>
        public IMapObject MapObject { get; internal set; }

        public readonly Vector2Int Coordinates;

        public GridCell(int x, int y)
        {
            Coordinates = new Vector2Int(x, y);
            MapObject = null;
        }
    }
}

