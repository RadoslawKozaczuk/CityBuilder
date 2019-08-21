﻿using UnityEngine;

namespace Assets.Scripts
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Executes the given function on every cell in the given area. 
        /// Immediately returns true if the function result is true, otherwise false.
        /// This method does not check parameters validity.
        /// </summary>
        public static bool Any(this GridCell[,] cells, int x, int y, Vector2Int areaSize, GameMap.FunctionRefStruct<GridCell> func)
        {
            for (int i = x; i < x + areaSize.x; i++)
                for (int j = y; j < y + areaSize.y; j++)
                    if (func(ref cells[i, j]))
                        return true;

            return false;
        }

        /// <summary>
        /// Executes the given action on every cell in the given area.
        /// This method does not check parameters validity.
        /// </summary>
        public static void All(this GridCell[,] cells, int x, int y, Vector2Int areaSize, GameMap.ActionRefStruct<GridCell> action)
        {
            for (int i = x; i < x + areaSize.x; i++)
                for (int j = y; j < y + areaSize.y; j++)
                    action(ref cells[i, j]);
        }

        public static Vector3 ApplyPrefabPositionOffset(this Vector3 position, BuildingType type)
        {
            GameObject prefab = GameEngine.Instance.BuildingPrefabs[(int)type];
            position.x += prefab.transform.position.x;
            position.y = prefab.transform.position.y;
            position.z += prefab.transform.position.z;

            return position;
        }

        public static Vector3 ApplyPrefabPositionOffset(this Vector3 position, GameObject prefab)
        {
            position.x += prefab.transform.position.x;
            position.y = prefab.transform.position.y;
            position.z += prefab.transform.position.z;

            return position;
        }
    }
}
