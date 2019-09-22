namespace Assets.GameLogic.Tasks
{
    /// <summary>
    /// Task is meant to be an internal system continuous action. Each task can either be completed, aborted or run infinitely.
    /// You can think of them as some sort of coroutines but better.
    /// </summary>
    internal abstract class AbstractTask
    {
        internal TaskStatus Status;
        internal readonly int Id;
        internal AbstractTask WaitingFor;

        protected bool _aborted;

        protected AbstractTask()
        {
            Id = TaskManager.GetFirstFreeTaskId();
        }

        internal void Start() => Status = TaskStatus.Ongoing;

        protected internal void Abort()
        {
            _aborted = true;
            Status = Status == TaskStatus.Pending ? TaskStatus.Completed : TaskStatus.Aborting;
        }

        internal abstract void Update();

        internal new abstract string ToString();

        protected bool IsCompletedOrPending()
        {
            if (Status == TaskStatus.Completed)
                return true;

            if (WaitingFor != null)
            {
                if (WaitingFor.Status == TaskStatus.Completed)
                {
                    WaitingFor = null;
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
