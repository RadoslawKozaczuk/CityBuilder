using Assets.Database.DataModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Database
{
    // as the name suggest this class imitates dummy database with caching turned on
    // caching gives us random access without any extra cost
    internal class DummyCachedDB : AbstractDatabase
    {
        internal DummyCachedDB()
        {
            // monostate pattern - many instances, one data - this is to imitate real db
            if (_buildingsTable == null)
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
            }

            if (_vehiclesTable == null)
                _vehiclesTable = new VehicleData[] { new VehicleData(new Vector2Int(1, 1), VehicleType.Truck) };
        }

        internal override int CreateBuilding(BuildingData data) => throw new NotImplementedException();
        internal override BuildingData ReadBuilding(int id) => _buildingsTable[id];
        internal override IEnumerable<BuildingData> ReadBuildings(Func<BuildingData, bool> condition) 
            => _buildingsTable.Where(b => condition(b));
        internal override void UpdateBuilding(int id, BuildingData data) => throw new NotImplementedException();
        internal override void DeleteBuilding(int id) => throw new NotImplementedException();

        internal override int CreateVehicle(VehicleData data) => throw new NotImplementedException();
        internal override VehicleData ReadVehicle(int id) => _vehiclesTable[id];
        internal override IEnumerable<VehicleData> ReadVehicles(Func<VehicleData, bool> condition)
            => _vehiclesTable.Where(v => condition(v));
        internal override void UpdateVehicle(int id, VehicleData data) => throw new NotImplementedException();
        internal override void DeleteVehicle(int id) => throw new NotImplementedException();
    }
}
