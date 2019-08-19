using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
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
        [SerializeField] Material _holographicMaterialGreen;
        [SerializeField] Material _holographicMaterialRed;

        readonly List<Building> _constructedBuildings = new List<Building>();
        InterfacePendingAction _interfacePendingAction;
        Material _tempMaterial;
        bool _pendingActionCanBeProcessed;

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();

        #region Unity life-cycle methods
        void Awake() => Instance = this;

        void Update()
        {
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
            // this violates good principles a bit
            if (_interfacePendingAction != null)
            {
                if (_interfacePendingAction.Parameters.TryGetValue(InterfacePendingActionParamType.BuildingInstance, out object obj))
                {
                    GameObject instance = (GameObject)obj;

                    // Check if the mouse is over the UI
                    if (EventSystem.current.IsPointerOverGameObject()
                        || !GameMap.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell))
                    {
                        instance.SetActive(false);
                        return;
                    }

                    _interfacePendingAction.Parameters.TryGetValue(InterfacePendingActionParamType.BuildingData, out obj);
                    BuildingData data = (BuildingData)obj;

                    if (GameMap.IsAreaOutOfBounds(cell.X, cell.Y, data.SizeX, data.SizeY))
                    {
                        instance.SetActive(false);
                        return;
                    }

                    instance.SetActive(true);

                    Vector3 targetPos = GameMap.GetMiddlePoint(cell.X, cell.Y, data.SizeX, data.SizeY)
                        .ApplyPrefabPositionOffset(data.Type);

                    bool enoughSpace = GameMap.IsAreaFree(cell.X, cell.Y, data.SizeX, data.SizeY);
                    _pendingActionCanBeProcessed = enoughSpace;
                    instance.GetComponent<MeshRenderer>().material = enoughSpace 
                        ? _holographicMaterialGreen 
                        : _holographicMaterialRed;

                    instance.transform.position = targetPos;
                }
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
                // Check if the mouse was clicked over a UI element
                if (EventSystem.current.IsPointerOverGameObject()
                    || !GameMap.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell))
                    return;

                if (_interfacePendingAction != null && _pendingActionCanBeProcessed)
                {
                    _interfacePendingAction.AddOrReplaceParameter(InterfacePendingActionParamType.CurrentCell, cell);
                    _interfacePendingAction.PendingAction(_interfacePendingAction.Parameters);
                    _interfacePendingAction = null;
                    return;
                }

                // regular mode
                if (cell.IsOccupied)
                    ShowBuildingInfo(cell);
                else
                    HideBuildingInfo();
            }

            if (Input.GetKeyDown(KeyCode.Space))
                ExplosionManager.Instance.SpawnRandomExplosion();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_interfacePendingAction != null)
                    ClearInterfacePendingActionVariables();
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
            BuildingData data = BuildingDataSource.Buildings[(int)type];
            if (!ResourceManager.Instance.IsEnoughResources(data.Cost))
            {
                Debug.Log("Not enough resources!!!");
                return;
            }

            Debug.Log($"Started building action , building type {type}");

            GameObject instance = InstanciateHologram(type);

            _interfacePendingAction = new InterfacePendingAction();
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingData, data);
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingInstance, instance);
            _interfacePendingAction.PendingAction = BuildingConstructionFollowUpAction;
        }

        GameObject InstanciateHologram(BuildingType type)
        {
            GameObject prefab = BuildingPrefabs[(int)type];
            GameObject instance = Instantiate(prefab);
            MeshRenderer mr = instance.GetComponent<MeshRenderer>();
            _tempMaterial = mr.material;
            mr.material = _holographicMaterialGreen;
            instance.SetActive(false);

            return instance;
        }

        void BuildingConstructionFollowUpAction(Dictionary<InterfacePendingActionParamType, object> parameters)
        {
            parameters.TryGetValue(InterfacePendingActionParamType.CurrentCell, out object obj);
            GridCell cell = (GridCell)obj;
            parameters.TryGetValue(InterfacePendingActionParamType.BuildingData, out obj);
            BuildingData data = (BuildingData)obj;
            parameters.TryGetValue(InterfacePendingActionParamType.BuildingInstance, out obj);
            GameObject instance = (GameObject)obj;

            if (!GameMap.IsAreaFree(cell.X, cell.Y, data.SizeX, data.SizeY))
                return; // action will still be pending

            // enough space
            ResourceManager.Instance.RemoveResources(data.Cost);

            instance.SetActive(true);
            instance.GetComponent<MeshRenderer>().material = _tempMaterial;

            // create object representation
            var building = new Building(cell.X, cell.Y, data, instance);

            _constructedBuildings.Add(building);

            GameMap.MarkAreaAsOccupied(cell.X, cell.Y, data.SizeX, data.SizeY, building);
        }

        public void BuildingReallocationAction(Building building)
        {
            Debug.Log("Started building reallocation action");
            BuildingData data = BuildingDataSource.Buildings[(int)building.BuildingType];

            // we need corner cell not the one that we clicked on
            GridCell currentCell = GameMap.GetCell(building.PositionX, building.PositionY);

            GameObject instance = InstanciateHologram(data.Type);

            _interfacePendingAction = new InterfacePendingAction();
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.PreviousCell, currentCell);
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingData, data);
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingInstance, instance);
            _interfacePendingAction.PendingAction = BuildingReallocationFollowUpAction;
        }

        void BuildingReallocationFollowUpAction(Dictionary<InterfacePendingActionParamType, object> parameters)
        {
            parameters.TryGetValue(InterfacePendingActionParamType.PreviousCell, out object obj);
            GridCell currentCell = (GridCell)obj;
            parameters.TryGetValue(InterfacePendingActionParamType.CurrentCell, out obj);
            GridCell targetCell = (GridCell)obj;
            parameters.TryGetValue(InterfacePendingActionParamType.BuildingData, out obj);
            BuildingData data = (BuildingData)obj;

            Building b = currentCell.Building;
            if (!GameMap.IsAreaFree(targetCell.X, targetCell.Y, data.SizeX, data.SizeY, b))
            {
                Debug.Log($"Not enough room for the given building ({data.SizeX}, {data.SizeY})");
                return; // action will still be pending
            }

            ResourceManager.Instance.RemoveResource(data.ReallocationCost);

            parameters.TryGetValue(InterfacePendingActionParamType.BuildingInstance, out obj);
            GameObject instance = (GameObject)obj;
            Destroy(instance);

            b.PositionX = targetCell.X;
            b.PositionY = targetCell.Y;
            b.GameObjectInstance.transform.position = GameMap.GetMiddlePoint(targetCell.X, targetCell.Y, data.SizeX, data.SizeY)
                .ApplyPrefabPositionOffset(data.Type);

            GameMap.MoveBuilding(ref currentCell, ref targetCell, ref data, b);
        }

        void ClearInterfacePendingActionVariables()
        {
            if (_interfacePendingAction.Parameters.TryGetValue(InterfacePendingActionParamType.BuildingInstance, out object obj))
                Destroy((GameObject)obj);

            _interfacePendingAction = null;
            _tempMaterial = null;
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
    }
}