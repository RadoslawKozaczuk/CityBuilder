using Assets.Database;
using Assets.Scripts.Interfaces;
using Assets.World;
using Assets.World.DataModels;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Commands
{
    class ConstructBuildingCommand : AbstractCommand, ICloneable<AbstractCommand>
    {
        public Vector2Int To { get; private set; }

        public new readonly BuildingType Type; // Type base variable is hidden but we want to have both to make it accessible without casting
        public Building Building { get; private set; }

        /// <summary>
        /// Commands GameMap to reallocate the building to the location passed in the 'to' parameter.
        /// </summary>
        public ConstructBuildingCommand(BuildingType type, Vector2Int to) : base(type) // to ensure base field is also set
        {
            Type = type;

            To = to;
        }

        /// <summary>
        /// Commands GameMap to reallocate the building to the location passed in the 'to' parameter.
        /// Important: The parameter 'to' is a late evaluation type of parameter 
        /// and it operating on a promise that it will have a value at the moment of the function execution.
        /// </summary>
        public ConstructBuildingCommand(BuildingType type, NullableGridCellStructRef to) : base(type, to)
        {
            Type = type;
        }

        ConstructBuildingCommand(BuildingType type, Vector2Int to, Building b, bool succeeded) : base(type)
        {
            Type = type;
            To = to;
            Building = b;
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

            Building = GameMap.BuildBuilding(Type, To);
            _succeeded = true;

            return true;
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            GameMap.RemoveBuilding(Building, true);
            Building = null;
            _succeeded = false;

            return true;
        }

        public override bool CheckExecutionContext()
        {
            if(EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            if(_lateEvaluation && !_promise.GridCell.HasValue)
                return false; // cursor is not over the map

            if(GameMap.IsAreaOutOfBounds(_promise.GridCell.Value.Coordinates, Type))
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

        public override string ToString() => $"Build {Building.Type.ToString()} "
            + $"at {(_lateEvaluation ? _promise.GridCell.Value.Coordinates : To).ToString()}";

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public override AbstractCommand Clone() => new ConstructBuildingCommand(Type, To, Building, _succeeded);
    }
}
