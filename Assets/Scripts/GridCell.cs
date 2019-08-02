namespace Assets.Scripts
{
    public struct GridCell
    {
        public bool IsOccupied { get => Building != null; }

        public Building Building;
        public int X, Y;
    }
}

