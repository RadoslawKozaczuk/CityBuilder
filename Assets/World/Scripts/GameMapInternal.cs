using Assets.Database;
using Assets.World.DataModels;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.World
{
    public sealed partial class GameMap
    {
        // to circumnavigate the regular anonymous method declaration limitation
        internal delegate void ActionRefStruct<T>(ref GridCell cell);
        internal delegate bool FunctionRefStruct<T>(ref GridCell cell);

        internal static GameMap Instance;
        internal static readonly Repository DB = new Repository();

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

        internal List<Vector2Int> Path;

        [Header("Turn on to see gizmos indicating which cells are occupied.")]
        [SerializeField] bool _debugDrawOccupied; // gizmos need to be turned on in the editor in order to make it work

        readonly GridCell[,] _cells;
        readonly float _localOffsetX, _localOffsetZ; // to allow designers to put the plane in an arbitrary position in the world space
        readonly List<Vehicle> _vehicles = new List<Vehicle>();
        GridShaderAdapter _gridShaderAdapter; // responsible for cell highlighting
        internal PathFinder PathFinder;
        Vector2Int _targetCell; // this value has not meaning if SelectedVehicle is null
        bool _pathIsDirty; // indicates if PathFinder should recalculate the path

        // This constructor will be called by Unity Engine.
        // We need private constructor in order to be able to mark these variables as read-only
        // in order to allow compiler to perform extra optimizations, this is especially important in case of cells array.
        GameMap()
        {
            _cells = new GridCell[_gridSizeX, _gridSizeY];
            for (int i = 0; i < _gridSizeX; i++)
                for (int j = 0; j < _gridSizeY; j++)
                    _cells[i, j] = new GridCell(i, j);

            _localOffsetX = _gridSizeX * CELL_SIZE / 2;
            _localOffsetZ = _gridSizeY * CELL_SIZE / 2;
        }

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
            MapFeaturePrefabCollection = GetComponent<MapFeaturePrefabCollection>();
            transform.localScale = new Vector3(_gridSizeX, 1, _gridSizeY);

            Material material = gameObject.GetComponent<MeshRenderer>().material;
            material.SetInt("_GridSizeX", _gridSizeX);
            material.SetInt("_GridSizeY", _gridSizeY);

            PathFinder = new PathFinder(Instance._cells);
            _gridShaderAdapter = new GridShaderAdapter();
        }

        void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_debugDrawOccupied)
                DebugDrawOccupied();
#endif

            // update related to vehicle
            VehicleSelectionUpdate();

            // update all ongoing tasks
            TaskManager.UpdateTasks();
        }

        void LateUpdate()
        {
            _gridShaderAdapter.SendDataToGPU(); // applies grid highlighting
            ExecutedCommandList.EndFrameSignal(); // if anything changes it will broadcast the new status
        }
        #endregion

        /// <summary>
        /// Attaches vehicle to another cell effectively making that cell occupied and the source cell free.
        /// </summary>
        internal static void MoveVehicle(Vehicle v, Vector2Int to)
        {
            MarkCellAsFree(v.Position);
            v.Position = to;
            MarkCellAsOccupied(to, v);
        }

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        internal static void BroadcastExecutedCommandsStatusChanged(string status) 
            => ExecutedCommandsStatusChangedEventHandler?.Invoke(Instance, new DebugLogEventArgs { Log = status });

        internal static void BroadcastTaskManagerStatusChanged(string status) 
            => TaskManagerStatusChangedEventHandler?.Invoke(Instance, new DebugLogEventArgs { Log = status });

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MarkAreaAsOccupied(Building b)
            => Instance._cells.All(b.Position, DB[b.Type].Size, (ref GridCell c) => c.MapObject = b);

        static void MarkCellAsOccupied(Vector2Int position, Vehicle v)
            => Instance._cells[position.x, position.y].MapObject = v;

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
        /// Marks cell as free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MarkCellAsFree(Vector2Int position) => Instance._cells[position.x, position.y].MapObject = null;

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

        static void VehicleSelectionUpdate()
        {
            if (Instance.SelectedVehicle == null || EventSystem.current.IsPointerOverGameObject())
            {
                Instance._gridShaderAdapter.ResetAllSelection();
                return;
            }

            if (Instance.SelectedVehicle == null || !GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell))
            {
                Instance._gridShaderAdapter.ResetAllSelection();
                return;
            }

            if (Instance.SelectedVehicle != null)
            {
                if (cell.Coordinates != Instance._targetCell)
                {
                    Instance._pathIsDirty = true;
                    Instance._targetCell = cell.Coordinates;
                }

                if (Instance._pathIsDirty)
                    Instance.Path = Instance.PathFinder.FindPath(Instance._selectedVehicle.Position, Instance._targetCell);
            }

            if (Instance.Path != null)
                Instance._gridShaderAdapter.SetData(Instance.Path, true);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
