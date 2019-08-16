using Assets.Scripts.DataModels;

namespace Assets.Scripts
{
    public class GridCell
    {
        public bool IsOccupied { get => Building != null; }

        public Building Building;
        public int X, Y;
    }
}

