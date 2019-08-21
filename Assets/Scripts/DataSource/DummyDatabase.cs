using Assets.Scripts.DataModels;
using System.Collections.Generic;

namespace Assets.Scripts.DataSource
{
    public class DummyDatabase : AbstractDatabase
    {
        public BuildingData[] AllBuildings => _buildings;

        public DummyDatabase()
        {
            // monostate pattern - many instances, one data - this is to imitate real db
            if (_buildings != null)
                return;

            _buildings = new BuildingData[]
            {
                new BuildingData
                {
                    Name = "Residence",
                    SizeX = 3, SizeY = 3,
                    Cost = new List<Resource> { new Resource(ResourceType.Gold, 100) },
                    Type = BuildingType.Residence,
                    ResourceProductionData = new ResourceProductionData(new Resource(ResourceType.Gold, 50), 10f, true, true),
                    AbleToReallocate = false,
                    ReallocationCost = null
                },

                new BuildingData
                {
                    Name = "Wood production building",
                    SizeX = 3,
                    SizeY = 2,
                    Cost = new List<Resource> { new Resource(ResourceType.Gold, 150) },
                    Type = BuildingType.WoodProduction,
                    ResourceProductionData = new ResourceProductionData(new Resource(ResourceType.Wood, 50), 10f, false, false),
                    AbleToReallocate = false,
                    ReallocationCost = null
                },

                new BuildingData
                {
                    Name = "Steel production building",
                    SizeX = 2,
                    SizeY = 2,
                    Cost = new List<Resource>() {
                        new Resource(ResourceType.Gold, 150),
                        new Resource(ResourceType.Wood, 100) },
                    Type = BuildingType.SteelProduction,
                    ResourceProductionData = new ResourceProductionData(new Resource(ResourceType.Iron, 50), 10f, false, false),
                    AbleToReallocate = false,
                    ReallocationCost = null
                },

                new BuildingData
                {
                    Name = "Tree",
                    SizeX = 1,
                    SizeY = 1,
                    Cost = new List<Resource>() {
                        new Resource(ResourceType.Gold, 150),
                        new Resource(ResourceType.Iron, 50) },
                    Type = BuildingType.Tree,
                    AbleToReallocate = true,
                    ReallocationCost = new Resource(ResourceType.Gold, 50)
                },

                new BuildingData
                {
                    Name = "Bench",
                    SizeX = 1,
                    SizeY = 1,
                    Cost = new List<Resource>() {
                        new Resource(ResourceType.Gold, 50),
                        new Resource(ResourceType.Wood, 200) },
                    Type = BuildingType.Bench,
                    AbleToReallocate = true,
                    ReallocationCost = new Resource(ResourceType.Gold, 50)
                }
            };
        }
    }
}
