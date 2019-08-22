using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using UnityEngine;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public class GameMap : MonoBehaviour
    {
        // to circumnavigate the regular anonymous method declaration limitation
        public delegate void ActionRefStruct<T1>(ref GridCell cell);
        public delegate bool FunctionRefStruct<T1>(ref GridCell cell);

        const float CELL_SIZE = 10f;

        static GameMap _instance;

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

        // editor params
        [Range(1, 100)] [SerializeField] int _gridSizeX = 12;
        [Range(1, 100)] [SerializeField] int _gridSizeY = 12;
        [SerializeField] bool _debugDrawOccupied;

        readonly AbstractDatabase _db = new DummyDatabase();

        GridCell[,] _cells;
        Material _material;
        float _localOffsetX, _localOffsetZ; // to allow designers to put the plane in an arbitrary position in the world space

        #region Unity life-cycle methods
        void Awake() => _instance = this;

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
        /// If the given coordinates points at already highlighted cell then it turn off cell's highlight.
        /// </summary>
        public static void SelectCell(Vector2Int coord) 
            => _instance.SelectedArea = coord.x == _instance.SelectedArea.x && coord.y == _instance.SelectedArea.y
                ? new Vector4(-1, -1, -1, -1)
                : new Vector4(coord.x, coord.y, coord.x, coord.y);

        /// <summary>
        /// Highlights the given cell.
        /// If the given cell is already highlighted then the cell's highlight is turned off.
        /// </summary>
        public static void SelectCell(ref GridCell cell) => SelectCell(cell.Coordinates);

        public static void ResetSelection() => _instance.SelectedArea = new Vector4(-1, -1, -1, -1);

        public static bool GetCell(Ray ray, out GridCell cell)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var position = _instance.transform.InverseTransformPoint(hit.point);
                int x = (int)Mathf.Floor(Utils.Map(0, _instance._gridSizeX, -5, 5, position.x));
                int y = (int)Mathf.Floor(Utils.Map(0, _instance._gridSizeY, -5, 5, position.z));
                cell =_instance._cells[x, y];
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
        public static Vector3 GetCellMiddlePosition(int coordX, int coordY)
        {
            Vector3 gridPos = _instance.transform.position;
            gridPos.x += coordX * CELL_SIZE + CELL_SIZE / 2 - _instance._localOffsetX;
            gridPos.z += coordY * CELL_SIZE + CELL_SIZE / 2 - _instance._localOffsetZ;

            return gridPos;
        }

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        public static Vector3 GetCellLeftBottomPosition(ref GridCell cell) => GetCellLeftBottomPositionInternal(cell.Coordinates);

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        public static Vector3 GetCellLeftBottomPosition(Vector2Int position) => GetCellLeftBottomPositionInternal(position);

        public static Vector3 GetMiddlePoint(Vector2Int position, Vector2Int areaSize)
        {
            if (areaSize.x < 1 || areaSize.y < 1)
                Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 leftBot = GetCellLeftBottomPosition(position);
            leftBot.x += CELL_SIZE * areaSize.x / 2;
            leftBot.z += CELL_SIZE * areaSize.y / 2;

            return leftBot;
        }

        public static Vector3 GetMiddlePoint(Vector2Int position, BuildingType type)
        {
            Vector2Int size = _instance._db[type].Size;

            if (size.x < 1 || size.y < 1)
                Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 leftBot = GetCellLeftBottomPosition(position);
            leftBot.x += CELL_SIZE * size.x / 2;
            leftBot.z += CELL_SIZE * size.y / 2;

            return leftBot;
        }

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// </summary>
        public static bool IsAreaFree(Vector2Int position, Vector2Int areaSize) 
            => !_instance._cells.Any(position, areaSize, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// </summary>
        public static bool IsAreaFree(Vector2Int position, BuildingType type) 
            => !_instance._cells.Any(position, _instance._db[type].Size, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// Additional parameter allow us to exclude certain building.
        /// </summary>
        public static bool IsAreaFree(Vector2Int position, Vector2Int size, Building exclude) 
            => !_instance._cells.Any(position, size, (ref GridCell cell) => cell.IsOccupied && cell.Building != exclude);

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        public static void MarkAreaAsOccupied(Building b) 
            => _instance._cells.All(b.Position, _instance._db[b.Type].Size, (ref GridCell c) => c.Building = b);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        public static void MarkAreaAsFree(Vector2Int position, Vector2Int areaSize) 
            => _instance._cells.All(position, areaSize, (ref GridCell cell) => cell.Building = null);

        public static bool IsAreaOutOfBounds(Vector2Int position, Vector2Int areaSize) 
            => position.x < 0 || position.y < 0 || position.x + areaSize.x > _instance._gridSizeX || position.y + areaSize.y > _instance._gridSizeY;

        public static bool IsAreaOutOfBounds(Vector2Int position, BuildingType type)
            => position.x < 0 || position.y < 0 || position.x + _instance._db[type].Size.x > _instance._gridSizeX || position.y + _instance._db[type].Size.y > _instance._gridSizeY;

        /// <summary>
        /// Moves building located in the current cell to target cell.
        /// </summary>
        public static void MoveBuilding(Building b, Vector2Int to)
        {
            b.Position = to;
            b.GameObject.transform.position = GetMiddlePoint(to, b.Size)
               .ApplyPrefabPositionOffset(b.Type);
        }

        static Vector3 GetCellLeftBottomPositionInternal(Vector2Int position)
        {
            Vector3 pos = _instance.transform.position;
            pos.x += position.x * CELL_SIZE - _instance._localOffsetX;
            pos.z += position.y * CELL_SIZE - _instance._localOffsetZ;

            return pos;
        }
         
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
