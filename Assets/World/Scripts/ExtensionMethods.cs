using Assets.Database;
using Assets.World.DataModels;
using UnityEngine;

namespace Assets.World
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Executes the given function on every cell in the given area. 
        /// Immediately returns true once the function return value is true.
        /// Returns false after performing all operations. 
        /// </summary>
        internal static bool Any(this GridCell[,] cells, Vector2Int leftBotCell, Vector2Int areaSize, GameMap.FunctionRefStruct<GridCell> func)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (leftBotCell.x < 0 || leftBotCell.x >= GameMap.GridSizeX 
                || leftBotCell.y < 0 || leftBotCell.y >= GameMap.GridSizeY
                || areaSize.x < 1 || areaSize.y < 1
                || leftBotCell.x + areaSize.x > GameMap.GridSizeX + 1 
                || leftBotCell.y + areaSize.y > GameMap.GridSizeY + 1)
                throw new System.ArgumentException($"Area arguments passed to 'Any' extension method are out of bounds.");
            else if(func == null)
                throw new System.ArgumentNullException("func", $"Function passed to 'Any' extension method cannot be null.");
#endif

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (leftBotCell.x < 0 || leftBotCell.x >= GameMap.GridSizeX
                || leftBotCell.y < 0 || leftBotCell.y >= GameMap.GridSizeY
                || areaSize.x < 1 || areaSize.y < 1
                || leftBotCell.x + areaSize.x > GameMap.GridSizeX + 1
                || leftBotCell.y + areaSize.y > GameMap.GridSizeY + 1)
                throw new System.ArgumentException($"Area arguments passed to 'All' extension method are out of bounds.");
            else if (action == null)
                throw new System.ArgumentNullException("action", $"Action passed to 'All' extension method cannot be null.");
#endif

            for (int i = leftBotCell.x; i < leftBotCell.x + areaSize.x; i++)
                for (int j = leftBotCell.y; j < leftBotCell.y + areaSize.y; j++)
                    action(ref cells[i, j]);
        }

        /// <summary>
        /// Adds building prefab's position values.
        /// Each building has different position offset.
        /// It is necessary to call this method to position the building right.
        /// </summary>
        internal static Vector3 ApplyPrefabPositionOffset(this Vector3 position, BuildingType type) 
            => ApplyPrefabPositionOffsetInternal(position, GameMap.MapFeaturePrefabCollection[type]);

        /// <summary>
        /// Adds vehicle prefab's position values.
        /// Each vehicle has different position offset.
        /// It is necessary to call this method to position the vehicle right.
        /// </summary>
        internal static Vector3 ApplyPrefabPositionOffset(this Vector3 position, VehicleType type) 
            => ApplyPrefabPositionOffsetInternal(position, GameMap.MapFeaturePrefabCollection[type]);

        static Vector3 ApplyPrefabPositionOffsetInternal(this Vector3 position, GameObject prefab)
        {
            position.x += prefab.transform.position.x;
            position.y = prefab.transform.position.y;
            position.z += prefab.transform.position.z;

            return position;
        }
    }
}
