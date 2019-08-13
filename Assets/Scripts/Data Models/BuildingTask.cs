using System;

namespace Assets.Scripts.DataModels
{
    public class BuildingTask
    {
        public float TotalTime, TimeLeft;
        public Action ActionOnFinish;

        public BuildingTask(float taskExecutionTime, Action onCompleteAction)
        {
            TotalTime = taskExecutionTime;
            TimeLeft = taskExecutionTime;
            ActionOnFinish = onCompleteAction;
        }
    }
}
