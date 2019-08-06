using Assets.Scripts;
using Assets.Scripts.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePanelUI : MonoBehaviour
{
    [SerializeField] GameObject ResourceElement;

    void Start()
    {
        // subscribe to ResourceManager
        ResourceManager.Instance.ResourceChangedEventHandler += ResourceUpdate;

        // for now all resource types are displayed
        int numberOfResources = Enum.GetNames(typeof(ResourceTypes)).Length;

        for (int i = 0; i < numberOfResources; i++)
        {
            var ui = Instantiate(ResourceElement, transform).GetComponent<ResourceElementUI>();
            ui.Image.sprite = ResourceManager.Instance.ResourceIcons[i];
            ui.Amount.text = "0";
        }
    }

    void ResourceUpdate(object sender, ResourceChangedEventArgs eventArgs)
    {
        Debug.Log("successfully observed the ResourceManager");
    }
}
