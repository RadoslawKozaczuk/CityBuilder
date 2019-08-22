using UnityEngine;

namespace Assets.Scripts
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Executes the given function on every cell in the given area. 
        /// Immediately returns true if the function result is true, otherwise false.
        /// This method assumes parameters are valid.
        /// </summary>
        public static bool Any(this GridCell[,] cells, Vector2Int leftBotCell, Vector2Int areaSize, GameMap.FunctionRefStruct<GridCell> func)
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
        public static void All(this GridCell[,] cells, Vector2Int leftBotCell, Vector2Int areaSize, GameMap.ActionRefStruct<GridCell> action)
        {
            for (int i = leftBotCell.x; i < leftBotCell.x + areaSize.x; i++)
                for (int j = leftBotCell.y; j < leftBotCell.y + areaSize.y; j++)
                    action(ref cells[i, j]);
        }

        /// <summary>
        /// Adds building prefab's position values.
        /// Each building has different position offset attached to him.
        /// Calling this method is necessary to position the building right.
        /// </summary>
        public static Vector3 ApplyPrefabPositionOffset(this Vector3 position, BuildingType type)
        {
            GameObject prefab = GameEngine.Instance.BuildingPrefabs[(int)type];
            position.x += prefab.transform.position.x;
            position.y = prefab.transform.position.y;
            position.z += prefab.transform.position.z;

            return position;
        }
    }
}
