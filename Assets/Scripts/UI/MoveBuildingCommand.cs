using Assets.Scripts.DataModels;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    class MoveBuildingCommand : ICommand
    {
        readonly BuildingType _type;

        Building _building;
        Vector2Int _location;
        Vector2Int _from;
        Vector2Int _to;
        bool _succeeded;

        public MoveBuildingCommand(Building b)
        {
            _type = b.Type;
            _building = b;
            _from = b.Position;
        }

        public bool IsSucceeded() => _succeeded;

        public bool Call()
        {
            if (_succeeded)
                return false;

            CheckConditions();

            _to = GameEngine.Instance.CellUnderCursorCached.Value.Coordinates;
            GameMap.MoveBuilding(_building, _to);
            ResourceManager.RemoveResources(GameEngine.Instance.Db[_type].ReallocationCost);
            _succeeded = true;
            return true;
        }

        public bool Undo()
        {
            if (!_succeeded)
                return false;

            ResourceManager.AddResources(_type);
            Object.Destroy(_building.GameObject);
            _building = null;

            return true;
        }

        public bool CheckExecutionContext()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            if (!GameEngine.Instance.CellUnderCursorCached.HasValue)
                return false; // cursor is not over the map

            if (GameMap.IsAreaOutOfBounds(GameEngine.Instance.CellUnderCursorCached.Value.Coordinates, _type))
                return false; // area is out of the map

            return true;
        }

        public bool CheckConditions()
        {
            if (!GameMap.IsAreaFree(GameEngine.Instance.CellUnderCursorCached.Value.Coordinates, _type))
                return false; // not enough space

            if (!ResourceManager.IsEnoughResources(_type))
                return false; // not enough resources

            return true;
        }
    }
}
