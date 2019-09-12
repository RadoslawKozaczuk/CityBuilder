using Assets.Database.DataModels;

namespace Assets.Database
{
    abstract class AbstractDatabase
    {
        // since we have to imitate real-life tables we need an abstract class to not duplicate the code
        protected BuildingData[] _buildingsTable;
        protected VehicleData[] _vehiclesTable;
    }
}
