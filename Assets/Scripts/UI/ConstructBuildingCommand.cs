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

            _to = new Vector2Int(GameEngine.Instance.CachedCurrentCell.Value.X, GameEngine.Instance.CachedCurrentCell.Value.Y);
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
            {
                // cursor is over the UI
                return false;
            }

            if(!GameEngine.Instance.CachedCurrentCell.HasValue)
            {
                // cursor is not over the map
                return false;
            }

            // sprawdź czy area within the game map
            if(GameEngine.Instance.GameMap.IsAreaOutOfBounds(GameEngine.Instance.CachedCurrentCell.Value.X, GameEngine.Instance.CachedCurrentCell.Value.Y, _type))
            {
                // area is out of the map
                return false;
            }

            return true;
        }

        public bool CheckConditions()
        {
            if (!GameEngine.Instance.GameMap.IsAreaFree(
                new Vector2Int(GameEngine.Instance.CachedCurrentCell.Value.X, GameEngine.Instance.CachedCurrentCell.Value.Y), _type))
            {
                Debug.Log("Not enough space");
                return false;
            }

            // check money
            if (!ResourceManager.IsEnoughResources(_type))
            {
                Debug.Log("No resources");
                return false;
            }

            return true;
        }
    }
}
