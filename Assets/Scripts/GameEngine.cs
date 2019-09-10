using Assets.Database;
using Assets.Scripts.UI;
using Assets.World;
using Assets.World.Commands;
using Assets.World.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance { get; private set; }

        public readonly Repository DB = new Repository();
        public GridCell? CellUnderCursorCached;

        [SerializeField] BuildingInfoUI _buildingInfoUI;
        [SerializeField] TextMeshProUGUI _commandListText;
        
        readonly List<AbstractCommand> _executedCommands = new List<AbstractCommand>();

        AbstractCommand _pendingCommand;
        (GameObject instance, BuildingType type) _hologram;
        MeshRenderer _hologramMeshRenderer;

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
            UpdateCommandListText();
        }

        void Start()
        {
            GameMap.BuildVehicle(VehicleType.Truck, new Vector2Int(1, 1));
        }

        void Update()
        {
            // some often used values are cached for performance reasons
            CellUnderCursorCached = EventSystem.current.IsPointerOverGameObject() 
                || !GameMap.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell)
                    ? null
                    : (GridCell?) cell;

            ProcessInput();
            UpdateHologramPosition();
        }
        #endregion

        public void StartBuildingConstruction(BuildingType type)
        {
            InstanciateHologram(type);
            _pendingCommand = new BuildBuildingCommand(type);
        }

        public void StartBuildingReallocation(Building b)
        {
            InstanciateHologram(b.Type);
            _pendingCommand = new MoveBuildingCommand(b);
        }

        void UpdateHologramPosition()
        {
            if (_pendingCommand != null)
            {
                // Check if this is valid context to execute this command
                if (!_pendingCommand.CheckExecutionContext())
                {
                    _hologram.instance.SetActive(false);
                    return;
                }

                if (!CellUnderCursorCached.HasValue)
                    return;

                _hologram.instance.SetActive(true);

                _hologramMeshRenderer.material = MaterialCollection.GetMaterial(_pendingCommand.CheckConditions()
                    ? CommonMaterials.HolographicGreen
                    : CommonMaterials.HolographicRed);

                _hologram.instance.transform.position = GameMap.GetMiddlePointWithOffset(
                    CellUnderCursorCached.Value.Coordinates, _hologram.type);
            }
        }

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // for testing vehicle selection
                //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //if(Physics.Raycast(ray, out RaycastHit hit))
                //{
                //    var vehicle = hit.collider.GetComponent<VehicleController>();
                //    vehicle.SelectMe();
                //    return;
                //}

                if (_pendingCommand != null)
                {
                    if (_pendingCommand.Call())
                    {
                        _executedCommands.Add(_pendingCommand.Clone());
                        _pendingCommand = null;
                        DestroyHologram();
                        UpdateCommandListText();
                    }
                }
                else if (CellUnderCursorCached.HasValue)
                {
                    if (CellUnderCursorCached.Value.IsOccupiedByVehicle)
                    {
                        (CellUnderCursorCached.Value.MapObject as Vehicle).ToggleSelection();
                    }
                    else if(CellUnderCursorCached.Value.IsOccupiedByBuilding)
                    {
                        ShowBuildingInfo(CellUnderCursorCached.Value);
                    }
                    else
                    {
                        HideBuildingInfo();
                        GameMap.HighlightCell(CellUnderCursorCached.Value.Coordinates);
                    }
                }
            }

            else if (Input.GetKeyDown(KeyCode.Space))
                ExplosionManager.SpawnRandomExplosion();

            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_pendingCommand != null)
                {
                    _pendingCommand = null;
                    DestroyHologram();
                }
                else
                    Application.Quit();
            }

            else
            {
                if(Application.isEditor)
                {
                    if (Input.GetKey(KeyCode.Z) && Input.GetKeyDown(KeyCode.X))
                    {
                        int lastCmdId = _executedCommands.Count - 1;
                        if (_executedCommands.Count > 0 && _executedCommands[lastCmdId].Undo())
                        {
                            _executedCommands.RemoveAt(lastCmdId);
                            UpdateCommandListText();
                        }
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
                    {
                        int lastCmdId = _executedCommands.Count - 1;
                        if (_executedCommands.Count > 0 && _executedCommands[lastCmdId].Undo())
                        {
                            _executedCommands.RemoveAt(lastCmdId);
                            UpdateCommandListText();
                        }
                    }
                }
            }
        }

        void UpdateCommandListText()
        {
            var sb = new StringBuilder();
            string shortcut = "<b>" + (Application.isEditor ? "Ctrl+Z" : "Z+X") + "</b>";
            sb.AppendLine($"Press {shortcut} to undo last command");
            sb.Append(Environment.NewLine);
            sb.AppendLine("Executed Commands:");

            for (int i = _executedCommands.Count - 1; i >= 0; i--)
                sb.AppendLine("- " + _executedCommands[i]);

            _commandListText.text = sb.ToString();
        }

        void InstanciateHologram(BuildingType type)
        {
            _hologram.instance = Instantiate(GameMap.MapFeaturePrefabCollection[type]);
            _hologramMeshRenderer = _hologram.instance.GetComponent<MeshRenderer>();
            _hologram.type = type;
        }

        void DestroyHologram()
        {
            Destroy(_hologram.instance);
            _hologram.instance = null;
            _hologram.type = BuildingType.None;
        }

        void ShowBuildingInfo(GridCell cell)
        {
            // TODO: temporary solution
            _buildingInfoUI.Building = (Building)cell.MapObject;
            _buildingInfoUI.gameObject.SetActive(true);
            _buildingInfoUI.gameObject.transform.position = Input.mousePosition;
        }

        void HideBuildingInfo() => _buildingInfoUI.gameObject.SetActive(false);
    }
}