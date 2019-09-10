using Assets.World.Interfaces;

namespace Assets.World.Commands
{
    public abstract class AbstractCommand : ICommand, ICloneable<AbstractCommand>
    {
        protected readonly bool _lateEvaluation;
        protected bool _succeeded;

        public bool IsSucceeded() => _succeeded;
        public abstract bool Call();
        public abstract bool Undo();
        public abstract bool CheckConditions();
        public abstract bool CheckExecutionContext();

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public abstract AbstractCommand Clone();
    }
}
