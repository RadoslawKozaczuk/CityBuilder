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
        public bool Constructed = false;
        public bool ProductionStarted;
        public BuildingTask ScheduledTask;
        public bool AbleToReallocate;
        public Resource? ReallocationCost;

        readonly Resource _resource;
        readonly float _productionTime;
        readonly bool _imidiatelyStartProduction;
        readonly bool _loopProduction;

        public Building(int posX, int posY, BuildingData data, GameObject instance)
        {
            PositionX = posX;
            PositionY = posY;
            SizeX = data.SizeX;
            SizeY = data.SizeY;
            Name = data.Name;
            BuildingType = data.Type;
            GameObjectInstance = instance;
            AbleToReallocate = data.AbleToReallocate;
            ReallocationCost = data.ReallocationCost;

            _resource = data.ResourceProductionData.Resource;
            _productionTime = data.ResourceProductionData.ProductionTime;
            _imidiatelyStartProduction = data.ResourceProductionData.StartImidiately;
            _loopProduction = data.ResourceProductionData.Loop;

            // schedule construction task
            BuildingTask task = new BuildingTask(ConstructionTime, FinishConstruction);
            ScheduledTask = task;
            GameEngine.Instance.ScheduleTask(task);
        }

        public void FinishConstruction()
        {
            Constructed = true;

            if (_imidiatelyStartProduction)
                StartProduction();
        }

        public void AddResource()
        {
            ResourceManager.Instance.AddResource(_resource);
            ProductionStarted = false;

            if (_loopProduction)
                StartProduction();
        }

        public void StartProduction()
        {
            // schedule production task
            BuildingTask task = new BuildingTask(_productionTime, AddResource);
            ScheduledTask = task;
            GameEngine.Instance.ScheduleTask(task);
            ProductionStarted = true;
        }
    }
}
