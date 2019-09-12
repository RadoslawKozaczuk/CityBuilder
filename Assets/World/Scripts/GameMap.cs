﻿using Assets.Database;
using Assets.World.Commands;
using Assets.World.DataModels;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.World
{
    public sealed class DebugLogEventArgs
    {
        public string Log;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MapFeaturePrefabCollection))]
    public sealed class GameMap : MonoBehaviour
    {
        public const float CELL_SIZE = 10f;

        // to circumnavigate the regular anonymous method declaration limitation
        internal delegate void ActionRefStruct<T>(ref GridCell cell);
        internal delegate bool FunctionRefStruct<T>(ref GridCell cell);

        public static MapFeaturePrefabCollection MapFeaturePrefabCollection;

        /// <summary>
        /// Subscribe to this event to receive notifications each time executed command list status has changed.
        /// </summary>
        public static event EventHandler<DebugLogEventArgs> ExecutedCommandsStatusChangedEventHandler;
        /// <summary>
        /// Subscribe to this event to receive notifications each time tasks status has changed.
        /// </summary>
        public static event EventHandler<DebugLogEventArgs> TaskManagerStatusChangedEventHandler;

        internal static GameMap Instance;
        internal static readonly Repository DB = new Repository();

        #region Properties
        [Header("These parameters are applied only at the start of the game.")]
        [Range(1, 100)] [SerializeField] int _gridSizeX = 16;
        public static int GridSizeX => Instance._gridSizeX;   // to bypass Range attribute only to variables appliance
        [Range(1, 100)] [SerializeField] int _gridSizeY = 16; // as well as to not break the Instance encapsulation (readonly)
        public static int GridSizeY => Instance._gridSizeX; // same as above

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
        #endregion

        internal List<Vector2Int> Path;

        [Header("Turn on to see gizmos indicating which cells are occupied.")]
        [SerializeField] bool _debugDrawOccupied; // gizmos need to be turned on in the editor in order to make it work

        readonly GridCell[,] _cells;
        readonly float _localOffsetX, _localOffsetZ; // to allow designers to put the plane in an arbitrary position in the world space
        GridShaderAdapter _gridShaderAdapter; // responsible for cell highlighting
        readonly List<Vehicle> _vehicles = new List<Vehicle>();

        PathFinder _pathFinder;
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

            _pathFinder = new PathFinder(Instance._cells);
            _gridShaderAdapter = new GridShaderAdapter();
        }

        void Update()
        {
#if UNITY_EDITOR
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
        /// Returns true if the player clicked on anything belonging to the game world.
        /// Additionally returns the building if the player clicked on such.
        /// </summary>
        public static bool ClickMe(out GridCell cell, out Building building)
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
                throw new System.Exception("ClickMe should not be called if the cursor is over the UI");
#endif

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                // player clicked outside of the world
                cell = new GridCell(); // dummy
                building = null;
                return false;
            }
            else
            {
                Collider collider = hit.collider;
                Building b = collider.gameObject.GetComponent<Building>();
                if (b != null)
                {
                    cell = Instance._cells[b.Position.x, b.Position.y];
                    building = b;
                    return true;
                }

                Vehicle v = collider.gameObject.GetComponent<Vehicle>();
                if(v != null)
                {
                    // player clicked on a vehicle
                    new SelectVehicleCommand(v).Call(); // call automatically puts the command in the execution command list

                    cell = Instance._cells[v.Position.x, v.Position.y];
                    building = null;
                    return true;
                }

                // player clicked on the ground
                GetCell(ray, out GridCell c);
                cell = c;
                new MoveVehicleCommand(Instance.SelectedVehicle, cell.Coordinates).Call();
                building = null;
                return false;
            }
        }

        public static void UndoLastCommand() => ExecutedCommandList.UndoLastCommand();

        public static void BuildVehicle(VehicleType type, Vector2Int position)
        {
            var instance = Instantiate(MapFeaturePrefabCollection[type]);
            Vehicle vehicle = instance.GetComponent<Vehicle>();
            vehicle.SetData(type, position);
            Instance._vehicles.Add(vehicle);
            MarkAreaAsOccupied(position, new Vector2Int(1, 1), vehicle);
        }

        public static Building BuildBuilding(BuildingType type, Vector2Int position)
        {
            var instance = Instantiate(MapFeaturePrefabCollection[type]);
            Building building = instance.GetComponent<Building>();
            building.SetData(type, position);
            MarkAreaAsOccupied(building);
            ResourceManager.RemoveResources(type);

            return building;
        }

        public static void RemoveBuilding(Building building, bool restoreResources = false)
        {
            MarkAreaAsFree(building);
            Destroy(building.gameObject);

            if (restoreResources)
                ResourceManager.AddResources(building.Type);
        }

        /// <summary>
        /// Moves building from its current position to the target cell.
        /// Last parameter allows us to specify whether the reallocation cost of the building 
        /// should be added or subtracted from player's resources.
        /// This can be useful, for example, for implementing undo operation.
        /// </summary>
        public static void MoveBuilding(Building b, Vector2Int to, bool addResources = false)
        {
            MarkAreaAsFree(b);

            b.Position = to;
            b.gameObject.transform.position = GetMiddlePoint(to, b.Size)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetMiddlePointWithOffset(Vector2Int position, BuildingType type)
            => GetMiddlePoint(position, DB[type].Size).ApplyPrefabPositionOffset(type);

        /// <summary>
        /// Returns position in the world space of the point that is exactly in the middle of the given area.
        /// The area is defined by the left bottom cell's coordinates and the given building's size.
        /// Important: This method automatically applies the prefab's position offset.
        /// If you want to get the position of the middle point without the offset please use a different overload.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public static bool IsAreaFree(Vector2Int position, Vector2Int size, Building omit) // used ReferenceEquals to suppress 'possible unintended reference comparison' warning
            => !Instance._cells.Any(position, size, (ref GridCell cell) => cell.IsOccupied && !ReferenceEquals(cell.MapObject, omit));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAreaOutOfBounds(Vector2Int position, Vector2Int areaSize)
            => position.x < 0 || position.x + areaSize.x > Instance._gridSizeX 
            || position.y < 0 || position.y + areaSize.y > Instance._gridSizeY;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAreaOutOfBounds(Vector2Int position, BuildingType type)
            => position.x < 0 || position.x + DB[type].Size.x > Instance._gridSizeX 
            || position.y < 0 || position.y + DB[type].Size.y > Instance._gridSizeY;

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
                    Instance.Path = Instance._pathFinder.FindPath(Instance._selectedVehicle.Position, Instance._targetCell);
            }

            if (Instance.Path != null)
                Instance._gridShaderAdapter.SetData(Instance.Path, true);
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
