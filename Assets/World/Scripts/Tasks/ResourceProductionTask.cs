using System;

namespace Assets.World.Tasks
{
    internal sealed class ResourceProductionTask : AbstractTask
    {
        internal readonly Action ActionOnFinish;
        internal readonly float TotalTime;
        internal float TimeLeft;

        internal ResourceProductionTask(float executionDelay, Action onCompleteAction)
        {
            TotalTime = executionDelay;
            TimeLeft = executionDelay;
            ActionOnFinish = onCompleteAction;
        }

        internal override void Abort() => throw new NotImplementedException();

        internal override void Update()
        {

        }
    }
}
