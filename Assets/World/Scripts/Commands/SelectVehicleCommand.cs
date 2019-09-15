using Assets.World.DataModels;
using Assets.World.Interfaces;
using UnityEngine.EventSystems;

namespace Assets.World.Commands
{
    // select command now seems to be a bit redundant but later on when I add another way to select a vehicle 
    // like select many or add to group etc. it should come in handy
    public class SelectVehicleCommand : AbstractCommand, ICommand, ICloneable<AbstractCommand>
    {
        internal readonly Vehicle Vehicle;

        internal SelectVehicleCommand(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }

        public override bool Call()
        {
            if (_succeeded || !CheckConditions())
                return false;

            // select
            Vehicle.Selected = true;
            GameMap.Instance.SelectedVehicle = Vehicle;

            return base.Call();
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            // unselect
            Vehicle.Selected = false;

            return base.Undo();
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
