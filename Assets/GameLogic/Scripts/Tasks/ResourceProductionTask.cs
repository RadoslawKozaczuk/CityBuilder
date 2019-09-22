using Assets.GameLogic.DataModels;
using UnityEngine;

namespace Assets.GameLogic.Tasks
{
    internal sealed class ResourceProductionTask : AbstractTask
    {
        internal readonly Building Building;
        internal readonly float TotalTime;
        internal float TimeLeft;

        internal ResourceProductionTask(Building building) : base()
        {
            Building = building;
            TotalTime = building.ProductionTime;
            TimeLeft = building.ProductionTime;

            building.ProductionStarted = true;
        }

        internal override void Update()
        {
            if (IsCompletedOrPending())
                return;

            TimeLeft -= Time.deltaTime;

            if (TimeLeft > 0)
                return;

            ResourceManager.AddResources(Building.Resource);

            if (Building.LoopProduction)
                TimeLeft = Building.ProductionTime;
            else
            {
                Status = TaskStatus.Completed;
                Building.ProductionStarted = false;
            }
        }

        internal override string ToString() 
            => $"[{Id}] ResourceProduction, time: {string.Format("{0:0.00}", TimeLeft)} "
                + $"resource: {Building.Resource.ResourceType.ToString()}[{Building.Resource.Quantity}] status: {Status}";
    }
}
