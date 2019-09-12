using Assets.World.DataModels;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.World
{
    // Utilizes the Breadth First Search approach
    internal sealed class PathFinder
    {
        // we use tuples extensively here as we need data to be logically tied together 
        // but at the same time we don't want to bother ourselves with dedicated structures
        readonly (int xOffset, int yOffset)[] _directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };
        readonly (int fromX, int fromY)[,] _cameFrom;
        readonly GridCell[,] _cells;
        readonly Queue<(int x, int y)> _frontier;

        internal PathFinder(GridCell[,] cells)
        {
#if UNITY_EDITOR
            if (cells.GetLength(0) != GameMap.GridSizeX || cells.GetLength(1) != GameMap.GridSizeY)
                throw new System.ArgumentException($"Cells array must be of size {GameMap.GridSizeX}x {GameMap.GridSizeX}");
#endif

            _cells = cells;
            _cameFrom = new (int fromX, int fromY)[GameMap.GridSizeX, GameMap.GridSizeX];
            _frontier = new Queue<(int x, int y)>();
        }

        internal List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
        {
            ClearData();

            // The key idea for all of these algorithms is that we keep track of an expanding ring called the frontier.
            _frontier.Enqueue((from.x, from.y));

            while (_frontier.Count > 0)
            {
                var (currX, currY) = _frontier.Dequeue();
                foreach (var (xOffset, yOffset) in _directions)
                {
                    var (nextX, nextY) = (currX + xOffset, currY + yOffset);

                    // is out of bounds check
                    if (nextX < 0 || nextX >= GameMap.GridSizeX || nextY < 0 || nextY >= GameMap.GridSizeY)
                        continue;

                    // is occupied check
                    if (_cells[nextX, nextY].IsOccupied)
                        continue;

                    // check if next is not the one that you came from
                    var (fromX, fromY) = _cameFrom[currX, currY];
                    if (nextX == fromX && nextY == fromY)
                        continue;

                    if (nextX == to.x && nextY == to.y)
                    {
                        // path found
                        _cameFrom[nextX, nextY] = (currX, currY);
                        return CreatePath(to);
                    }

                    // is not visited check
                    if (_cameFrom[nextX, nextY].fromX == int.MinValue) // MinValue means null in this context
                    {
                        _frontier.Enqueue((nextX, nextY));
                        _cameFrom[nextX, nextY] = (currX, currY);
                    }
                }
            }

            return null;
        }

        void ClearData()
        {
            for (int i = 0; i < GameMap.GridSizeX; i++)
                for (int j = 0; j < GameMap.GridSizeY; j++)
                    _cameFrom[i, j] = (int.MinValue, int.MinValue); // MinValue means null in this context

            _frontier.Clear();
        }

        List<Vector2Int> CreatePath(Vector2Int to)
        {
            var path = new List<Vector2Int> { to };
            var (x, y) = (to.x, to.y);
            while (_cameFrom[x, y].fromX != int.MinValue)
            {
                (x, y) = _cameFrom[x, y];
                path.Add(new Vector2Int(x, y));
            }

            path.Reverse();
            return path;
        }
    }
}