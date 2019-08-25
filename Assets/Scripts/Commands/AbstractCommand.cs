using Assets.Database;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Commands
{
    abstract class AbstractCommand : ICommand, ICloneable<AbstractCommand>
    {
        public readonly BuildingType Type; // to make Type visible outside without necessity for casting

        protected readonly NullableGridCellStructRef _promise;
        protected readonly bool _lateEvaluation;
        protected bool _succeeded;

        protected AbstractCommand(BuildingType type)
        {
            Type = type;
        }

        protected AbstractCommand(BuildingType type, NullableGridCellStructRef promise)
        {
            Type = type;
            _promise = promise;
            _lateEvaluation = true;
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
