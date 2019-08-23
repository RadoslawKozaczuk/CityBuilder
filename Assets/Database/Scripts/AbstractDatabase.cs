using Assets.Database.DataModels;

namespace Assets.Database
{
    public abstract class AbstractDatabase
    {
        // custom indexers for convenience
        public BuildingData this[BuildingType type] => _buildings[(int)type];

        public BuildingData this[int id] => _buildings[id];

        // monostate pattern - many instances, one data - this is to imitate real db
        protected static BuildingData[] _buildings = null;
    }
}
