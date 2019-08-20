namespace Assets.Scripts.DataSource
{
    public abstract class AbstractDatabase
    {
        // custom indexers
        public BuildingData this[BuildingType type] { get => _buildings[(int)type]; }

        public BuildingData this[int id] { get => _buildings[id]; }

        // monostate pattern - many instances, one data
        protected static BuildingData[] _buildings = null;
    }
}
