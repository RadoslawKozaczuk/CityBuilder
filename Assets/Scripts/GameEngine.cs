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

        public Grid GameMap;

        [SerializeField] BuildingInfoUI _buildingInfoUI;
        public GameObject[] BuildingPrefabs;

        InterfacePendingAction _interfacePendingAction;
        ICommand _pendingAction;
        GameObject _hologram;
        BuildingType _typeTroll;

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();
        public readonly DummyDatabase Db = new DummyDatabase();

        public Ray CachedMousePositionRay;
        public GridCell? CachedCurrentCell;

        #region Unity life-cycle methods
        void Awake() => Instance = this;

        void Update()
        {
            // update cached values - some often used values are cached for performance resons
            CachedMousePositionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            CachedCurrentCell = GameMap.GetCell(CachedMousePositionRay, out GridCell cell) ? (GridCell?)cell : null;

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

                Vector3 targetPos = GameMap.GetMiddlePoint(CachedCurrentCell.Value.X, CachedCurrentCell.Value.Y, 3, 3)
                    .ApplyPrefabPositionOffset(BuildingType.Residence);

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
            }

            if (Input.GetKeyDown(KeyCode.Space))
                ExplosionManager.Instance.SpawnRandomExplosion();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_interfacePendingAction != null)
                    ClearPendingActionParams();
                else
                    Application.Quit();
            }
        }

        /// <summary>
        /// Add BuildingTask object to the task buffer.
        /// </summary>
        public void ScheduleTask(BuildingTask task) => _taskBuffer.Add(task);

        public void BuildingConstructionAction(BuildingType type)
        {
            // check if player has enough resources
            BuildingData data = Db[type];
            if (!ResourceManager.IsEnoughResources(data.Cost))
                return;

            _hologram = InstanciateHologram(type);
            _pendingAction = new ConstructBuildingCommand(type);
        }

        public bool TryGibMeClickedCell(out Vector2Int cellCoord)
        {
            if (EventSystem.current.IsPointerOverGameObject()
                || !GameMap.GetCell(CachedMousePositionRay, out GridCell cell))
            {
                cellCoord = Vector2Int.zero;
                return false;
            }

            cellCoord = new Vector2Int(cell.X, cell.Y);
            return true;
        }

        GameObject InstanciateHologram(BuildingType type)
        {
            GameObject prefab = BuildingPrefabs[(int)type];
            GameObject instance = Instantiate(prefab);
            instance.GetComponent<MeshRenderer>().material = MaterialManager.Instance.GetMaterial(CommonMaterialType.HolographicGreen);
            instance.SetActive(false);

            return instance;
        }

        void BuildingConstructionFollowUpAction(Dictionary<UIPendingActionParam, object> parameters)
        {
            parameters.TryGetValue(UIPendingActionParam.CurrentCell, out object obj);
            var cell = (GridCell)obj;
            parameters.TryGetValue(UIPendingActionParam.Building, out obj);
            var b = (Building)obj;

            // enough space
            ResourceManager.RemoveResources(Db[b.Type].Cost);

            b.GameObject.SetActive(true);
            b.Position = new Vector2Int(cell.X, cell.Y);
            b.UseDefaultMaterial();
        }

        public void BuildingReallocationAction(Building b)
        {
            BuildingData data = Db[b.Type];
            var hologram = new Building(ref data);
            hologram.UseCommonMaterial(CommonMaterialType.HolographicGreen);

            _interfacePendingAction = new InterfacePendingAction();
            _interfacePendingAction.Parameters.Add(UIPendingActionParam.PreviousCell, GameMap.GetCell(b.Position));
            _interfacePendingAction.Parameters.Add(UIPendingActionParam.Building, hologram);
            _interfacePendingAction.PendingAction = BuildingReallocationFollowUpAction;
        }

        void BuildingReallocationFollowUpAction(Dictionary<UIPendingActionParam, object> parameters)
        {
            parameters.TryGetValue(UIPendingActionParam.PreviousCell, out object obj);
            var currentCell = (GridCell)obj;
            parameters.TryGetValue(UIPendingActionParam.CurrentCell, out obj);
            var targetCell = (GridCell)obj;
            parameters.TryGetValue(UIPendingActionParam.Building, out obj);
            var hologram = (Building)obj;

            ResourceManager.RemoveResource(hologram.ReallocationCost);

            hologram.GameObject.SetActive(false);
            Destroy(hologram.GameObject);

            GameMap.MoveBuilding(ref currentCell, ref targetCell);
        }

        void ShowBuildingInfo(GridCell cell)
        {
            _buildingInfoUI.gameObject.SetActive(true);
            _buildingInfoUI.gameObject.transform.position = Input.mousePosition;

            Building b = cell.Building;
            _buildingInfoUI.Building = b;

            _buildingInfoUI.Initialize();
        }

        void HideBuildingInfo() => _buildingInfoUI.gameObject.SetActive(false);

        void ClearPendingActionParams()
        {
            if (_interfacePendingAction.Parameters.TryGetValue(UIPendingActionParam.Building, out object obj))
                Destroy(((Building)obj).GameObject);

            _interfacePendingAction.Parameters.Clear();
            _interfacePendingAction = null;
        }
    }
}