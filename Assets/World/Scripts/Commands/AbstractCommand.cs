using Assets.World.Interfaces;

namespace Assets.World.Commands
{
    // Commands can be understood as orders given directly or indirectly by external agent (user or AI).
    // A command may select a unit or build a building. Each command has a Undo action allowing the command's effects to be reverted.
    // Reverting an action may mean a different thing depends on the command and the context. For example reverting a move command 
    // stop the vehicle from moving while reverting a build command removes the building.
    public abstract class AbstractCommand : ICommand, ICloneable<AbstractCommand>
    {
        public bool Succeeded() => _succeeded;

        protected bool _succeeded;

        public virtual bool Call()
        {
            _succeeded = true;
            ExecutedCommandList.Add(this);
            return true;
        }

        public virtual bool Undo()
        {
            _succeeded = false;
            return true;
        }

        public abstract bool CheckConditions();

        public abstract bool CheckExecutionContext();

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public abstract AbstractCommand Clone();
    }
}
