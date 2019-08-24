using Assets.Database;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Commands
{
    abstract class AbstractCommand : ICommand, ICloneable<AbstractCommand>
    {
        public readonly BuildingType Type; // this is here to make it visible outside without casting

        protected bool _succeeded;

        protected AbstractCommand(BuildingType type)
        {
            Type = type;
        }

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
