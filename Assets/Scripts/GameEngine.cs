using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using Assets.Scripts.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GameEngine : MonoBehaviour
    {
        readonly List<Building> _constructedBuildings = new List<Building>();
        InterfacePendingAction _interfacePendingAction;
        [SerializeField] Transform _buildingPanel;
        [SerializeField] GameObject _buildingUIElement;
        [SerializeField] GameObject _resourceUIElement;
        [SerializeField] GameObject[] _buildingPrefabs;

        public Grid GameMap;

        void Update()
        {
            ProcessInput();
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
            }
        }

        void InsertBuilding(BuildingType type)
        {
            BuildingData data = BuildingDataSource.Buildings[(int)type];

            BuildingButtonUI buildingUI = Instantiate(_buildingUIElement, _buildingPanel).GetComponent<BuildingButtonUI>();
            buildingUI.Title.text = data.Name;
            buildingUI.GetComponent<Button>().onClick.AddListener(() => BuildingConstructionAction(type));

            foreach (Resource cost in data.Cost)
            {
                ResourceElementUI resourceUI = Instantiate(_resourceUIElement, buildingUI.HorizontalGroup).GetComponent<ResourceElementUI>();
                resourceUI.Image.sprite = ResourceManager.Instance.ResourceIcons[(int)cost.ResourceType];
                resourceUI.Amount.text = cost.Quantity.ToString();
            }
        }

        void BuildingConstructionAction(BuildingType type)
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

            Debug.Log($"Check if there is enough space cell={cell.X}, {cell.Y}, size={data.SizeX}, {data.SizeY}");
            if (!GameMap.IsAreaFree(cell.X, cell.Y, data.SizeX, data.SizeY))
            {
                Debug.Log($"Not enough room for the given building ({data.SizeX}, {data.SizeY})");
                return; // action will still be pending
            }

            // enough space
            ResourceManager.Instance.RemoveResources(data.Cost);

            GameObject prefab = _buildingPrefabs[(int)type];
            Vector3 targetPos = GameMap.GetCellPosition(cell);
            targetPos.y = prefab.transform.position.y; // each prefab has different y
            targetPos.z += 10; // we need upper left corner

            // create object representation
            var building = new Building(cell.X, cell.Y, type, data, Instantiate(prefab, targetPos, prefab.transform.rotation));

            _constructedBuildings.Add(building);

            GameMap.MarkAreaAsOccupied(cell.X, cell.Y, data.SizeX, data.SizeY, building);
        }
    }
}