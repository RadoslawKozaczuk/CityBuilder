using Assets.Database;
using Assets.Database.DataModels;
using Assets.World.DataModels;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.World
{
    public class ResourceChangedEventArgs
    {
        public List<Resource> Resources;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ResourceManager), typeof(BuildingPrefabCollection))]
    public class GameMap : MonoBehaviour
    {
        // to circumnavigate the regular anonymous method declaration limitation
        public delegate void ActionRefStruct<T1>(ref GridCell cell);
        public delegate bool FunctionRefStruct<T1>(ref GridCell cell);

        const float CELL_SIZE = 10f;

        public static GameMap Instance;

        public static BuildingPrefabCollection BuildingPrefabCollection;

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

        internal readonly AbstractDatabase Db = new DummyDatabase();
        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();

        GridCell[,] _cells;
        Material _material;
        float _localOffsetX, _localOffsetZ; // to allow designers to put the plane in an arbitrary position in the world space

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
            BuildingPrefabCollection = GetComponent<BuildingPrefabCollection>();
        }

        void Start()
        {
            _cells = new GridCell[_gridSizeX, _gridSizeY];
            for (int i = 0; i < _gridSizeX; i++)
                for (int j = 0; j < _gridSizeY; j++)
                    _cells[i, j] = new GridCell(i, j);

            _localOffsetX = _gridSizeX * CELL_SIZE / 2;
            _localOffsetZ = _gridSizeY * CELL_SIZE / 2;

            _material = gameObject.GetComponent<MeshRenderer>().material;
        }

        void Update()
        {
            if (Debug.isDebugBuild && _debugDrawOccupied)
                DebugDrawOccupied();

            UpdateTasks();
        }

        void LateUpdate()
        {
            _scheduledTasks.AddRange(_taskBuffer);
            _taskBuffer.Clear();
        }
        #endregion

        public static Building BuildBuilding(BuildingType type, Vector2Int position)
        {
            var building = new Building(type, position);
            MarkAreaAsOccupied(building);

            return building;
        }

        /// <summary>
        /// Highlights the given cell.
        /// If the given coordinates points at already highlighted cell then it turn off cell's highlight.
        /// </summary>
        public static void HighlightCell(Vector2Int coord) 
            => Instance.SelectedArea = coord.x == Instance.SelectedArea.x && coord.y == Instance.SelectedArea.y
                ? new Vector4(-1, -1, -1, -1)
                : new Vector4(coord.x, coord.y, coord.x, coord.y);

        /// <summary>
        /// Highlights the given cell.
        /// If the given cell is already highlighted then the cell's highlight is turned off.
        /// </summary>
        public static void HighlightCell(ref GridCell cell) => HighlightCell(cell.Coordinates);

        public static void ResetSelection() => Instance.SelectedArea = new Vector4(-1, -1, -1, -1);

        public static bool GetCell(Ray ray, out GridCell cell)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var position = Instance.transform.InverseTransformPoint(hit.point);
                int x = (int)Mathf.Floor(Utils.Map(0, Instance._gridSizeX, -5, 5, position.x));
                int y = (int)Mathf.Floor(Utils.Map(0, Instance._gridSizeY, -5, 5, position.z));
                cell =Instance._cells[x, y];
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
            Vector3 gridPos = Instance.transform.position;
            gridPos.x += coordX * CELL_SIZE + CELL_SIZE / 2 - Instance._localOffsetX;
            gridPos.z += coordY * CELL_SIZE + CELL_SIZE / 2 - Instance._localOffsetZ;

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

        /// <summary>
        /// Returns position in the world space of the point that is exactly in the middle of the given area.
        /// Position variable points at the left bottom corner cell of the area.
        /// </summary>
        public static Vector3 GetMiddlePoint(Vector2Int position, Vector2Int areaSize)
        {
            // debug assertions
            if (Debug.isDebugBuild)
                if (areaSize.x < 1 || areaSize.y < 1)
                    Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 middle = GetCellLeftBottomPosition(position);
            middle.x += CELL_SIZE * areaSize.x / 2;
            middle.z += CELL_SIZE * areaSize.y / 2;

            return middle;
        }

        /// <summary>
        /// Returns position in the world space of the point that is exactly in the middle of the given area.
        /// The area is defined by the left bottom cell's coordinates and the given building's size.
        /// This method automatically applies the prefab's position offset.
        /// If you want to get the position of the middle point without the offset please use different overload.
        /// </summary>
        public static Vector3 GetMiddlePoint(Vector2Int position, BuildingType type)
        {
            Vector2Int size = Instance.Db[type].Size;

            // debug assertions
            if (Debug.isDebugBuild)
                if (size.x < 1 || size.y < 1)
                    Debug.LogError("Invalid argument(s). Size need to be a positive number.");

            Vector3 middle = GetCellLeftBottomPosition(position);
            middle.x += CELL_SIZE * size.x / 2;
            middle.z += CELL_SIZE * size.y / 2;

            return middle.ApplyPrefabPositionOffset(type);
        }

        /// <summary>
        /// Returns true if the area of the given size is entirely free, false otherwise.
        /// Position variable points at the left bottom corner cell of the area.
        /// </summary>
        public static bool IsAreaFree(Vector2Int position, Vector2Int areaSize)
            => !Instance._cells.Any(position, areaSize, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Returns true if the area of the size of the given building type is entirely free, false otherwise.
        /// Position variable points at the left bottom corner cell of the area.
        /// </summary>
        public static bool IsAreaFree(Vector2Int position, BuildingType type)
            => !Instance._cells.Any(position, Instance.Db[type].Size, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Returns true if the area of the given size is entirely free, false otherwise.
        /// Position variable points at the left bottom corner cell of the area.
        /// Additional parameter allow us to omit certain building from the check.
        /// </summary>
        public static bool IsAreaFree(Vector2Int position, Vector2Int size, Building omit) 
            => !Instance._cells.Any(position, size, (ref GridCell cell) => cell.IsOccupied && cell.Building != omit);

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        static void MarkAreaAsOccupied(Building b) 
            => Instance._cells.All(b.Position, Instance.Db[b.Type].Size, (ref GridCell c) => c.Building = b);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        static void MarkAreaAsFree(Vector2Int position, Vector2Int areaSize) 
            => Instance._cells.All(position, areaSize, (ref GridCell cell) => cell.Building = null);

        public static bool IsAreaOutOfBounds(Vector2Int position, Vector2Int areaSize) 
            => position.x < 0 || position.y < 0 || position.x + areaSize.x > Instance._gridSizeX || position.y + areaSize.y > Instance._gridSizeY;

        public static bool IsAreaOutOfBounds(Vector2Int position, BuildingType type)
            => position.x < 0 || position.y < 0 || position.x + Instance.Db[type].Size.x > Instance._gridSizeX || position.y + Instance.Db[type].Size.y > Instance._gridSizeY;

        /// <summary>
        /// Moves building located in the current cell to target cell.
        /// </summary>
        public static void MoveBuilding(Building b, Vector2Int to, ResourceOperationType rot = ResourceOperationType.Remove)
        {
            b.Position = to;
            b.GameObject.transform.position = GetMiddlePoint(to, b.Size)
               .ApplyPrefabPositionOffset(b.Type);
        }

        static Vector3 GetCellLeftBottomPositionInternal(Vector2Int position)
        {
            Vector3 pos = Instance.transform.position;
            pos.x += position.x * CELL_SIZE - Instance._localOffsetX;
            pos.z += position.y * CELL_SIZE - Instance._localOffsetZ;

            return pos;
        }
        
        /// <summary>
        /// Add BuildingTask object to the task buffer.
        /// </summary>
        public void ScheduleTask(BuildingTask task) => _taskBuffer.Add(task);

        void UpdateTasks()
        {
            for (int i = 0; i < _scheduledTasks.Count; i++)
            {
                BuildingTask task = _scheduledTasks[i];
                task.TimeLeft -= Time.deltaTime;

                if (task.TimeLeft > 0)
                    return;

                task.ActionOnFinish();
                _scheduledTasks.RemoveAt(i--);
            }
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
