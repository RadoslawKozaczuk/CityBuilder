using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePanelUI : MonoBehaviour
{
    private void Awake()
    {
        // for now all resource types are displayed
        int numberOfResources = Enum.GetNames(typeof(ResourceTypes)).Length;

        // subscribe to ResourceManager
        ResourceManager.Instance.ResourceChangedEventHandler += ResourceUpdate;
    }

    void Start()
    {

    }

    void ResourceUpdate(object sender, ResourceChangedEventArgs eventArgs)
    {
        // yo yoy yo update the visuals

        Debug.Log("succesfully observed the ResourceManager");
    }
}
