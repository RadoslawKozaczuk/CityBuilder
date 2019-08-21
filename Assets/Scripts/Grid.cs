using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using UnityEngine;

namespace Assets.Scripts
{
    public class Grid : MonoBehaviour
    {
        // to circumnavigate the regular anonymous method declaration limitation
        public delegate void ActionRefStruct<T1>(ref GridCell cell);
        public delegate bool FunctionRefStruct<T1>(ref GridCell cell);

        const float CELL_SIZE = 10f;

        public static Grid Instance { get; private set; }

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
                    _cells[i, j] = new GridCell() { X = i, Y = j };

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
        public void SelectCell(int x, int y) => SelectCellInternal(x, y);

        /// <summary>
        /// Highlights the given cell.
        /// If the given cell is already selected then the cell is deselected.
        /// </summary>
        public void SelectCell(ref GridCell cell) => SelectCellInternal(cell.X, cell.Y);

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
        public Vector3 GetCellLeftBottomPosition(ref GridCell cell) => GetCellLeftBottomPositionInternal(cell.X, cell.Y);

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        public Vector3 GetCellLeftBottomPosition(int x, int y) => GetCellLeftBottomPositionInternal(x, y);

        public Vector3 GetMiddlePoint(int coordX, int coordY, int sizeX, int sizeY)
        {
            if (sizeX < 1 || sizeY < 1)
                Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 leftBot = GetCellLeftBottomPosition(coordX, coordY);
            leftBot.x += CELL_SIZE * sizeX / 2;
            leftBot.z += CELL_SIZE * sizeY / 2;

            return leftBot;
        }

        public Vector3 GetMiddlePoint(Vector2Int leftBotCoord, BuildingType type)
        {
            int sizeX = _db[type].SizeX;
            int sizeY = _db[type].SizeY;

            if (sizeX < 1 || sizeY < 1)
                Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 leftBot = GetCellLeftBottomPosition(leftBotCoord.x, leftBotCoord.y);
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
        /// </summary>
        public bool IsAreaFree(Vector2Int location, BuildingType type) 
            => !_cells.Any(location.x, location.y, _db[type].SizeX, _db[type].SizeY, (ref GridCell cell) => cell.IsOccupied);

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
        public void MarkAreaAsOccupied(Building b) 
            => _cells.All(b.Position.x, b.Position.y, b.SizeX, b.SizeY, (ref GridCell c) => c.Building = b);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        public void MarkAreaAsFree(int x, int y, int sizeX, int sizeY) 
            => _cells.All(x, y, sizeX, sizeY, (ref GridCell cell) => cell.Building = null);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        public void MarkAreaAsFree(Vector2Int position, int sizeX, int sizeY)
            => _cells.All(position.x, position.y, sizeX, sizeY, (ref GridCell cell) => cell.Building = null);

        public bool IsAreaOutOfBounds(int x, int y, int sizeX, int sizeY) 
            => x < 0 || y < 0 || x + sizeX > _gridSizeX || y + sizeY > _gridSizeY;

        public bool IsAreaOutOfBounds(int x, int y, BuildingType type)
            => x < 0 || y < 0 || x + _db[type].SizeX > _gridSizeX || y + _db[type].SizeY > _gridSizeY;

        /// <summary>
        /// Moves building located in the current cell to target cell.
        /// </summary>
        public void MoveBuilding(ref GridCell from, ref GridCell to)
        {
            Building b = from.Building;
            b.Position = new Vector2Int(to.X, to.Y);
            b.GameObject.transform.position = GetMiddlePoint(to.X, to.Y, b.SizeX, b.SizeY)
               .ApplyPrefabPositionOffset(b.Type);
        }

        /// <summary>
        /// Moves building located in the current cell to target cell.
        /// </summary>
        public void MoveBuilding(Building b, Vector2Int to)
        {
            b.Position = to;
            b.GameObject.transform.position = GetMiddlePoint(to.x, to.y, b.SizeX, b.SizeY)
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

        Vector3 GetCellLeftBottomPositionInternal(int x, int y)
        {
            Vector3 pos = transform.position;
            pos.x += x * CELL_SIZE - _localOffsetX;
            pos.z += y * CELL_SIZE - _localOffsetZ;

            return pos;
        }

        void SelectCellInternal(int x, int y) 
            => SelectedArea = x == SelectedArea.x && y == SelectedArea.y
                ? new Vector4(-1, -1, -1, -1)
                : new Vector4(x, y, x, y);

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
