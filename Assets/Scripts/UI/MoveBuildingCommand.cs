using Assets.Scripts.DataModels;
using Assets.Scripts.Interfaces;
using UnityEngine;

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

        public MoveBuildingCommand(Building b, Vector2Int to)
        {
            _building = b;
            _to = to;
        }

        public bool IsSucceeded() => _succeeded;

        public bool Call()
        {
            if (_succeeded)
                return false;

            CheckConditions();

            _from = _building.Position;
            GameEngine.Instance.GameMap.MoveBuilding(_building, _to);

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

        public bool CheckExecutionContext() => throw new System.NotImplementedException();

        public bool CheckConditions()
        {
            if (!GameEngine.Instance.TryGibMeClickedCell(out _to))
            {
                Debug.Log("Location invalid");
                return false;
            }

            if (!ResourceManager.IsEnoughResources(_type))
            {
                Debug.Log("No resources");
                return false;
            }

            return true;
        }
    }
}
