using Assets.Database;
using Assets.World;
using Assets.World.DataModels;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    class ConstructBuildingCommand : AbstractCommand
    {
        public Building Building { get; internal set; }
        public Vector2Int To { get; internal set; }

        public readonly BuildingType Type;

        public ConstructBuildingCommand(BuildingType type)
        {
            Type = type;
        }

        public override bool Call()
        {
            if (_succeeded || !CheckConditions())
                return false;

            To = GameEngine.Instance.CellUnderCursorCached.Value.Coordinates;
            Building = GameMap.BuildBuilding(Type, To);
            _succeeded = true;

            return true;
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            GameMap.RemoveBuilding(Building);
            Building = null;
            _succeeded = false;

            return true;
        }

        public override bool CheckExecutionContext()
        {
            if(EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            if(!GameEngine.Instance.CellUnderCursorCached.HasValue)
                return false; // cursor is not over the map

            if(GameMap.IsAreaOutOfBounds(GameEngine.Instance.CellUnderCursorCached.Value.Coordinates, Type))
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

        public override string ToString() => $"Build {Building.Type.ToString()} at {To.ToString()}";
    }
}
