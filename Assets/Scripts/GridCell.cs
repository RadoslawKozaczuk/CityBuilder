using Assets.Scripts.DataModels;
using UnityEngine;

namespace Assets.Scripts
{
    public struct GridCell
    {
        public bool IsOccupied { get => Building != null; }

        public Building Building;
        public Vector2Int Coordinates;
    }
}

