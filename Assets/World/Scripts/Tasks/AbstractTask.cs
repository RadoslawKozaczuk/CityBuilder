namespace Assets.World.Tasks
{
    /// <summary>
    /// Task is meant to be an internal system continuous action. Each task can either be completed, aborted or run infinitely.
    /// You can think of them as some sort of coroutines but better.
    /// </summary>
    internal abstract class AbstractTask
    {
        internal bool Completed;

        protected AbstractTask _waitingFor;
        protected bool _aborted;

        internal abstract void Update();

        internal abstract void Abort();
    }
}
