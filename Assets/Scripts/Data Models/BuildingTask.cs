using System;

namespace Assets.Scripts.DataModels
{
    public class BuildingTask
    {
        public float TotalTime, TimeLeft;
        public Action ActionOnFinish;

        public BuildingTask(float executionDelay, Action onCompleteAction)
        {
            TotalTime = executionDelay;
            TimeLeft = executionDelay;
            ActionOnFinish = onCompleteAction;
        }
    }
}
