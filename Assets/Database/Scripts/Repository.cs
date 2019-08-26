using Assets.Database.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Database
{
    // repository encapsulates data base
    // it may also add some logic if necessary
    // this extra logic may create some data transformation
    public class Repository
    {
        // custom indexers for convenience
        public BuildingData this[BuildingType type] => _db.ReadBuilding((int)type);
        public VehicleData this[VehicleType type] => _db.ReadVehicle((int)type);

        //// encapsulated
        public List<BuildingData> AllBuildings => _db.ReadBuildings((BuildingData b) => true).ToList();

        // monostate pattern - many instances, one data - this is to imitate real db
        static AbstractDatabase _db = new DummyCachedDB();
    }
}
