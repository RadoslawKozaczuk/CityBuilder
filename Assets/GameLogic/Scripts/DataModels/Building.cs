using Assets.Database;
using Assets.Database.DataModels;
using Assets.GameLogic.Interfaces;
using Assets.GameLogic.Tasks;
using UnityEngine;

namespace Assets.GameLogic.DataModels
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (ScheduledTask == null)
                    throw new System.Exception("Cannot ask for TaskTimeLeft if none tasks are scheduled");
#endif

                return ScheduledTask.TimeLeft;
            }
        }

        internal bool LoopProduction { get; private set; }

        internal Resource Resource { get; private set; }

        internal float ProductionTime { get; private set; }
        #endregion

        public string Name;
        public bool ProductionStarted;
        public bool AbleToReallocate;
        public Resource? ReallocationCost;

        internal ResourceProductionTask ScheduledTask;
        
        bool _immediatelyStartProduction;

        internal void SetData(BuildingType type, Vector2Int position)
        {
            Type = type;

            BuildingData data = GameMap.DB[type];
            transform.position = GameMap.GetMiddlePointWithOffset(position, type);

            Position = position;

            if (data.ResourceProductionData.HasValue)
            {
                Resource = data.ResourceProductionData.Value.Resource;
                ProductionTime = data.ResourceProductionData.Value.ProductionTime;
                _immediatelyStartProduction = data.ResourceProductionData.Value.StartImmediately;
                LoopProduction = data.ResourceProductionData.Value.Loop;
            }

            Name = data.Name;
            AbleToReallocate = data.AbleToReallocate;
            ReallocationCost = data.ReallocationCost;

            if (_immediatelyStartProduction)
                StartProduction();
        }

        public void StartProduction()
        {
            // schedule production task
            var task = new ResourceProductionTask(this);
            ScheduledTask = task;
            TaskManager.ScheduleTask(task);
        }
    }
}
