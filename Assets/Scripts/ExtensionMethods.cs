using System;

namespace Assets.Scripts
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Executes the given function on every cell in the given area. 
        /// Immediately returns true if the function result is true.
        /// Otherwise returns false.
        /// This method does check if the parameters are valid.
        /// </summary>
        public static bool Any(this GridCell[,] cells, int x, int y, int sizeX, int sizeY, Func<GridCell, bool> func)
        {
            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j < y + sizeY; j++)
                    if (func(cells[i, j]))
                        return true;

            return false;
        }

        /// <summary>
        /// Executes the given action on every cell in the given area. 
        /// This method does check if the parameters are valid.
        /// </summary>
        public static void All(this GridCell[,] cells, int x, int y, int sizeX, int sizeY, Action<GridCell> action)
        {
            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j < y + sizeY; j++)
                    action(cells[i, j]);
        }
    }
}
