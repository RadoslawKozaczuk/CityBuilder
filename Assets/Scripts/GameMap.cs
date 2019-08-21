﻿using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameMap : MonoBehaviour
    {
        // to circumnavigate the regular anonymous method declaration limitation
        public delegate void ActionRefStruct<T1>(ref GridCell cell);
        public delegate bool FunctionRefStruct<T1>(ref GridCell cell);

        const float CELL_SIZE = 10f;

        public static GameMap Instance { get; private set; }

        // to allow designers to put the plane in an arbitrary position in the world space
        [SerializeField] float _localOffsetX, _localOffsetZ;
        [SerializeField] int _gridSizeX = 12, _gridSizeY = 12;
        [SerializeField] bool _debugDrawOccupied;

        Vector4 _selectedArea = new Vector4(-1, -1, -1, -1);
        Vector4 SelectedArea
        {
            get => _selectedArea;
            set
            {
                _selectedArea = value;
                _material.SetVector("_SelectedArea", _selectedArea);
            }
        }

        GridCell[,] _cells;
        Material _material;
        readonly DummyDatabase _db = new DummyDatabase();

        #region Unity life-cycle methods
        void Awake() => Instance = this;

        void Start()
        {
            _cells = new GridCell[_gridSizeX, _gridSizeY];
            for (int i = 0; i < _gridSizeX; i++)
                for (int j = 0; j < _gridSizeY; j++)
                    _cells[i, j] = new GridCell() { Coordinates = new Vector2Int(i, j) };

            _localOffsetX = _gridSizeX * CELL_SIZE / 2;
            _localOffsetZ = _gridSizeY * CELL_SIZE / 2;

            _material = gameObject.GetComponent<MeshRenderer>().material;
        }

        void Update()
        {
            if (Debug.isDebugBuild && _debugDrawOccupied)
                DebugDrawOccupied();
        }
        #endregion

        /// <summary>
        /// Highlights the given cell.
        /// If the coordinates points at already selected cell then the cell is deselected.
        /// </summary>
        //public void SelectCell(int x, int y) => SelectCellInternal(x, y);

        /// <summary>
        /// Highlights the given cell.
        /// If the given cell is already selected then the cell is deselected.
        /// </summary>
        public void SelectCell(ref GridCell cell) => SelectCellInternal(cell.Coordinates);

        public void ResetSelection() => SelectedArea = new Vector4(-1, -1, -1, -1);

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

        public ref GridCell GetCell(Vector2Int positon)
        {
            if (positon.x < 0 || positon.y < 0 || positon.x >= _gridSizeX || positon.y >= _gridSizeY)
                throw new System.Exception("Index out of bounds");

            return ref _cells[positon.x, positon.y];
        }

        /// <summary>
        /// Returns world position of the center of the cell.
        /// </summary>
        public Vector3 GetCellMiddlePosition(GridCell cell)
        {
            Vector3 gridPos = transform.position;
            gridPos.x += cell.Coordinates.x * CELL_SIZE + CELL_SIZE / 2 - _localOffsetX;
            gridPos.z += cell.Coordinates.y * CELL_SIZE + CELL_SIZE / 2 - _localOffsetZ;

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
        public Vector3 GetCellLeftBottomPosition(ref GridCell cell) => GetCellLeftBottomPositionInternal(cell.Coordinates);

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        public Vector3 GetCellLeftBottomPosition(Vector2Int coords) => GetCellLeftBottomPositionInternal(coords);

        public Vector3 GetMiddlePoint(Vector2Int coords, Vector2Int areaSize)
        {
            if (areaSize.x < 1 || areaSize.y < 1)
                Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 leftBot = GetCellLeftBottomPosition(coords);
            leftBot.x += CELL_SIZE * areaSize.x / 2;
            leftBot.z += CELL_SIZE * areaSize.y / 2;

            return leftBot;
        }

        public Vector3 GetMiddlePoint(Vector2Int leftBotCoord, BuildingType type)
        {
            Vector2Int size = _db[type].Size;

            if (size.x < 1 || size.y < 1)
                Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 leftBot = GetCellLeftBottomPosition(leftBotCoord);
            leftBot.x += CELL_SIZE * size.x / 2;
            leftBot.z += CELL_SIZE * size.y / 2;

            return leftBot;
        }

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// </summary>
        public bool IsAreaFree(Vector2Int position, Vector2Int areaSize) 
            => !_cells.Any(position, areaSize, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// </summary>
        public bool IsAreaFree(Vector2Int position, BuildingType type) 
            => !_cells.Any(position, _db[type].Size, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// Additional parameter allow us to exclude certain building.
        /// </summary>
        public bool IsAreaFree(Vector2Int position, Vector2Int size, Building exclude) 
            => !_cells.Any(position, size, (ref GridCell cell) => cell.IsOccupied && cell.Building != exclude);

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        public void MarkAreaAsOccupied(Building b) 
            => _cells.All(b.Position, _db[b.Type].Size, (ref GridCell c) => c.Building = b);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        public void MarkAreaAsFree(Vector2Int position, Vector2Int areaSize) 
            => _cells.All(position, areaSize, (ref GridCell cell) => cell.Building = null);

        public bool IsAreaOutOfBounds(Vector2Int position, Vector2Int areaSize) 
            => position.x < 0 || position.y < 0 || position.x + areaSize.x > _gridSizeX || position.y + areaSize.y > _gridSizeY;

        public bool IsAreaOutOfBounds(Vector2Int position, BuildingType type)
            => position.x < 0 || position.y < 0 || position.x + _db[type].Size.x > _gridSizeX || position.y + _db[type].Size.y > _gridSizeY;

        /// <summary>
        /// Moves building located in the current cell to target cell.
        /// </summary>
        public void MoveBuilding(ref GridCell from, ref GridCell to)
        {
            Building b = from.Building;
            b.Position = to.Coordinates;
            b.GameObject.transform.position = GetMiddlePoint(to.Coordinates, b.Size)
               .ApplyPrefabPositionOffset(b.Type);
        }

        /// <summary>
        /// Moves building located in the current cell to target cell.
        /// </summary>
        public void MoveBuilding(Building b, Vector2Int to)
        {
            b.Position = to;
            b.GameObject.transform.position = GetMiddlePoint(to, b.Size)
               .ApplyPrefabPositionOffset(b.Type);
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

        Vector3 GetCellLeftBottomPositionInternal(Vector2Int coords)
        {
            Vector3 pos = transform.position;
            pos.x += coords.x * CELL_SIZE - _localOffsetX;
            pos.z += coords.y * CELL_SIZE - _localOffsetZ;

            return pos;
        }

        void SelectCellInternal(Vector2Int coord) 
            => SelectedArea = coord.x == SelectedArea.x && coord.y == SelectedArea.y
                ? new Vector4(-1, -1, -1, -1)
                : new Vector4(coord.x, coord.y, coord.x, coord.y);

        void DebugDrawOccupied()
        {
            for (int x = 0; x < _gridSizeX; x++)
                for (int y = 0; y < _gridSizeY; y++)
                {
                    ref GridCell cell = ref _cells[x, y];

                    if (cell.IsOccupied)
                    {
                        Vector3 leftBottomCorner = GetCellLeftBottomPosition(ref cell);
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
