using Assets.Database;
using Assets.Database.DataModels;
using Assets.World.Interfaces;
using Assets.World.Tasks;
using UnityEngine;

namespace Assets.World.DataModels
{
    // this represents building object in the game
    public sealed class Building : MonoBehaviour, IMapObject
    {
        #region Properties
        [HideInInspector] public BuildingType Type { get; private set; }

        /// <summary>
        /// Game map's coordinates.
        /// </summary>
        public Vector2Int Position { get; internal set; }

        public Vector2Int Size => GameMap.DB[Type].Size;

        public bool HasScheduledTask => ScheduledTask != null;

        public float TaskTotalTime
        {
            get
            {
#if UNITY_EDITOR
                if (ScheduledTask == null)
                    throw new System.Exception("Cannot ask for TaskTotalTime if none tasks are scheduled");
#endif

                return ScheduledTask.TotalTime;
            }
        }

        public float TaskTimeLeft
        {
            get
            {
#if UNITY_EDITOR
                if (ScheduledTask == null)
                    throw new System.Exception("Cannot ask for TaskTimeLeft if none tasks are scheduled");
#endif

                return ScheduledTask.TimeLeft;
            }
        }
        #endregion

        public string Name;
        public bool Constructed = false;
        public bool ProductionStarted;
        public bool AbleToReallocate;
        public Resource? ReallocationCost;

        internal ResourceProductionTask ScheduledTask;

        Resource _resource;
        float _productionTime;
        bool _imidiatelyStartProduction;
        bool _loopProduction;

        internal void SetData(BuildingType type, Vector2Int position)
        {
            Type = type;

            BuildingData data = GameMap.DB[type];
            transform.position = GameMap.GetMiddlePointWithOffset(position, type);

            Position = position;

            if (data.ResourceProductionData.HasValue)
            {
                _resource = data.ResourceProductionData.Value.Resource;
                _productionTime = data.ResourceProductionData.Value.ProductionTime;
                _imidiatelyStartProduction = data.ResourceProductionData.Value.StartImidiately;
                _loopProduction = data.ResourceProductionData.Value.Loop;
            }

            Name = data.Name;
            AbleToReallocate = data.AbleToReallocate;
            ReallocationCost = data.ReallocationCost;

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
            ResourceProductionTask task = new ResourceProductionTask(_productionTime, AddResource);
            ScheduledTask = task;
            TaskManager.ScheduleTask(task);
            ProductionStarted = true;
        }
    }
}
