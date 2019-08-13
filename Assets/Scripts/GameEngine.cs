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

        readonly List<Building> _constructedBuildings = new List<Building>();
        InterfacePendingAction _interfacePendingAction;
        [SerializeField] GameObject[] _buildingPrefabs;

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();

        public Grid GameMap;

        void Awake() => Instance = this;

        void Update()
        {
            ProcessInput();
            UpdateTasks();
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
                    _interfacePendingAction.AddOrReplaceParameter(InterfacePendingActionParamType.CurrentCell, cell);
                    _interfacePendingAction.PendingAction.Invoke(_interfacePendingAction.Parameters);
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

            _interfacePendingAction = new InterfacePendingAction();
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingType, type);
            _interfacePendingAction.Parameters.Add(InterfacePendingActionParamType.BuildingData, data);
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

            //Debug.Log($"Check if there is enough space cell={cell.X}, {cell.Y}, size={data.SizeX}, {data.SizeY}");
            if (!GameMap.IsAreaFree(cell.X, cell.Y, data.SizeX, data.SizeY))
            {
                //Debug.Log($"Not enough room for the given building ({data.SizeX}, {data.SizeY})");
                return; // action will still be pending
            }

            // enough space
            ResourceManager.Instance.RemoveResources(data.Cost);

            GameObject prefab = _buildingPrefabs[(int)type];
            Vector3 targetPos = GameMap.GetMiddlePoint(cell.X, cell.Y, data.SizeX, data.SizeY);

            // prefab position may be adjusted and he have to take it into account
            targetPos.x += prefab.transform.position.x;
            targetPos.y = prefab.transform.position.y;
            targetPos.z += prefab.transform.position.z;

            Debug.Log("Building instantiated at x=" + targetPos.x + " y=" + targetPos.z);

            // create object representation
            var building = new Building(cell.X, cell.Y, type, data, Instantiate(prefab, targetPos, prefab.transform.rotation));

            _constructedBuildings.Add(building);

            GameMap.MarkAreaAsOccupied(cell.X, cell.Y, data.SizeX, data.SizeY, building);
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