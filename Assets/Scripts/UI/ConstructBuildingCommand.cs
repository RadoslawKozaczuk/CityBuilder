using Assets.Scripts.DataModels;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    class ConstructBuildingCommand : ICommand
    {
        readonly BuildingType _type;

        Vector2Int _to;
        Building _building;
        bool _succeeded;

        public ConstructBuildingCommand(BuildingType type)
        {
            _type = type;
        }

        public bool IsSucceeded() => _succeeded;

        public bool Call()
        {
            if (_succeeded)
                return false;

            CheckConditions();

            _to = GameEngine.Instance.CachedCurrentCell.Value.Coordinates;
            _building = new Building(_type, _to);
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
            if(EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            if(!GameEngine.Instance.CachedCurrentCell.HasValue)
                return false; // cursor is not over the map

            if(GameMap.Instance.IsAreaOutOfBounds(GameEngine.Instance.CachedCurrentCell.Value.Coordinates, _type))
                return false; // area is out of the map

            return true;
        }

        public bool CheckConditions()
        {
            if (!GameMap.Instance.IsAreaFree(GameEngine.Instance.CachedCurrentCell.Value.Coordinates, _type))
                return false; // not enough space

            if (!ResourceManager.IsEnoughResources(_type))
                return false; // not enough resources

            return true;
        }
    }
}
