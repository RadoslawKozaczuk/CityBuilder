using Assets.World.Interfaces;

namespace Assets.World.Commands
{
    /// <summary>
    /// Commands can be understood as orders given directly or indirectly by an external agent (player or AI).
    /// A command may, for example, select a unit or build a building. Each command has an Undo action allowing its effects to be reverted.
    /// Reverting a command may mean a different thing depends on the command and the context. For example, reverting a move command 
    /// simply stops the vehicle from moving while reverting a build command removes the building and gives back the resources.
    /// </summary>
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
            ExecutedCommandList.RemoveCommand(this);
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
