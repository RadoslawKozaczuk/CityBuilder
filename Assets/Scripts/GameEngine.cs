using Assets.Database;
using Assets.Scripts.UI;
using Assets.World;
using Assets.World.Commands;
using Assets.World.DataModels;
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
        
        AbstractCommand _pendingCommand;
        (GameObject instance, BuildingType type) _hologram;
        MeshRenderer _hologramMeshRenderer;

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
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
                if (_pendingCommand != null)
                {
                    if (_pendingCommand.Call())
                    {
                        _pendingCommand = null;
                        DestroyHologram();
                    }
                }
                else if (CellUnderCursorCached.HasValue) 
                {
                    // send click info
                    bool hitAnything = GameMap.ClickMe(out GridCell cell, out Building building);
                    if(hitAnything)
                    {
                        if (building == null)
                            HideBuildingInfo();
                        else
                            ShowBuildingInfo(CellUnderCursorCached.Value);
                    }
                    else
                    {
                        HideBuildingInfo();
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
                        GameMap.UndoLastCommand();
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
                        GameMap.UndoLastCommand();
                }
            }
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
            _buildingInfoUI.Building = (Building)cell.MapObject;
            _buildingInfoUI.gameObject.SetActive(true);
            _buildingInfoUI.gameObject.transform.position = Input.mousePosition;
        }

        void HideBuildingInfo() => _buildingInfoUI.gameObject.SetActive(false);
    }
}