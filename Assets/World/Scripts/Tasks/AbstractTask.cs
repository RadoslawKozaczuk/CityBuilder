namespace Assets.World.Tasks
{
    /// <summary>
    /// Task is meant to be an internal system continuous action. Each task can either be completed, aborted or run infinitely.
    /// You can think of them as some sort of coroutines but better.
    /// </summary>
    internal abstract class AbstractTask
    {
        internal TaskStatus TaskStatus;
        internal readonly int Id;
        internal bool Completed { get; private set; }

        internal AbstractTask WaitingFor;
        protected bool _aborted;

        protected AbstractTask()
        {
            Id = TaskManager.GetFirstFreeTaskId();
        }

        protected internal void Abort()
        {
            _aborted = true;
            TaskStatus = TaskStatus.Aborting;
        }

        internal abstract void Update();

        internal new abstract string ToString();

        protected void MarkAsCompleted()
        {
            Completed = true;
            TaskStatus = TaskStatus.Completed;
        }
    }
}
