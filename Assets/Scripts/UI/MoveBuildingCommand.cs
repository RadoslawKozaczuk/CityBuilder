﻿using Assets.Scripts.DataModels;
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

            _to = GameEngine.Instance.CachedCurrentCell.Value.Coordinates;
            GameMap.Instance.MoveBuilding(_building, _to);

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

            if (!GameEngine.Instance.CachedCurrentCell.HasValue)
                return false; // cursor is not over the map

            if (GameMap.Instance.IsAreaOutOfBounds(GameEngine.Instance.CachedCurrentCell.Value.Coordinates, _type))
                return false; // area is out of the map

            return true;
        }

        public bool CheckConditions()
        {
            if (!GameMap.Instance.IsAreaFree(GameEngine.Instance.CachedCurrentCell.Value.Coordinates, _type))
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
