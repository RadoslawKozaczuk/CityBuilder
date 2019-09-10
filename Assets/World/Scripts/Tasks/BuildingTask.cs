using System;

namespace Assets.World.Tasks
{
    public sealed class BuildingTask : AbstractTask
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

        public override void Update()
        {

        }
    }
}
