using Assets.Database;
using Assets.Database.DataModels;
using UnityEngine;

namespace Assets.World.DataModels
{
    // this represents building object in the game
    public sealed class Building
    {
        const float CONSTRUCTION_TIME = 5f; // hardcoded for simplicity

        /// <summary>
        /// Position always point at the left bottom corner of the building.
        /// </summary>
        public Vector2Int Position { get; internal set; }

        public Vector2Int Size => GameMap.DB[Type].Size;

        public string Name;
        public BuildingType Type { get; set; }
        public GameObject GameObject { get; set; }
        public bool Constructed = false;
        public bool ProductionStarted;
        public BuildingTask ScheduledTask;
        public readonly bool AbleToReallocate;
        public Resource? ReallocationCost;

        readonly Resource _resource;
        readonly float _productionTime;
        readonly bool _imidiatelyStartProduction;
        readonly bool _loopProduction;

        internal Building(BuildingType type, Vector2Int position)
        {
            Type = type;

            BuildingData data = GameMap.DB[type];
            GameObject = Object.Instantiate(GameMap.BuildingPrefabCollection[type]);
            GameObject.transform.position = GameMap.GetMiddlePointWithOffset(position, type);

            Position = position;

            if(data.ResourceProductionData.HasValue)
            {
                _resource = data.ResourceProductionData.Value.Resource;
                _productionTime = data.ResourceProductionData.Value.ProductionTime;
                _imidiatelyStartProduction = data.ResourceProductionData.Value.StartImidiately;
                _loopProduction = data.ResourceProductionData.Value.Loop;
            }

            Name = data.Name;
            AbleToReallocate = data.AbleToReallocate;
            ReallocationCost = data.ReallocationCost;

            // schedule construction task
            var task = new BuildingTask(CONSTRUCTION_TIME, FinishConstruction);
            ScheduledTask = task;
            GameMap.Instance.ScheduleTask(task);
        }

        public void FinishConstruction()
        {
            Constructed = true;

            if (_imidiatelyStartProduction)
                StartProduction();
        }

        public void AddResource()
        {
            ResourceManager.AddResources(_resource);
            ProductionStarted = false;

            if (_loopProduction)
                StartProduction();
        }

        public void StartProduction()
        {
            // schedule production task
            BuildingTask task = new BuildingTask(_productionTime, AddResource);
            ScheduledTask = task;
            GameMap.Instance.ScheduleTask(task);
            ProductionStarted = true;
        }
    }
}
