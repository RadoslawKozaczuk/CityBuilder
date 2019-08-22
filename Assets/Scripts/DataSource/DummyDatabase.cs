using Assets.Scripts.DataModels;
using System.Collections.Generic;
using UnityEngine;

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
                    Size = new Vector2Int(3, 3),
                    Cost = new List<Resource> { new Resource(ResourceType.Gold, 150) },
                    Type = BuildingType.Residence,
                    ResourceProductionData = new ResourceProductionData(new Resource(ResourceType.Gold, 50), 5f, true, true),
                    AbleToReallocate = false,
                    ReallocationCost = null
                },

                new BuildingData
                {
                    Name = "Wood production building",
                    Size = new Vector2Int(3, 2),
                    Cost = new List<Resource> { new Resource(ResourceType.Gold, 150) },
                    Type = BuildingType.WoodProduction,
                    ResourceProductionData = new ResourceProductionData(new Resource(ResourceType.Wood, 50), 5f, false, false),
                    AbleToReallocate = false,
                    ReallocationCost = null
                },

                new BuildingData
                {
                    Name = "Steel production building",
                    Size = new Vector2Int(2, 2),
                    Cost = new List<Resource>() {
                        new Resource(ResourceType.Gold, 150),
                        new Resource(ResourceType.Wood, 100) },
                    Type = BuildingType.SteelProduction,
                    ResourceProductionData = new ResourceProductionData(new Resource(ResourceType.Iron, 50), 5f, false, false),
                    AbleToReallocate = false,
                    ReallocationCost = null
                },

                new BuildingData
                {
                    Name = "Tree",
                    Size = new Vector2Int(1, 1),
                    Cost = new List<Resource>() {
                        new Resource(ResourceType.Gold, 100),
                        new Resource(ResourceType.Iron, 50) },
                    Type = BuildingType.Tree,
                    AbleToReallocate = true,
                    ReallocationCost = new Resource(ResourceType.Gold, 50)
                },

                new BuildingData
                {
                    Name = "Bench",
                    Size = new Vector2Int(1, 1),
                    Cost = new List<Resource>() {
                        new Resource(ResourceType.Gold, 50),
                        new Resource(ResourceType.Wood, 50) },
                    Type = BuildingType.Bench,
                    AbleToReallocate = true,
                    ReallocationCost = new Resource(ResourceType.Gold, 50)
                }
            };
        }
    }
}
