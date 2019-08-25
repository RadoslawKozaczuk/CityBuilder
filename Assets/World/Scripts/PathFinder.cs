using Assets.World.DataModels;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.World
{
    public class PathFinder
    {
        readonly (int xOffset, int yOffset)[] _directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };
        readonly (int fromX, int fromY)[,] _cameFrom;
        readonly GridCell[,] _cells;
        readonly Queue<(int x, int y)> frontier;

        public PathFinder(GridCell[,] cells)
        {
            _cells = cells;
            _cameFrom = new (int fromX, int fromY)[GameMap.GridSizeX, GameMap.GridSizeX]; // if visited then where from, otherwise minus infinity
            frontier = new Queue<(int x, int y)>();
        }

        public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
        {
            ClearData();

            // The key idea for all of these algorithms is that we keep track of an expanding ring called the frontier.
            frontier.Enqueue((from.x, from.y));

            while (frontier.Count > 0)
            {
                var (currX, currY) = frontier.Dequeue();

                if(currX == 1 && currY == 2)
                {
                    int trolll = 234;
                }

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
                    if (_cameFrom[nextX, nextY].fromX == int.MinValue)
                    {
                        frontier.Enqueue((nextX, nextY));
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
                    _cameFrom[i, j] = (int.MinValue, int.MinValue);

            frontier.Clear();
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