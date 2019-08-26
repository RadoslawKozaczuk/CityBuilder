using Assets.Database;
using Assets.World.DataModels;
using UnityEngine;

namespace Assets.World
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Executes the given function on every cell in the given area. 
        /// Immediately returns true if the function result is true, otherwise false.
        /// This method assumes parameters are valid.
        /// </summary>
        internal static bool Any(this GridCell[,] cells, Vector2Int leftBotCell, Vector2Int areaSize, GameMap.FunctionRefStruct<GridCell> func)
        {
            for (int i = leftBotCell.x; i < leftBotCell.x + areaSize.x; i++)
                for (int j = leftBotCell.y; j < leftBotCell.y + areaSize.y; j++)
                    if (func(ref cells[i, j]))
                        return true;

            return false;
        }

        /// <summary>
        /// Executes the given action on every cell in the given area.
        /// This method assumes parameters are valid.
        /// </summary>
        internal static void All(this GridCell[,] cells, Vector2Int leftBotCell, Vector2Int areaSize, GameMap.ActionRefStruct<GridCell> action)
        {
            for (int i = leftBotCell.x; i < leftBotCell.x + areaSize.x; i++)
                for (int j = leftBotCell.y; j < leftBotCell.y + areaSize.y; j++)
                    action(ref cells[i, j]);
        }

        /// <summary>
        /// Adds building prefab's position values.
        /// Each building has different position offset.
        /// It is necessary to call this method is necessary to position the building right.
        /// </summary>
        internal static Vector3 ApplyPrefabPositionOffset(this Vector3 position, BuildingType type) 
            => ApplyPrefabPositionOffsetInternal(position, GameMap.BuildingPrefabCollection[type]);

        /// <summary>
        /// Adds vehicle prefab's position values.
        /// Each vehicle has different position offset.
        /// It is necessary to call this method is necessary to position the vehicle right.
        /// </summary>
        internal static Vector3 ApplyPrefabPositionOffset(this Vector3 position, VehicleType type) 
            => ApplyPrefabPositionOffsetInternal(position, GameMap.BuildingPrefabCollection[type]);

        static Vector3 ApplyPrefabPositionOffsetInternal(this Vector3 position, GameObject prefab)
        {
            position.x += prefab.transform.position.x;
            position.y = prefab.transform.position.y;
            position.z += prefab.transform.position.z;

            return position;
        }
    }
}
