using Assets.Scripts;
using Assets.Scripts.DataModels;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class ResourcePanelUI : MonoBehaviour
{
    [SerializeField] GameObject ResourceElementPrefab;

    readonly ResourceElementUI[] _resources = new ResourceElementUI[Enum.GetNames(typeof(ResourceType)).Length];

    void Start()
    {
        // subscribe to ResourceManager
        ResourceManager.Instance.ResourceChangedEventHandler += ResourceUpdate;

        // for now all resource types are displayed
        for (int i = 0; i < Enum.GetNames(typeof(ResourceType)).Length; i++)
        {
            var ui = Instantiate(ResourceElementPrefab, transform).GetComponent<ResourceElementUI>();
            ui.Image.sprite = ResourceManager.Instance.ResourceIcons[i];
            ui.Amount.text = "0";
            _resources[i] = ui;
        }
    }

    void ResourceUpdate(object sender, ResourceChangedEventArgs eventArgs)
    {
        foreach (Resource r in eventArgs.Resources)
            _resources[(int)r.ResourceType].Amount.text = r.Quantity.ToString();

        Debug.Log("successfully observed the ResourceManager");
    }
}
