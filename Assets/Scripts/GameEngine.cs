using Assets.Database;
using Assets.Scripts.UI;
using Assets.World;
using Assets.World.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public sealed class NullableGridCellStructRef
    {
        public GridCell? GridCell;
    }

    [DisallowMultipleComponent]
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance { get; private set; }

        public readonly AbstractDatabase Db = new DummyDatabase();
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

        void Update()
        {
            // some often used values are cached for performance reasons
            CellUnderCursorCached.GridCell = EventSystem.current.IsPointerOverGameObject()
                || !GameMap.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell)
                    ? null
                    : (GridCell?) cell;

            //if(CellUnderCursorCached.GridCell.HasValue)
            //    Debug.Log("coords: " + CellUnderCursorCached.GridCell.Value.Coordinates.x + ", " + CellUnderCursorCached.GridCell.Value.Coordinates.y);

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

                _hologram.transform.position = GameMap.GetMiddlePointWithOffset(CellUnderCursorCached.GridCell.Value.Coordinates, _pendingCommand.Type);
                //_hologram.transform.position = GameMap.GetMiddlePoint(CellUnderCursorCached.GridCell.Value.Coordinates, new Vector2Int(2, 2));

                var p = CellUnderCursorCached.GridCell.Value.Coordinates;
                Debug.Log("Hologram: " + _pendingCommand.Type.ToString() + " " + p.x + ", " + p.y);
            }
        }

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
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
            _hologram = Instantiate(GameMap.BuildingPrefabCollection[type]);
            _hologramMeshRenderer = _hologram.GetComponent<MeshRenderer>();
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