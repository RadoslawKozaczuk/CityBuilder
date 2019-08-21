using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using Assets.Scripts.Interfaces;
using Assets.Scripts.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance { get; private set; }

        public GameObject[] BuildingPrefabs;
        public readonly AbstractDatabase Db = new DummyDatabase();
        public GridCell? CellUnderCursorCached;

        [SerializeField] BuildingInfoUI _buildingInfoUI;

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();

        ICommand _pendingAction;
        GameObject _hologram;
        BuildingType _type;

        #region Unity life-cycle methods
        void Awake() => Instance = this;

        void Update()
        {
            // some often used values are cached for performance reasons
            CellUnderCursorCached = EventSystem.current.IsPointerOverGameObject() 
                || !GameMap.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell)
                ? null
                : (GridCell?)cell;

            ProcessInput();
            UpdateTasks();

            UpdatePendingTaskBasedOnMousePosition();
        }

        void LateUpdate()
        {
            _scheduledTasks.AddRange(_taskBuffer);
            _taskBuffer.Clear();
        }
        #endregion

        void UpdatePendingTaskBasedOnMousePosition()
        {
            if (_pendingAction != null)
            {
                // Check if this is valid context to execute this command
                if (!_pendingAction.CheckExecutionContext())
                {
                    _hologram.SetActive(false);
                    return;
                }

                _hologram.SetActive(true);

                _hologram.GetComponent<MeshRenderer>().material = MaterialManager.GetMaterial(_pendingAction.CheckConditions()
                    ? CommonMaterialType.HolographicGreen
                    : CommonMaterialType.HolographicRed);

                _hologram.transform.position = GameMap.GetMiddlePoint(CellUnderCursorCached.Value.Coordinates, _type)
                    .ApplyPrefabPositionOffset(_type);
            }
        }

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

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_pendingAction != null)
                {
                    if(_pendingAction.Call())
                    {
                        Destroy(_hologram);
                        _pendingAction = null;
                    }
                }
                else if(CellUnderCursorCached.HasValue)
                {
                    if (CellUnderCursorCached.Value.IsOccupied)
                        ShowBuildingInfo(CellUnderCursorCached.Value);
                    else
                        HideBuildingInfo();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
                ExplosionManager.Instance.SpawnRandomExplosion();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_pendingAction != null)
                {
                    Destroy(_hologram);
                    _pendingAction = null;
                }
                else
                    Application.Quit();
            }
        }

        /// <summary>
        /// Add BuildingTask object to the task buffer.
        /// </summary>
        public void ScheduleTask(BuildingTask task) => _taskBuffer.Add(task);

        public void StartBuildingConstruction(BuildingType type)
        {
            _type = type;
            _hologram = InstanciateHologram(type);
            _pendingAction = new ConstructBuildingCommand(type);
        }

        public void StartBuildingReallocation(Building b)
        {
            _type = b.Type;
            _hologram = InstanciateHologram(b.Type);
            _pendingAction = new MoveBuildingCommand(b);
        }

        GameObject InstanciateHologram(BuildingType type)
        {
            GameObject prefab = BuildingPrefabs[(int)type];
            GameObject instance = Instantiate(prefab);
            instance.GetComponent<MeshRenderer>().material = MaterialManager.GetMaterial(CommonMaterialType.HolographicGreen);
            instance.SetActive(false);

            return instance;
        }

        void ShowBuildingInfo(GridCell cell)
        {
            _buildingInfoUI.Building = cell.Building;
            _buildingInfoUI.gameObject.SetActive(true);
            _buildingInfoUI.gameObject.transform.position = Input.mousePosition;
        }

        void HideBuildingInfo() => _buildingInfoUI.gameObject.SetActive(false);
    }
}