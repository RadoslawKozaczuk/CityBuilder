using Assets.Database;
using Assets.Scripts.Commands;
using Assets.Scripts.UI;
using Assets.World;
using Assets.World.Controllers;
using Assets.World.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    /// <summary>
    /// This wrapper is here because it is troublesome to pass reference to a nullable struct in C#.
    /// </summary>
    public sealed class NullableGridCellStructRef
    {
        public GridCell? GridCell;
    }

    [DisallowMultipleComponent]
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance { get; private set; }

        public readonly Repository Db = new Repository();
        public NullableGridCellStructRef CellUnderCursorCached = new NullableGridCellStructRef();

        [SerializeField] BuildingInfoUI _buildingInfoUI;
        [SerializeField] TextMeshProUGUI _commandListText;
        
        readonly List<AbstractCommand> _executedCommands = new List<AbstractCommand>();

        AbstractCommand _pendingCommand;
        GameObject _hologram;
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
            CellUnderCursorCached.GridCell = EventSystem.current.IsPointerOverGameObject()
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
            _pendingCommand = new ConstructBuildingCommand(type, CellUnderCursorCached);
        }

        public void StartBuildingReallocation(Building b)
        {
            InstanciateHologram(b.Type);
            _pendingCommand = new MoveBuildingCommand(b, CellUnderCursorCached);
        }

        void UpdateHologramPosition()
        {
            if (_pendingCommand != null)
            {
                // Check if this is valid context to execute this command
                if (!_pendingCommand.CheckExecutionContext())
                {
                    _hologram.SetActive(false);
                    return;
                }

                _hologram.SetActive(true);

                _hologramMeshRenderer.material = MaterialManager.GetMaterial(_pendingCommand.CheckConditions()
                    ? CommonMaterialType.HolographicGreen
                    : CommonMaterialType.HolographicRed);

                _hologram.transform.position = GameMap.GetMiddlePointWithOffset(
                    CellUnderCursorCached.GridCell.Value.Coordinates, _pendingCommand.Type);
            }
        }

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // for testing vehicle selection
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit))
                {
                    var vehicle = hit.collider.GetComponent<VehicleController>();
                    vehicle.SelectMe();
                    return;
                }


                if (_pendingCommand != null)
                {
                    if (_pendingCommand.Call())
                    {
                        _executedCommands.Add(_pendingCommand.Clone());
                        _pendingCommand = null;
                        Destroy(_hologram);
                        UpdateCommandListText();
                    }
                }
                else if (CellUnderCursorCached.GridCell.HasValue)
                {
                    if (CellUnderCursorCached.GridCell.Value.IsOccupied)
                        ShowBuildingInfo(CellUnderCursorCached.GridCell.Value);
                    else
                    {
                        HideBuildingInfo();
                        GameMap.HighlightCell(CellUnderCursorCached.GridCell.Value.Coordinates);
                    }
                }
            }

            else if (Input.GetKeyDown(KeyCode.Space))
                //GameMap.PathFinderTest(new Vector2Int(0, 0), new Vector2Int(5, 10));
                ExplosionManager.SpawnRandomExplosion();

            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_pendingCommand != null)
                {
                    _pendingCommand = null;
                    Destroy(_hologram);
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
            _hologram = Instantiate(GameMap.MapFeaturePrefabCollection[type]);
            _hologramMeshRenderer = _hologram.GetComponent<MeshRenderer>();
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