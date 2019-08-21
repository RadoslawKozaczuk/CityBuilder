namespace Assets.Scripts.DataSource
{
    public abstract class AbstractDatabase
    {
        // custom indexers for convenience
        public BuildingData this[BuildingType type] { get => _buildings[(int)type]; }

        public BuildingData this[int id] { get => _buildings[id]; }

        // monostate pattern - many instances, one data  - this is to imitate real db
        protected static BuildingData[] _buildings = null;
    }
}
