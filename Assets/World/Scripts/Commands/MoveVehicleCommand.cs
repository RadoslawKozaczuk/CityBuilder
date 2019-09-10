using Assets.World.DataModels;
using Assets.World.Interfaces;
using UnityEngine.EventSystems;

namespace Assets.World.Commands
{
    public class MoveVehicleCommand : AbstractCommand, ICommand, ICloneable<AbstractCommand>
    {
        public Vehicle Vehicle { get; private set; }

        public MoveVehicleCommand()
        {
        }

        public MoveVehicleCommand(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }

        public override bool Call()
        {
            if (_succeeded || !CheckConditions())
                return false;

            // select
            Vehicle.Selected = true;

            _succeeded = true;

            return true;
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            // unselect
            Vehicle.Selected = false;

            _succeeded = false;

            return true;
        }

        public override bool CheckExecutionContext()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            return true;
        }

        public override bool CheckConditions()
        {
            // is player's vehicle 
            // is selectable etc.

            // for now always true
            return true;
        }

        public override string ToString() => $"Move unit from to {Vehicle.Type.ToString()} ";

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public override AbstractCommand Clone() => new SelectVehicleCommand(Vehicle);
    }
}
