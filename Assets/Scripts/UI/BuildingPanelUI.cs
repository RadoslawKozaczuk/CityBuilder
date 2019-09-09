using Assets.Database;
using Assets.Database.DataModels;
using Assets.World;
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

        readonly Repository _db = new Repository();
        readonly List<BuildingButtonUI> _buttons = new List<BuildingButtonUI>();

        void Start()
        {
            ResourceManager.ResourceChangedEventHandler += ResourceUpdate; // subscribe to ResourceManager

            // initialize buildings
            foreach (BuildingData data in _db.AllBuildings)
            {
                BuildingButtonUI buttonUI = Instantiate(_buildingElementPrefab, Resources).GetComponent<BuildingButtonUI>();
                buttonUI.Title.text = data.Name;
                buttonUI.BuildingType = data.Type;
                buttonUI.GetComponent<Button>().onClick.AddListener(() => GameEngine.Instance.StartBuildingConstruction(buttonUI.BuildingType));
                buttonUI.BuildButton.interactable = ResourceManager.IsEnoughResources(data.BuildCost);
                _buttons.Add(buttonUI);

                foreach (Resource resource in data.BuildCost)
                {
                    GameObject go = Instantiate(_resourceElementPrefab, buttonUI.Resources);
                    ResourceElementUI ui = go.GetComponent<ResourceElementUI>();
                    ui.Amount.text = resource.Quantity.ToString();
                    ui.Image.sprite = ResourceManager.GetResourceIcon(resource.ResourceType);
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
