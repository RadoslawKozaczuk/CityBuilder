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
                Cost = new List<Resource> { new Resource(ResourceTypes.Gold, 100) } },

            new BuildingData {
                Name = "Wood production building",
                SizeX = 2,
                SizeY = 2,
                Cost = new List<Resource> { new Resource(ResourceTypes.Gold, 150) } },

            new BuildingData {
                Name = "Steel production building",
                SizeX = 3,
                SizeY = 2,
                Cost = new List<Resource>() {
                    new Resource(ResourceTypes.Gold, 150),
                    new Resource(ResourceTypes.Wood, 100) } },

            new BuildingData {
                Name = "Tree",
                SizeX = 2,
                SizeY = 2,
                Cost = new List<Resource>() {
                    new Resource(ResourceTypes.Gold, 150),
                    new Resource(ResourceTypes.Iron, 50) } },

            new BuildingData {
                Name = "Bench",
                SizeX = 1,
                SizeY = 1,
                Cost = new List<Resource>() {
                    new Resource(ResourceTypes.Gold, 50),
                    new Resource(ResourceTypes.Wood, 200) } }
        };
    }
}
