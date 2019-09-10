using Assets.Database;
using Assets.World.DataModels;
using Assets.World.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.World.Commands
{
    public class BuildBuildingCommand : AbstractCommand, ICommand, ICloneable<AbstractCommand>
    {
        public Vector2Int To { get; private set; }
        public Building ConstructedBuilding { get; private set; }

        public readonly BuildingType Type;

        /// <summary>
        /// Commands GameMap to reallocate the building to the location passed in the 'to' parameter.
        /// </summary>
        public BuildBuildingCommand(BuildingType type) // type is provided in the constructor and never changes  
        {                                                  // because in this game we always know what building we are going to build
            Type = type;
        }

        BuildBuildingCommand(BuildingType type, Vector2Int to, Building b, bool succeeded)
        {
            Type = type;
            To = to;
            ConstructedBuilding = b;
            _succeeded = succeeded;
        }

        public override bool Call()
        {
            if (_succeeded || !CheckExecutionContext() || !CheckConditions())
                return false;

            ConstructedBuilding = GameMap.BuildBuilding(Type, To);
            _succeeded = true;

            return true;
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            GameMap.RemoveBuilding(ConstructedBuilding, true);
            ConstructedBuilding = null;
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
            {
                Debug.Log("Returned due to area not being free");
                return false; // not enough space
            }

            if (!ResourceManager.IsEnoughResources(Type))
            {
                Debug.Log("Returned due to not enough resources");
                return false; // not enough resources
            }

            return true;
        }

        public override string ToString() => $"Build {ConstructedBuilding.Type.ToString()} at {To.ToString()}";

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public override AbstractCommand Clone() => new BuildBuildingCommand(Type, To, ConstructedBuilding, _succeeded);
    }
}
