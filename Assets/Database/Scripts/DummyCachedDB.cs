using Assets.Database.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Database
{
    /// <summary>
    /// As the name suggest this class imitates a database with caching option turned on.
    /// The assumption that all data can be randomly accessed without extra cost from anywhere in the game 
    /// was necessary to keep the scope of the project realistic.
    /// 
    /// However, dummy objects are still necessary to express the overall architecture of the project, 
    /// and separation of concern of different layers of the application.
    /// 
    /// DummyCachedDB allows us to perform two operations:
    /// - retrieve one element by id
    /// - retrieve all elements that fulfill the given condition
    /// 
    /// Other operations are purposely not implemented as, as mentioned above, 
    /// their functionality would be outside of the scope of this project.
    /// </summary>
    internal sealed class DummyCachedDB : AbstractDatabase, IDatabase
    {
        internal DummyCachedDB()
        {
            _buildingsTable = new BuildingData[]
            {
                new BuildingData(
                    "Residence",
                    new Vector2Int(3, 3),
                    new List<Resource> { new Resource(ResourceType.Gold, 150) },
                    BuildingType.Residence,
                    new ResourceProductionData(new Resource(ResourceType.Gold, 50), 5f, true, true),
                    false,
                    null),

                new BuildingData(
                    "Wood production building",
                    new Vector2Int(3, 2),
                    new List<Resource> { new Resource(ResourceType.Gold, 150) },
                    BuildingType.WoodProduction,
                    new ResourceProductionData(new Resource(ResourceType.Wood, 50), 5f, false, false),
                    false,
                    null),

                new BuildingData(
                    "Steel production building",
                    new Vector2Int(2, 2),
                    new List<Resource>()
                    {
                        new Resource(ResourceType.Gold, 150),
                        new Resource(ResourceType.Wood, 100)
                    },
                    BuildingType.SteelProduction,
                    new ResourceProductionData(new Resource(ResourceType.Iron, 50), 5f, false, false),
                    false,
                    null),

                new BuildingData(
                    "Tree",
                    new Vector2Int(1, 1),
                    new List<Resource>()
                    {
                        new Resource(ResourceType.Gold, 100),
                        new Resource(ResourceType.Iron, 50)
                    },
                    BuildingType.Tree,
                    null,
                    true,
                    new Resource(ResourceType.Gold, 50)),

                new BuildingData(
                    "Bench",
                    new Vector2Int(1, 1),
                    new List<Resource>()
                    {
                        new Resource(ResourceType.Gold, 50),
                        new Resource(ResourceType.Wood, 50)
                    },
                    BuildingType.Bench,
                    null,
                    true,
                    new Resource(ResourceType.Gold, 50))
                };

            _vehiclesTable = new VehicleData[] { new VehicleData(new Vector2Int(1, 1), VehicleType.Truck) };
        }

        public int CreateBuilding(BuildingData data) 
            => throw new NotImplementedException(); // dummy
        
        public BuildingData ReadBuilding(int id) 
            => _buildingsTable[id];
        
        public IEnumerable<BuildingData> ReadBuildings(Func<BuildingData, bool> condition) 
            => _buildingsTable.Where(b => condition(b));
        
        public void UpdateBuilding(int id, BuildingData data) 
            => throw new NotImplementedException(); // dummy

        public void DeleteBuilding(int id) 
            => throw new NotImplementedException(); // dummy

        public int CreateVehicle(VehicleData data) 
            => throw new NotImplementedException(); // dummy

        public VehicleData ReadVehicle(int id) 
            => _vehiclesTable[id];
        
        public IEnumerable<VehicleData> ReadVehicles(Func<VehicleData, bool> condition)
            => _vehiclesTable.Where(v => condition(v));
        
        public void UpdateVehicle(int id, VehicleData data) 
            => throw new NotImplementedException(); // dummy

        public void DeleteVehicle(int id) 
            => throw new NotImplementedException(); // dummy
    }
}
