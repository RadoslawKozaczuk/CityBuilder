namespace Assets.World.Tasks
{
    public abstract class AbstractTask
    {
        public bool Completed;

        public abstract void Update();
    }
}
