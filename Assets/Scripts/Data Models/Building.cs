using Assets.Scripts.DataSource;
using UnityEngine;

namespace Assets.Scripts.DataModels
{
    // this represents building object in the game
    public class Building
    {
        const float ConstructionTime = 10f; // hardcoded

        Vector2Int _position;

        public Vector2Int Position
        {
            get => _position;
            set
            {
                Grid.Instance.MarkAreaAsFree(_position, SizeX, SizeY);

                _position = value;
                Grid.Instance.MarkAreaAsOccupied(this);
                GameObject.transform.position = GameEngine.Instance.GameMap.GetMiddlePoint(_position, Type)
                    .ApplyPrefabPositionOffset(Type);
            }
        }

        public string Name;
        public int SizeX, SizeY; // X and Y always point at the left bottom corner of the building
        public BuildingType Type { get; private set; }
        public GameObject GameObject;
        public bool Constructed = false;
        public bool ProductionStarted;
        public BuildingTask ScheduledTask;
        public bool AbleToReallocate;
        public Resource? ReallocationCost;

        readonly Resource _resource;
        readonly float _productionTime;
        readonly bool _imidiatelyStartProduction;
        readonly bool _loopProduction;

        MeshRenderer _meshRenderer;
        Material _defaultMaterial;

        #region Constructors
        public Building(ref BuildingData data)
        {
            Type = data.Type;

            GameObject instance = GameObject.Instantiate(GameEngine.Instance.BuildingPrefabs[(int)data.Type]);
            GameObject = instance;

            _resource = data.ResourceProductionData.Resource;
            _productionTime = data.ResourceProductionData.ProductionTime;
            _imidiatelyStartProduction = data.ResourceProductionData.StartImidiately;
            _loopProduction = data.ResourceProductionData.Loop;

            InnerConstructorLogic(ref data, instance);
        }

        public Building(int posX, int posY, ref BuildingData data, GameObject instance)
        {
            Type = data.Type;
            GameObject = instance;

            _resource = data.ResourceProductionData.Resource;
            _productionTime = data.ResourceProductionData.ProductionTime;
            _imidiatelyStartProduction = data.ResourceProductionData.StartImidiately;
            _loopProduction = data.ResourceProductionData.Loop;
            Position = new Vector2Int(posX, posY);

            InnerConstructorLogic(ref data, instance);
        }

        public Building(ref GridCell cell, ref BuildingData data, GameObject instance)
        {
            Type = data.Type;
            GameObject = instance;

            _resource = data.ResourceProductionData.Resource;
            _productionTime = data.ResourceProductionData.ProductionTime;
            _imidiatelyStartProduction = data.ResourceProductionData.StartImidiately;
            _loopProduction = data.ResourceProductionData.Loop;
            Position = new Vector2Int(cell.X, cell.Y);

            InnerConstructorLogic(ref data, instance);
        }

        public Building(BuildingType type)
        {
            Type = type;

            BuildingData data = GameEngine.Instance.Db[type];
            GameObject instance = GameObject.Instantiate(GameEngine.Instance.BuildingPrefabs[(int)type]);
            GameObject = instance;

            Position = new Vector2Int(
                GameEngine.Instance.CachedCurrentCell.Value.X,
                GameEngine.Instance.CachedCurrentCell.Value.Y);

            _resource = data.ResourceProductionData.Resource;
            _productionTime = data.ResourceProductionData.ProductionTime;
            _imidiatelyStartProduction = data.ResourceProductionData.StartImidiately;
            _loopProduction = data.ResourceProductionData.Loop;

            InnerConstructorLogic(ref data, instance);
        }

        public Building(BuildingType type, Vector2Int position)
        {
            Type = type;



            BuildingData data = GameEngine.Instance.Db[type];
            GameObject instance = GameObject.Instantiate(GameEngine.Instance.BuildingPrefabs[(int)type]);
            GameObject = instance;
            Position = position;

            _resource = data.ResourceProductionData.Resource;
            _productionTime = data.ResourceProductionData.ProductionTime;
            _imidiatelyStartProduction = data.ResourceProductionData.StartImidiately;
            _loopProduction = data.ResourceProductionData.Loop;

            InnerConstructorLogic(ref data, instance);
        }
        #endregion

        public void UseCommonMaterial(CommonMaterialType type) => _meshRenderer.material = MaterialManager.Instance.GetMaterial(type);

        public void UseDefaultMaterial() => _meshRenderer.material = _defaultMaterial;

        public void FinishConstruction()
        {
            Constructed = true;

            if (_imidiatelyStartProduction)
                StartProduction();
        }

        public void AddResource()
        {
            ResourceManager.AddResource(_resource);
            ProductionStarted = false;

            if (_loopProduction)
                StartProduction();
        }

        public void StartProduction()
        {
            // schedule production task
            BuildingTask task = new BuildingTask(_productionTime, AddResource);
            ScheduledTask = task;
            GameEngine.Instance.ScheduleTask(task);
            ProductionStarted = true;
        }

        void InnerConstructorLogic(ref BuildingData data, GameObject instance)
        {
            SizeX = data.SizeX;
            SizeY = data.SizeY;
            Name = data.Name;
            Type = data.Type;
            GameObject = instance;
            AbleToReallocate = data.AbleToReallocate;
            ReallocationCost = data.ReallocationCost;

            _meshRenderer = instance.GetComponent<MeshRenderer>();
            _defaultMaterial = _meshRenderer.material;

            // schedule construction task
            var task = new BuildingTask(ConstructionTime, FinishConstruction);
            ScheduledTask = task;
            GameEngine.Instance.ScheduleTask(task);
        }
    }
}
