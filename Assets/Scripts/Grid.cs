﻿using Assets.Scripts.DataModels;
using UnityEngine;

namespace Assets.Scripts
{
    public class Grid : MonoBehaviour
    {
        // to circumnavigate the regular anonymous method declaration limitation
        public delegate void ActionRefStruct<T1>(ref GridCell cell);
        public delegate bool FunctionRefStruct<T1>(ref GridCell cell);

        const float CELL_SIZE = 10f;

        // to allow designers to put the plane in an arbitrary position in the world space
        [SerializeField] float _localOffsetX, _localOffsetZ;
        [SerializeField] int _gridSizeX = 12, _gridSizeY = 12;
        [SerializeField] bool _debugDrawOccupied;

        GridCell[,] _cells;

        #region Unity life-cycle methods
        void Start()
        {
            _cells = new GridCell[_gridSizeX, _gridSizeY];
            for (int i = 0; i < _gridSizeX; i++)
                for (int j = 0; j < _gridSizeY; j++)
                    _cells[i, j] = new GridCell() { X = i, Y = j };

            _localOffsetX = _gridSizeX * CELL_SIZE / 2;
            _localOffsetZ = _gridSizeY * CELL_SIZE / 2;
        }

        void Update()
        {
            if (Debug.isDebugBuild && _debugDrawOccupied)
                DebugDrawOccupied();
        }
        #endregion

        public bool GetCell(Ray ray, out GridCell cell)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                cell = GetCell(hit.point);
                return true;
            }

            cell = new GridCell(); // dummy
            return false;
        }

        /// <summary>
        /// Returns world position of the center of the cell.
        /// </summary>
        public Vector3 GetCellMiddlePosition(GridCell cell)
        {
            Vector3 gridPos = transform.position;
            gridPos.x += cell.X * CELL_SIZE + CELL_SIZE / 2 - _localOffsetX;
            gridPos.z += cell.Y * CELL_SIZE + CELL_SIZE / 2 - _localOffsetZ;

            return gridPos;
        }

        /// <summary>
        /// Returns world position of the center of the cell.
        /// </summary>
        public Vector3 GetCellMiddlePosition(int coordX, int coordY)
        {
            Vector3 gridPos = transform.position;
            gridPos.x += coordX * CELL_SIZE + CELL_SIZE / 2 - _localOffsetX;
            gridPos.z += coordY * CELL_SIZE + CELL_SIZE / 2 - _localOffsetZ;

            return gridPos;
        }

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        public Vector3 GetCellLeftBottomPosition(GridCell cell)
        {
            Vector3 pos = transform.position;
            pos.x += cell.X * CELL_SIZE - _localOffsetX;
            pos.z += cell.Y * CELL_SIZE - _localOffsetZ;

            return pos;
        }

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        public Vector3 GetCellLeftBottomPosition(int coordX, int coordY)
        {
            Vector3 pos = transform.position;
            pos.x += coordX * CELL_SIZE - _localOffsetX;
            pos.z += coordY * CELL_SIZE - _localOffsetZ;

            return pos;
        }

        public Vector3 GetMiddlePoint(int coordX, int coordY, int sizeX, int sizeY)
        {
            if (sizeX < 1 || sizeY < 1)
                Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 leftBot = GetCellLeftBottomPosition(coordX, coordY);
            leftBot.x += CELL_SIZE * sizeX / 2;
            leftBot.z += CELL_SIZE * sizeY / 2;

            return leftBot;
        }

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// </summary>
        public bool IsAreaFree(int x, int y, int sizeX, int sizeY) 
            => !_cells.Any(x, y, sizeX, sizeY, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// Additional parameter allow us to exclude certain building.
        /// </summary>
        public bool IsAreaFree(int x, int y, int sizeX, int sizeY, Building exclude) 
            => !_cells.Any(x, y, sizeX, sizeY, (ref GridCell cell) => cell.IsOccupied && cell.Building != exclude);

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        public void MarkAreaAsOccupied(int x, int y, int sizeX, int sizeY, Building building) 
            => _cells.All(x, y, sizeX, sizeY, (ref GridCell cell) => cell.Building = building);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        public void MarkAreaAsFree(int x, int y, int sizeX, int sizeY) 
            => _cells.All(x, y, sizeX, sizeY, (ref GridCell cell) => cell.Building = null);

        public bool IsAreaOutOfBounds(int x, int y, int sizeX, int sizeY) 
            => x < 0 || y < 0 || x + sizeX > _gridSizeX || y + sizeY > _gridSizeY;

        // Get cell returns cell from a given position
        GridCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);

            int coordX = (int)Mathf.Floor(Utils.Map(0, _gridSizeX, -5, 5, position.x));
            int coordY = (int)Mathf.Floor(Utils.Map(0, _gridSizeY, -5, 5, position.z));
            Debug.Log("CellCoord: x: " + coordX + ", z: " + coordY);

            return _cells[coordX, coordY];
        }

        void DebugDrawOccupied()
        {
            for (int x = 0; x < _gridSizeX; x++)
                for (int y = 0; y < _gridSizeY; y++)
                {
                    GridCell cell = _cells[x, y];

                    if (cell.IsOccupied)
                    {
                        Vector3 leftBottomCorner = GetCellLeftBottomPosition(cell);
                        Vector3 rightBottomCorner = new Vector3() { x = leftBottomCorner.x + CELL_SIZE, z = leftBottomCorner.z };
                        Vector3 leftTopCorner = new Vector3() { x = leftBottomCorner.x, z = leftBottomCorner.z + CELL_SIZE };
                        Vector3 rightTopCorner = new Vector3() { x = leftBottomCorner.x + CELL_SIZE, z = leftBottomCorner.z + CELL_SIZE };

                        Debug.DrawLine(leftBottomCorner, rightTopCorner, Color.red, 0f, false);
                        Debug.DrawLine(rightBottomCorner, leftTopCorner, Color.red, 0f, false);
                    }
                }
        }
    }
}
