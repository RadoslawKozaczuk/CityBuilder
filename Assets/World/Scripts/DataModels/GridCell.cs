using UnityEngine;

namespace Assets.World.DataModels
{
    public struct GridCell
    {
        // for convenience
        public bool IsOccupied { get => Building != null; }

        public Building Building { get; internal set; }

        public readonly Vector2Int Coordinates;

        public GridCell(int x, int y)
        {
            Coordinates = new Vector2Int(x, y);
            Building = null;
        }
    }
}

