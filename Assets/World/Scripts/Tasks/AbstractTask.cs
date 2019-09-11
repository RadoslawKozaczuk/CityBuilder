namespace Assets.World.Tasks
{
    /// <summary>
    /// Task is meant to be an internal system continuous action. Each task can either completed, aborted or run infinitely.
    /// You can think of them as some sort of coroutines but better.
    /// </summary>
    public abstract class AbstractTask
    {
        public bool Completed;

        protected bool _aborted;

        public abstract void Update();

        public abstract void Abort();
    }
}
