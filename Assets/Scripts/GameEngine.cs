using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance { get; private set; }

        [SerializeField] GameObject[] _buildingPrefabs;
        [SerializeField] Material _holographicMaterialGreen;
        [SerializeField] Material _holographicMaterialRed;

        readonly List<Building> _constructedBuildings = new List<Building>();
        InterfacePendingAction _interfacePendingAction;
        Material _tempMaterial;
        bool _pendingActionCanBeProcessed;

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();

        public Grid GameMap;

        void Awake() => Instance = this;

        void Update()
        {
            ProcessInput();
            UpdateTasks();

            // this violates good principles a bit
            if(_interfacePendingAction != null)
            {
                if(_interfacePendingAction.Parameters.TryGetValue(InterfacePendingActionParamType.BuildingInstance, out object obj))
                {
                    GameObject instance = (GameObject)obj;
                    instance.SetActive(false);

                    // Check if the mouse is over the UI
                    if (EventSystem.current.IsPointerOverGameObject())
                        return;

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
                        return;

                    if (hit.transform == null)
                        return;

                    if (!GameMap.GetCell(ray, out GridCell cell))
                        return;

                    _interfacePendingAction.Parameters.TryGetValue(InterfacePendingActionParamType.BuildingData, out obj);
                    BuildingData data = (BuildingData)obj;

                    if (GameMap.IsAreaOutOfBounds(cell.X, cell.Y, data.SizeX, data.SizeY))
                        return;

                    instance.SetActive(true);

                    _interfacePendingAction.Parameters.TryGetValue(InterfacePendingActionParamType.BuildingType, out obj);
                    BuildingType type = (BuildingType)obj;

                    Vector3 targetPos = ApplyPrefabOffset(
                        GameMap.GetMiddlePoint(cell.X, cell.Y, data.SizeX, data.SizeY), 
                        type);

                    bool areaFreeCheck = GameMap.IsAreaFree(cell.X, cell.Y, data.SizeX, data.SizeY);
                    if(areaFreeCheck)
                    {
                        instance.GetComponent<MeshRenderer>().material = _holographicMaterialGreen;
                        _pendingActionCanBeProcessed = true;
                    }
                    else
                    {
                        instance.GetComponent<MeshRenderer>().material = _holographicMaterialRed;
                        _pendingActionCanBeProcessed = false;
                    }

                    instance.transform.position = targetPos;
                }
            }
        }

        void LateUpdate()
        {
            _scheduledTasks.AddRange(_taskBuffer);
            _taskBuffer.Clear();
        }

        void UpdateTasks()
        {
            for (int i = 0; i < _scheduledTasks.Count; i++)
            {
                BuildingTask task = _scheduledTasks[i];
                task.TimeLeft -= Time.deltaTime;

                if (task.TimeLeft <= 0)
                {
                    task.ActionOnFinish();
                    _scheduledTasks.RemoveAt(i);
                    i--;
                }
            }
        }

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Check if the mouse was clicked over a UI element
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("Clicked on the UI");
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
                    return;

                if (hit.transform == null)
                    return;

                GameMap.GetCell(ray, out GridCell cell);
                Debug.Log("Hit the " + cell.X + " " + cell.Y);

                if (_interfacePendingAction != null)
                {
                    if (!_pendingActionCanBeProcessed)
                        return;

                    _interfacePendingAction.AddOrReplaceParameter(InterfacePendingActionParamType.CurrentCell, cell);
                    _interfacePendingAction.PendingAction(_interfacePendingAction.Parameters);
                    _interfacePendingAction = null;
                    return;
                }

                // regular mode
                if (cell.IsOccupied)
                    BuildingSelected(cell);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_interfacePendingAction != null)
                    _interfacePendingAction = null;
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

            GameObject prefab = _buildingPrefabs[(int)type];
            GameObject instance = Instantiate(prefab, Vector3.zero, prefab.transform.rotation);
            MeshRenderer mr = instance.GetComponent<MeshRenderer>();
            _tempMaterial = mr.material;
            mr.material = _holographicMaterialGreen;
            instance.SetActive(false);

            _interfacePendingAction = new InterfacePendingAction();
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingType, type);
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingData, data);
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingInstance, instance);
            _interfacePendingAction.PendingAction = BuildingConstructionFollowUpAction;
        }

        void BuildingConstructionFollowUpAction(Dictionary<InterfacePendingActionParamType, object> parameters)
        {
            parameters.TryGetValue(InterfacePendingActionParamType.CurrentCell, out object obj);
            GridCell cell = (GridCell)obj;
            parameters.TryGetValue(InterfacePendingActionParamType.BuildingType, out obj);
            BuildingType type = (BuildingType)obj;
            parameters.TryGetValue(InterfacePendingActionParamType.BuildingData, out obj);
            BuildingData data = (BuildingData)obj;
            parameters.TryGetValue(InterfacePendingActionParamType.BuildingInstance, out obj);
            GameObject instance = (GameObject)obj;

            //Debug.Log($"Check if there is enough space cell={cell.X}, {cell.Y}, size={data.SizeX}, {data.SizeY}");
            if (!GameMap.IsAreaFree(cell.X, cell.Y, data.SizeX, data.SizeY))
            {
                //Debug.Log($"Not enough room for the given building ({data.SizeX}, {data.SizeY})");
                return; // action will still be pending
            }

            // enough space
            ResourceManager.Instance.RemoveResources(data.Cost);

            instance.SetActive(true);
            instance.GetComponent<MeshRenderer>().material = _tempMaterial;

            // create object representation
            var building = new Building(cell.X, cell.Y, type, data, instance);

            _constructedBuildings.Add(building);

            GameMap.MarkAreaAsOccupied(cell.X, cell.Y, data.SizeX, data.SizeY, building);
        }

        // prefab position may be adjusted and he have to take it into account
        Vector3 ApplyPrefabOffset(Vector3 position, GameObject prefab)
        {
            position.x += prefab.transform.position.x;
            position.y = prefab.transform.position.y;
            position.z += prefab.transform.position.z;

            return position;
        }

        // prefab position may be adjusted and he have to take it into account
        Vector3 ApplyPrefabOffset(Vector3 position, BuildingType type)
        {
            GameObject prefab = _buildingPrefabs[(int)type];

            position.x += prefab.transform.position.x;
            position.y = prefab.transform.position.y;
            position.z += prefab.transform.position.z;

            return position;
        }

        void BuildingSelected(GridCell cell)
        {
            //_buildingInfoUI.gameObject.SetActive(true);
            //_buildingInfoUI.gameObject.transform.position = Input.mousePosition;

            //Building b = cell.Building;
            //_buildingInfoUI.Building = b;
        }
    }
}