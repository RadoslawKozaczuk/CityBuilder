using Assets.Database;
using Assets.Database.DataModels;
using Assets.GameLogic;
using System;
using UnityEngine;

namespace Assets.GraphicRepresentation.UI
{
    public class ResourcePanelUI : MonoBehaviour
    {
        [SerializeField] GameObject _resourceElementPrefab;

        readonly ResourceElementUI[] _resources = new ResourceElementUI[Enum.GetNames(typeof(ResourceType)).Length];

        void Start()
        {
            // subscribe to ResourceManager
            ResourceManager.ResourceChangedEventHandler += ResourceUpdate;

            // for now all resource types are displayed
            for (int i = 0; i < Enum.GetNames(typeof(ResourceType)).Length; i++)
            {
                var ui = Instantiate(_resourceElementPrefab, transform).GetComponent<ResourceElementUI>();
                ui.Image.sprite = ResourceManager.GetResourceIcon(i);
                ui.Amount.text = "0";
                _resources[i] = ui;
            }
        }

        void ResourceUpdate(object sender, ResourceChangedEventArgs eventArgs)
        {
            foreach (Resource r in eventArgs.Resources)
                _resources[(int)r.ResourceType].Amount.text = r.Quantity.ToString();
        }
    }
}