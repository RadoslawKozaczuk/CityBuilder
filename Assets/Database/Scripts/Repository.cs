using Assets.Database.DataModels;
using Assets.Database.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Database
{
    // repository encapsulates our database
    // in real life it may also add some extra logic if necessary
    // this extra logic may for example create some data transformation
    public class Repository : IRepository
    {
        // custom indexers for convenience
        public BuildingData this[BuildingType type] => _db.ReadBuilding((int)type);
        public VehicleData this[VehicleType type] => _db.ReadVehicle((int)type);

        // we use dummy condition here as DB area is out of the scope of this project 
        // this is just to show the role of a repository in this particular application design
        public List<BuildingData> AllBuildings => _db.ReadBuildings((BuildingData b) => true).ToList();  // db encapsulation

        // monostate pattern - many instances, one data - this is to imitate real db
        static readonly IDatabase _db = new DummyCachedDB();
    }
}
