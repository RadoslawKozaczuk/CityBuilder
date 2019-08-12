using Assets.Scripts.DataModels;
using System.Collections.Generic;

namespace Assets.Scripts.DataSource
{
    public static class BuildingDataSource
    {
        public static BuildingData[] Buildings = new BuildingData[]
        {
            new BuildingData {
                Name = "Residence",
                SizeX = 3, SizeY = 3,
                Cost = new List<Resource> { new Resource(ResourceType.Gold, 100) },
                Type = BuildingType.Residence },

            new BuildingData {
                Name = "Wood production building",
                SizeX = 2,
                SizeY = 2,
                Cost = new List<Resource> { new Resource(ResourceType.Gold, 150) },
                Type = BuildingType.WoodProduction },

            new BuildingData {
                Name = "Steel production building",
                SizeX = 3,
                SizeY = 2,
                Cost = new List<Resource>() {
                    new Resource(ResourceType.Gold, 150),
                    new Resource(ResourceType.Wood, 100) },
                Type = BuildingType.SteelProduction },

            new BuildingData {
                Name = "Tree",
                SizeX = 1,
                SizeY = 1,
                Cost = new List<Resource>() {
                    new Resource(ResourceType.Gold, 150),
                    new Resource(ResourceType.Iron, 50) },
                Type = BuildingType.Tree },

            new BuildingData {
                Name = "Bench",
                SizeX = 1,
                SizeY = 1,
                Cost = new List<Resource>() {
                    new Resource(ResourceType.Gold, 50),
                    new Resource(ResourceType.Wood, 200) },
                Type = BuildingType.Bench }
        };
    }
}
