using System;
using UnityEngine;

namespace Assets.World.Tasks
{
    internal sealed class ResourceProductionTask : AbstractTask
    {
        internal readonly Action ActionOnFinish;
        internal readonly float TotalTime;
        internal float TimeLeft;

        internal ResourceProductionTask(float executionDelay, Action onCompleteAction) : base()
        {
            TotalTime = executionDelay;
            TimeLeft = executionDelay;
            ActionOnFinish = onCompleteAction;
        }

        internal override void Update()
        {
            if (IsCompletedOrPending())
                return;

            TimeLeft -= Time.deltaTime;

            if (TimeLeft > 0)
                return;

            ActionOnFinish();
        }

        internal override string ToString()
        {
            return $"ResourceProductionTask ID[{Id}] time: {string.Format("{0:0.00}", TimeLeft)} status: {Status}";
        }
    }
}
