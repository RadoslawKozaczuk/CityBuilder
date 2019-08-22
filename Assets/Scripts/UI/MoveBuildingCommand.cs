using Assets.Scripts.DataModels;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    class MoveBuildingCommand : AbstractCommand, ICommand
    {
        public Building Building { get; internal set; }
        public Vector2Int From { get; internal set; }
        public Vector2Int To { get; internal set; }

        public readonly BuildingType Type;

        public MoveBuildingCommand(Building b)
        {
            Type = b.Type;
            Building = b;
            From = b.Position;
        }

        public override bool Call()
        {
            if (_succeeded)
                return false;

            CheckConditions();

            To = GameEngine.Instance.CellUnderCursorCached.Value.Coordinates;
            GameMap.MoveBuilding(Building, To);
            ResourceManager.RemoveResources(GameEngine.Instance.Db[Type].ReallocationCost);
            _succeeded = true;

            return true;
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            ResourceManager.AddResources(Building.ReallocationCost);
            GameMap.MoveBuilding(Building, From);
            _succeeded = false;

            return true;
        }

        public override bool CheckExecutionContext()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            if (!GameEngine.Instance.CellUnderCursorCached.HasValue)
                return false; // cursor is not over the map

            if (GameMap.IsAreaOutOfBounds(GameEngine.Instance.CellUnderCursorCached.Value.Coordinates, Type))
                return false; // area is out of the map

            return true;
        }

        public override bool CheckConditions()
        {
            if (!GameMap.IsAreaFree(GameEngine.Instance.CellUnderCursorCached.Value.Coordinates, Type))
                return false; // not enough space

            if (!ResourceManager.IsEnoughResources(Type))
                return false; // not enough resources

            return true;
        }

        public override string ToString() => $"Move {Building.Type.ToString()} from {From.ToString()} to {To.ToString()}";
    }
}
