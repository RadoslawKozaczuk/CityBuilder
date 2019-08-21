using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using Assets.Scripts.Interfaces;
using Assets.Scripts.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance { get; private set; }

        [SerializeField] BuildingInfoUI _buildingInfoUI;
        public GameObject[] BuildingPrefabs;

        ICommand _pendingAction;
        GameObject _hologram;
        BuildingType _type;

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();
        public readonly DummyDatabase Db = new DummyDatabase();

        public Ray CachedMousePositionRay;
        public GridCell? CachedCurrentCell;

        #region Unity life-cycle methods
        void Awake() => Instance = this;

        void Update()
        {
            // some often used values are cached for performance reasons
            CachedMousePositionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            CachedCurrentCell = EventSystem.current.IsPointerOverGameObject() || !GameMap.Instance.GetCell(CachedMousePositionRay, out GridCell cell)
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

                bool conditionsMet = _pendingAction.CheckConditions();

                Vector3 targetPos = GameMap.Instance.GetMiddlePoint(CachedCurrentCell.Value.Coordinates, Db[_type].Size)
                    .ApplyPrefabPositionOffset(_type);

                _hologram.GetComponent<MeshRenderer>().material = MaterialManager.Instance.GetMaterial(conditionsMet
                    ? CommonMaterialType.HolographicGreen
                    : CommonMaterialType.HolographicRed);

                _hologram.transform.position = targetPos;
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
                else if(CachedCurrentCell.HasValue)
                {
                    if (CachedCurrentCell.Value.IsOccupied)
                        ShowBuildingInfo(CachedCurrentCell.Value);
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
            instance.GetComponent<MeshRenderer>().material = MaterialManager.Instance.GetMaterial(CommonMaterialType.HolographicGreen);
            instance.SetActive(false);

            return instance;
        }

        void ShowBuildingInfo(GridCell cell)
        {
            _buildingInfoUI.Building = cell.Building;
            _buildingInfoUI.gameObject.SetActive(true);
            _buildingInfoUI.gameObject.transform.position = Input.mousePosition;
        }

        void HideBuildingInfo()
        {
            _buildingInfoUI.Building = null;
            _buildingInfoUI.gameObject.SetActive(false);
        }
    }
}