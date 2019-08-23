using System;

namespace Assets.Database.DataModels
{
    public class BuildingTask
    {
        public readonly Action ActionOnFinish;
        public readonly float TotalTime;
        public float TimeLeft;

        public BuildingTask(float executionDelay, Action onCompleteAction)
        {
            TotalTime = executionDelay;
            TimeLeft = executionDelay;
            ActionOnFinish = onCompleteAction;
        }
    }
}
