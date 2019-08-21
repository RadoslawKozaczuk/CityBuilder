using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    class BuildingPanelUI : MonoBehaviour
    {
        public Transform Resources;

        [SerializeField] GameObject _buildingElementPrefab;
        [SerializeField] GameObject _resourceElementPrefab;

        readonly DummyDatabase _db = new DummyDatabase();
        readonly List<BuildingButtonUI> _buttons = new List<BuildingButtonUI>();

        void Start()
        {
            ResourceManager.ResourceChangedEventHandler += ResourceUpdate; // subscribe to ResourceManager

            // initialize buildings
            foreach (BuildingData b in _db.AllBuildings)
            {
                BuildingButtonUI buttonUI = Instantiate(_buildingElementPrefab, Resources).GetComponent<BuildingButtonUI>();
                buttonUI.Title.text = b.Name;
                buttonUI.BuildingType = b.Type;
                buttonUI.GetComponent<Button>().onClick.AddListener(() => GameEngine.Instance.StartBuildingConstruction(buttonUI.BuildingType));
                buttonUI.BuildButton.interactable = ResourceManager.IsEnoughResources(b.Cost);
                _buttons.Add(buttonUI);

                foreach (Resource r in b.Cost)
                {
                    GameObject go = Instantiate(_resourceElementPrefab, buttonUI.Resources);
                    ResourceElementUI resUI = go.GetComponent<ResourceElementUI>();
                    Transform t = go.GetComponent<Transform>();

                    t.localScale = new Vector3(0.6f, 0.6f);

                    resUI.Amount.text = r.Quantity.ToString();
                    resUI.Image.sprite = ResourceManager.GetResourceIcon(r.ResourceType);
                }
            }
        }

        void ResourceUpdate(object sender, ResourceChangedEventArgs eventArgs)
        {
            foreach(BuildingButtonUI ui in _buttons)
                ui.BuildButton.interactable = ResourceManager.IsEnoughResources(ui.BuildingType);
        }
    }
}
