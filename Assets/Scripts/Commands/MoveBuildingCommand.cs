using Assets.Database;
using Assets.Scripts.Interfaces;
using Assets.World;
using Assets.World.DataModels;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Commands
{
    class MoveBuildingCommand : AbstractCommand, ICommand, ICloneable<AbstractCommand>
    {
        public Vector2Int To { get; private set; }

        public new readonly BuildingType Type; // Type base variable is hidden but we want to have both to make it accessible without casting
        public readonly Building Building;
        public readonly Vector2Int From;

        /// <summary>
        /// Commands GameMap to reallocate the building to the location passed in the 'to' parameter.
        /// </summary>
        public MoveBuildingCommand(Building b, Vector2Int to) : base(b.Type) // to ensure base field is also set
        {
            Building = b;
            Type = b.Type;
            From = b.Position;
            To = to;
        }

        /// <summary>
        /// Commands GameMap to reallocate the building to the location passed in the 'to' parameter.
        /// Important: The parameter 'to' is a late evaluation type of parameter 
        /// and it operating on a promise that it will have a value at the moment of the function execution.
        /// </summary>
        public MoveBuildingCommand(Building b, NullableGridCellStructRef to) : base(b.Type, to)
        {
            Building = b;
            Type = b.Type;
            From = b.Position;
        }

        // copy constructor (well in fact it is a normal constructor but we use it as a copy constructor)
        MoveBuildingCommand(Building b, Vector2Int from, Vector2Int to, bool succeeded) : base(b.Type)
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

            if (_lateEvaluation)
            {
                if (_promise.GridCell.HasValue)
                    To = _promise.GridCell.Value.Coordinates;
                else
                    throw new ArgumentException("Promise unfulfilled - parameter 'to' does not have a value.");
            }

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

            if (_lateEvaluation && !_promise.GridCell.HasValue)
                return false; // cursor is not over the map

            if (GameMap.IsAreaOutOfBounds(_promise.GridCell.Value.Coordinates, Type))
                return false; // area is out of the map

            return true;
        }

        public override bool CheckConditions()
        {
            if (!GameMap.IsAreaFree(_lateEvaluation ? _promise.GridCell.Value.Coordinates : To, Type))
                return false; // not enough space

            if (!ResourceManager.IsEnoughResources(Type))
                return false; // not enough resources

            return true;
        }

        public override string ToString() => $"Move {Building.Type.ToString()} "
            + $"to {(_lateEvaluation ? _promise.GridCell.Value.Coordinates : To).ToString()}";

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public override AbstractCommand Clone() => new MoveBuildingCommand(Building, From, To, _succeeded);
    }
}
