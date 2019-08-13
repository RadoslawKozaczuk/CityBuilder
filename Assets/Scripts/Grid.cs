﻿using Assets.Scripts.DataModels;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Grid : MonoBehaviour
    {
        const float CELL_SIZE = 10f;

        // to allow designers to put the plane in an arbitrary position in the world space
        [SerializeField] float _localOffsetX, _localOffsetZ;
        [SerializeField] int _gridSizeX = 12, _gridSizeY = 12;
        [SerializeField] bool _debugDrawOccupied;

        GridCell[,] _cells;

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
        {
            if (!AreArgumentsCool(x, y, sizeX, sizeY))
                return false;

            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j < y + sizeY; j++)
                    if (_cells[i, j].IsOccupied)
                        return false;

            return true;
        }

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// Additional parameter allow us to exclude certain building.
        /// </summary>
        public bool IsAreaFree(int x, int y, int sizeX, int sizeY, Building exclude)
        {
            if (!AreArgumentsCool(x, y, sizeX, sizeY))
                return false;

            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j < y + sizeY; j++)
                    if (_cells[i, j].IsOccupied && _cells[i, j].Building != exclude)
                        return false;

            return true;
        }

        bool AreArgumentsCool(int x, int y, int sizeX, int sizeY)
        {
            if (x < 0 || y < 0)
            {
                Debug.LogError("Invalid argument. X and y coordinates must be equal or greater than zero.");
                return false;
            }

            if (x + sizeX > _gridSizeX || y + sizeY > _gridSizeY)
            {
                Debug.LogError("Argument out of bounds. The given area should not be out of the grid's bounds.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        public void MarkAreaAsOccupied(int x, int y, int sizeX, int sizeY, Building building) 
            => ExecuteActionPerCell(x, y, sizeX, sizeY, (i, j) => _cells[i, j].Building = building);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        public void MarkAreaAsFree(int x, int y, int sizeX, int sizeY) 
            => ExecuteActionPerCell(x, y, sizeX, sizeY, (i, j) => _cells[i, j].Building = null);

        void ExecuteActionPerCell(int x, int y, int sizeX, int sizeY, Action<int, int> action)
        {
            if (!AreArgumentsCool(x, y, sizeX, sizeY))
                return;

            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j < y + sizeY; j++)
                    action(i, j);
        }

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
