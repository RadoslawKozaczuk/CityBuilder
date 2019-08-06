using Assets.Scripts.DataSource;
using UnityEngine;

namespace Assets.Scripts.DataModels
{
    // this represents building object in the game
    public class Building
    {
        const float ConstructionTime = 10f; // hardcoded

        public string Name;

        // X and Y always point at the left bottom corner of the building
        public int PositionX, PositionY, SizeX, SizeY;
        public BuildingType BuildingType;
        public GameObject GameObjectInstance;
        public bool Finished = false;
        public bool ProductionStarted;

        public Building(int posX, int posY, BuildingType type, BuildingData data, GameObject instance)
        {
            PositionX = posX;
            PositionY = posY;
            SizeX = data.SizeX;
            SizeY = data.SizeY;
            Name = data.Name;
            BuildingType = type;
            GameObjectInstance = instance;
        }
    }
}
