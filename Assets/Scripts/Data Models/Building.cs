using Assets.Scripts.DataSource;
using UnityEngine;

namespace Assets.Scripts.DataModels
{
    // this represents building object in the game
    public class Building
    {
        const float CONSTRUCTION_TIME = 5f; // hardcoded

        /// <summary>
        /// Position always point at the left bottom corner of the building.
        /// </summary>
        public Vector2Int Position
        {
            get => _position;
            set
            {
                GameMap.MarkAreaAsFree(_position, Size);

                _position = value;
                GameMap.MarkAreaAsOccupied(this);
                GameObject.transform.position = GameMap.GetMiddlePoint(_position, Type)
                    .ApplyPrefabPositionOffset(Type);
            }
        }
        Vector2Int _position;

        public Vector2Int Size => GameEngine.Instance.Db[Type].Size;

        public string Name;
        public BuildingType Type { get; private set; }
        public GameObject GameObject;
        public bool Constructed = false;
        public bool ProductionStarted;
        public BuildingTask ScheduledTask;
        public readonly bool AbleToReallocate;
        public Resource? ReallocationCost;

        readonly Resource _resource;
        readonly float _productionTime;
        readonly bool _imidiatelyStartProduction;
        readonly bool _loopProduction;

        public Building(BuildingType type, Vector2Int position)
        {
            Type = type;

            BuildingData data = GameEngine.Instance.Db[type];
            GameObject = Object.Instantiate(GameEngine.Instance.BuildingPrefabs[(int)type]);
            Position = position;

            _resource = data.ResourceProductionData.Resource;
            _productionTime = data.ResourceProductionData.ProductionTime;
            _imidiatelyStartProduction = data.ResourceProductionData.StartImidiately;
            _loopProduction = data.ResourceProductionData.Loop;

            Name = data.Name;
            AbleToReallocate = data.AbleToReallocate;
            ReallocationCost = data.ReallocationCost;

            // schedule construction task
            var task = new BuildingTask(CONSTRUCTION_TIME, FinishConstruction);
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
            GameEngine.Instance.ScheduleTask(task);
            ProductionStarted = true;
        }
    }
}
