using Assets.Scripts.Interfaces;
using Assets.World.DataModels;
using UnityEngine.EventSystems;

namespace Assets.World.Commands
{
    public class SelectVehicleCommand : AbstractCommand, ICommand, ICloneable<AbstractCommand>
    {
        public Vehicle Vehicle { get; private set; }

        public SelectVehicleCommand()
        {
        }

        public SelectVehicleCommand(Vehicle vehicle)
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

        public override string ToString() => $"Select unit {Vehicle.Type.ToString()} ";

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public override AbstractCommand Clone() => new SelectVehicleCommand(Vehicle);
    }
}
