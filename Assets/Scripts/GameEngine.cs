using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using Assets.Scripts.Interfaces;
using Assets.Scripts.UI;
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

        public GameObject[] BuildingPrefabs;
        public readonly AbstractDatabase Db = new DummyDatabase();
        public GridCell? CellUnderCursorCached;

        [SerializeField] BuildingInfoUI _buildingInfoUI;
        [SerializeField] TextMeshProUGUI _commandListText;

        readonly List<BuildingTask> _taskBuffer = new List<BuildingTask>();
        readonly List<BuildingTask> _scheduledTasks = new List<BuildingTask>();
        readonly List<AbstractCommand> _executedCommands = new List<AbstractCommand>();

        AbstractCommand _pendingCommand;
        GameObject _hologram;
        MeshRenderer _hologramMeshRenderer;
        BuildingType _type;

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
            UpdateCommandListText();
        }

        void Update()
        {
            // some often used values are cached for performance reasons
            CellUnderCursorCached = EventSystem.current.IsPointerOverGameObject()
                || !GameMap.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition), out GridCell cell)
                    ? null
                    : (GridCell?)cell;

            ProcessInput();
            UpdateTasks();

            UpdateLocalsBasedOnMousePos();
        }

        void LateUpdate()
        {
            _scheduledTasks.AddRange(_taskBuffer);
            _taskBuffer.Clear();
        }
        #endregion

        void UpdateLocalsBasedOnMousePos()
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
                else if (CellUnderCursorCached.HasValue)
                {
                    if (CellUnderCursorCached.Value.IsOccupied)
                        ShowBuildingInfo(CellUnderCursorCached.Value);
                    else
                    {
                        HideBuildingInfo();
                        GameMap.SelectCell(CellUnderCursorCached.Value.Coordinates);
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

        /// <summary>
        /// Add BuildingTask object to the task buffer.
        /// </summary>
        public void ScheduleTask(BuildingTask task) => _taskBuffer.Add(task);

        public void StartBuildingConstruction(BuildingType type)
        {
            InstanciateHologram(type);
            _type = type;
            _pendingCommand = new ConstructBuildingCommand(type);
        }

        public void StartBuildingReallocation(Building b)
        {
            InstanciateHologram(b.Type);
            _type = b.Type;
            _pendingCommand = new MoveBuildingCommand(b);
        }

        void InstanciateHologram(BuildingType type)
        {
            GameObject prefab = BuildingPrefabs[(int)type];
            _hologram = Instantiate(prefab);
            _hologramMeshRenderer = _hologram.GetComponent<MeshRenderer>();
            _hologramMeshRenderer.material = MaterialManager.GetMaterial(CommonMaterialType.HolographicGreen);
            _hologram.SetActive(false);
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