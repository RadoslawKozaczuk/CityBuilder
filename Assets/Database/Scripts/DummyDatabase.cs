using Assets.Database.DataModels;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Database
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
    }
}
