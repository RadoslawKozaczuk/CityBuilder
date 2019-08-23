using Assets.Scripts.Interfaces;
using System;

namespace Assets.Scripts.UI
{
    abstract class AbstractCommand : ICommand, ICloneable<AbstractCommand>
    {
        protected bool _succeeded;

        public bool IsSucceeded() => _succeeded;
        public abstract bool Call();
        public abstract bool Undo();
        public abstract bool CheckConditions();
        public abstract bool CheckExecutionContext();

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public AbstractCommand Clone()
        {
            AbstractCommand command;

            switch (this)
            {
                case ConstructBuildingCommand c:
                    command = new ConstructBuildingCommand(c.Type)
                    {
                        Building = c.Building,
                        To = c.To
                    };
                    break;
                case MoveBuildingCommand c:
                    command = new MoveBuildingCommand(c.Building)
                    {
                        Building = c.Building,
                        From = c.From,
                        To = c.To
                    };
                    break;
                default:
                    throw new NotImplementedException();
            }

            InjectBaseVariables(command);
            return command;
        }

        void InjectBaseVariables(AbstractCommand reciever) => reciever._succeeded = _succeeded;
    }
}
