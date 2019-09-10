using Assets.Database;
using Assets.World.DataModels;
using Assets.World.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.World.Commands
{
    public class MoveBuildingCommand : AbstractCommand, ICommand, ICloneable<AbstractCommand>
    {
        public Vector2Int To { get; private set; }

        public readonly BuildingType Type;
        public readonly Building Building;
        public readonly Vector2Int From;

        /// <summary>
        /// Commands GameMap to reallocate the building to the location passed in the 'to' parameter.
        /// </summary>
        public MoveBuildingCommand(Building b)
        {
            Building = b;
            Type = b.Type;
            From = b.Position;
        }

        // copy constructor (well in fact it is a normal constructor but we use it as a copy constructor)
        MoveBuildingCommand(Building b, Vector2Int from, Vector2Int to, bool succeeded)// : base(b.Type)
        {
            Building = b;
            Type = b.Type;
            From = from;
            To = to;

            _succeeded = succeeded;
        }

        public override bool Call()
        {
            if (_succeeded || !CheckConditions())
                return false;

            GameMap.MoveBuilding(Building, To);
            _succeeded = true;

            return true;
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            GameMap.MoveBuilding(Building, From, true);
            _succeeded = false;

            return true;
        }

        public override bool CheckExecutionContext()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            if (!GameMap.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell))
                return false; // cursor is not over the Grid

            To = cell.Coordinates; // we need to feel to with current data so check condition can proceed

            return true;
        }

        public override bool CheckConditions()
        {
            if (!GameMap.IsAreaFree(To, Type))
                return false; // not enough space

            if (!ResourceManager.IsEnoughResources(Type))
                return false; // not enough resources

            return true;
        }

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public override AbstractCommand Clone() => new MoveBuildingCommand(Building, From, To, _succeeded);
    }
}
