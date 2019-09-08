using Assets.Database;
using Assets.Database.DataModels;
using Assets.World.DataModels;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.World
{
    public sealed class ResourceChangedEventArgs
    {
        public List<Resource> Resources;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MapFeaturePrefabCollection))]
    public sealed class GameMap : MonoBehaviour
    {
        public const float CELL_SIZE = 10f;

        // to circumnavigate the regular anonymous method declaration limitation
        internal delegate void ActionRefStruct<T1>(ref GridCell cell);
        internal delegate bool FunctionRefStruct<T1>(ref GridCell cell);

        public static MapFeaturePrefabCollection MapFeaturePrefabCollection;

        internal static GameMap Instance;
        internal static readonly Repository DB = new Repository();

        #region Properties
        [Header("These parameters are applied only at the start of the game.")]
        [Range(1, 100)] [SerializeField] int _gridSizeX = 16;
        public static int GridSizeX => Instance._gridSizeX;   // to bypass Range ATR only to variables appliance
        [Range(1, 100)] [SerializeField] int _gridSizeY = 16; // as well as to not break the Instance encapsulation
        public static int GridSizeY => Instance._gridSizeX; // same as above
        #endregion

        [Header("Turn on to see gizmos indicating which cells are occupied.")]
        [SerializeField] bool _debugDrawOccupied; // gizmos need to be turned on in the editor in order to make it work

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();
        readonly GridCell[,] _cells;
        readonly float _localOffsetX, _localOffsetZ; // to allow designers to put the plane in an arbitrary position in the world space
        readonly GridShaderAdapter _gridShaderAdapter = new GridShaderAdapter(); // grid shader params

        PathFinder _pathFinder;

        Vehicle _selectedVehicle;
        internal Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (value == null || value != _selectedVehicle)
                    _pathIsDirty = true;

                _selectedVehicle = value;
            }
        }

        Vector2Int _targetCell; // this value has not meaning if SelectedVehicle is null
        bool _pathIsDirty; // indicates if PathFinder should recalculate the path
        List<Vector2Int> _path;

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
            MapFeaturePrefabCollection = GetComponent<MapFeaturePrefabCollection>();
            transform.localScale = new Vector3(_gridSizeX, 1, _gridSizeY);

            Material material = gameObject.GetComponent<MeshRenderer>().material;
            material.SetInt("_GridSizeX", _gridSizeX);
            material.SetInt("_GridSizeY", _gridSizeY);

            _gridShaderAdapter.InitializeCellTexture();

            _pathFinder = new PathFinder(Instance._cells);
        }

        void Update()
        {
#if UNITY_EDITOR
            if (_debugDrawOccupied)
                DebugDrawOccupied();
#endif

            UpdateTasks();

            if (EventSystem.current.IsPointerOverGameObject())
            {
                _gridShaderAdapter.ResetAllSelection();
                return;
            }

            if(!GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell))
            {
                _gridShaderAdapter.ResetAllSelection();
                return;
            }

            if (SelectedVehicle != null)
            {
                if(cell.Coordinates != _targetCell)
                {
                    _pathIsDirty = true;
                    _targetCell = cell.Coordinates;
                }

                if(_pathIsDirty)
                    _path = _pathFinder.FindPath(_selectedVehicle.Position, _targetCell);
            }

            if (_path != null)
                _gridShaderAdapter.SetData(_path, true);
        }

        void LateUpdate()
        {
            _scheduledTasks.AddRange(_taskBuffer);
            _taskBuffer.Clear();

            _gridShaderAdapter.SendDataToGPU();
        }
        #endregion

        // This constructor will be called by Unity Engine.
        // We need private constructor in order to be able to mark these variables as read-only
        // in order to allow compiler to perform extra optimizations, this is especially important in case of cells array 
        GameMap()
        {
            _cells = new GridCell[_gridSizeX, _gridSizeY];
            for (int i = 0; i < _gridSizeX; i++)
                for (int j = 0; j < _gridSizeY; j++)
                    _cells[i, j] = new GridCell(i, j);

            _localOffsetX = _gridSizeX * CELL_SIZE / 2;
            _localOffsetZ = _gridSizeY * CELL_SIZE / 2;
        }

        public static void PathFinderTest(Vector2Int from, Vector2Int to)
        {
            List<Vector2Int> path = Instance._pathFinder.FindPath(from, to);
            ResetSelection();
            
            foreach(var v in path)
                Instance._gridShaderAdapter[v] = true;
        }

        public static void BuildVehicle(VehicleType type, Vector2Int position)
        {
            var truck = new Vehicle(type, position);
            MarkAreaAsOccupied(position, new Vector2Int(1, 1), truck);
        }

        public static Building BuildBuilding(BuildingType type, Vector2Int position)
        {
            var building = new Building(type, position);
            MarkAreaAsOccupied(building);
            ResourceManager.RemoveResources(type);

            return building;
        }

        public static void RemoveBuilding(Building building, bool restoreResources = false)
        {
            MarkAreaAsFree(building);
            Destroy(building.GameObject);

            if (restoreResources)
                ResourceManager.AddResources(building.Type);
        }

        /// <summary>
        /// Moves building located from its current position to target cell.
        /// Last parameter allows you to specify whether the reallocation cost of the building 
        /// should be added or subtracted from player's resources.
        /// This can be useful, for example, for implementing undo operation.
        /// </summary>
        public static void MoveBuilding(Building b, Vector2Int to, bool addResources = false)
        {
            MarkAreaAsFree(b);

            b.Position = to;
            b.GameObject.transform.position = GetMiddlePoint(to, b.Size)
                .ApplyPrefabPositionOffset(b.Type);

            MarkAreaAsOccupied(b);

            if (addResources)
                ResourceManager.AddResources(b.ReallocationCost);
            else
                ResourceManager.RemoveResources(b.ReallocationCost);
        }

        /// <summary>
        /// Highlights the given cell.
        /// If the given coordinates points at already highlighted cell then it turn off cell's highlight.
        /// </summary>
        public static void HighlightCell(Vector2Int coord) => Instance._gridShaderAdapter[coord] = !Instance._gridShaderAdapter[coord];
                
        /// <summary>
        /// Highlights the given cell.
        /// If the given cell is already highlighted then the cell's highlight is turned off.
        /// </summary>
        public static void HighlightCell(ref GridCell cell) => HighlightCell(cell.Coordinates);

        /// <summary>
        /// Disables all highlight.
        /// </summary>
        public static void ResetSelection() => Instance._gridShaderAdapter.ResetAllSelection();

        public static bool GetCell(Ray ray, out GridCell cell)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var position = Instance.transform.InverseTransformPoint(hit.point);
                int x = (int)Mathf.Floor(Utils.Map(0, Instance._gridSizeX, -5, 5, position.x));
                int y = (int)Mathf.Floor(Utils.Map(0, Instance._gridSizeY, -5, 5, position.z));
                cell = Instance._cells[x, y];
                return true;
            }

            cell = new GridCell(); // dummy
            return false;
        }

        public ref GridCell GetCell(Vector2Int positon) => ref _cells[positon.x, positon.y];

        /// <summary>
        /// Returns world position of the center of the cell.
        /// </summary>
        public Vector3 GetCellMiddlePosition(ref GridCell cell)
        {
            Vector3 gridPos = transform.position;
            gridPos.x += cell.Coordinates.x * CELL_SIZE + CELL_SIZE / 2 - _localOffsetX;
            gridPos.z += cell.Coordinates.y * CELL_SIZE + CELL_SIZE / 2 - _localOffsetZ;

            return gridPos;
        }

        /// <summary>
        /// Returns world position of the center of the cell.
        /// </summary>
        public static Vector3 GetCellMiddlePosition(Vector2Int position)
        {
            Vector3 gridPos = Instance.transform.position;
            gridPos.x += position.x * CELL_SIZE + CELL_SIZE / 2 - Instance._localOffsetX;
            gridPos.z += position.y * CELL_SIZE + CELL_SIZE / 2 - Instance._localOffsetZ;

            return gridPos;
        }

        /// <summary>
        /// Returns position in the world space of the point that is exactly in the middle of the given area.
        /// Position variable points at the left bottom corner cell of the area.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetMiddlePoint(Vector2Int position, Vector2Int areaSize)
        {
#if UNITY_EDITOR
            if (IsAreaOutOfBounds(position, areaSize))
                Debug.LogError("Invalid argument(s). Part of the area is out of the map.");
#endif

            Vector3 middle = GetCellLeftBottomPosition(position);
            middle.x += CELL_SIZE * areaSize.x / 2;
            middle.z += CELL_SIZE * areaSize.y / 2;

            return middle;
        }

        /// <summary>
        /// Returns position in the world space of the point that is exactly in the middle of the given area.
        /// The area is defined by the left bottom cell's coordinates and the given building's size.
        /// Important: This method automatically applies the prefab's position offset.
        /// If you want to get the position of the middle point without the offset please use a different overload.
        /// </summary>
        public static Vector3 GetMiddlePointWithOffset(Vector2Int position, BuildingType type)
            => GetMiddlePoint(position, DB[type].Size).ApplyPrefabPositionOffset(type);

        /// <summary>
        /// Returns position in the world space of the point that is exactly in the middle of the given area.
        /// The area is defined by the left bottom cell's coordinates and the given building's size.
        /// Important: This method automatically applies the prefab's position offset.
        /// If you want to get the position of the middle point without the offset please use a different overload.
        /// </summary>
        public static Vector3 GetMiddlePointWithOffset(Vector2Int position, VehicleType type)
            => GetMiddlePoint(position, DB[type].Size).ApplyPrefabPositionOffset(type);

        /// <summary>
        /// Returns true if the area of the given size is entirely free, false otherwise.
        /// Position variable points at the left bottom corner cell of the area.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAreaFree(Vector2Int position, Vector2Int areaSize)
            => !Instance._cells.Any(position, areaSize, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Returns true if the area of the size of the given building type is entirely free, false otherwise.
        /// Position variable points at the left bottom corner cell of the area.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAreaFree(Vector2Int position, BuildingType type) 
            => !Instance._cells.Any(position, DB[type].Size, (ref GridCell cell) => cell.IsOccupied);

        /// <summary>
        /// Returns true if the area of the given size is entirely free, false otherwise.
        /// Position variable points at the left bottom corner cell of the area.
        /// Additional parameter allow us to omit certain building from the check.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAreaFree(Vector2Int position, Vector2Int size, Building omit)
            => !Instance._cells.Any(position, size, (ref GridCell cell) => cell.IsOccupied && cell.MapObject != omit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAreaOutOfBounds(Vector2Int position, Vector2Int areaSize)
            => position.x < 0 || position.x + areaSize.x > Instance._gridSizeX 
            || position.y < 0 || position.y + areaSize.y > Instance._gridSizeY;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAreaOutOfBounds(Vector2Int position, BuildingType type)
            => position.x < 0 || position.x + DB[type].Size.x > Instance._gridSizeX 
            || position.y < 0 || position.y + DB[type].Size.y > Instance._gridSizeY;

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MarkAreaAsOccupied(Building b)
            => Instance._cells.All(b.Position, DB[b.Type].Size, (ref GridCell c) => c.MapObject = b);

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MarkAreaAsOccupied(Vector2Int position, Vector2Int areaSize, Vehicle v)
            => Instance._cells.All(position, areaSize, (ref GridCell c) => c.MapObject = v);

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MarkAreaAsFree(Vector2Int position, Vector2Int areaSize)
            => Instance._cells.All(position, areaSize, (ref GridCell cell) => cell.MapObject = null);

        /// <summary>
        /// Mark cells occupied by the given building as free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MarkAreaAsFree(Building building)
            => MarkAreaAsFree(building.Position, building.Size);

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector3 GetCellLeftBottomPosition(ref GridCell cell) => GetCellLeftBottomPosition(cell.Coordinates);

        /// <summary>
        /// Returns world position of the left bottom corner of the cell.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector3 GetCellLeftBottomPosition(Vector2Int position)
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

#if UNITY_EDITOR
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
#endif
    }
}
