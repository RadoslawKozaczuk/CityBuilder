using Assets.Scripts.DataModels;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    class ResourceChangedEventArgs
    {
        public List<Resource> Resources;
    }

    /// <summary>
    /// ResourceManager is a singleton class responsible for storing resources.
    /// Subscribe to ResourceChangedEventHandler to be informed about any changes.
    /// </summary>
    class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        // subscribers
        public event EventHandler<ResourceChangedEventArgs> ResourceChangedEventHandler;

        public Sprite[] ResourceIcons;

        readonly int[] _playerResources = new int[Enum.GetNames(typeof(ResourceType)).Length];

        #region Unity life-cycle methods
        void Awake() => Instance = this;

        void Start()
        {
            var startResources = new List<Resource>(3)
            {
                new Resource(ResourceType.Gold, 400),
                new Resource(ResourceType.Wood, 200),
                new Resource(ResourceType.Iron, 200)
            };

            AddResources(startResources);
        }
        #endregion

        /// <summary>
        /// Adds resources and broadcasts the ResourceChanged event.
        /// </summary>
        public void AddResources(List<Resource> resources)
        {
            var newResources = new List<Resource>(resources.Count);

            foreach (Resource resource in resources)
            {
                AddResourceNoEventCall(resource);
                newResources.Add(new Resource(resource.ResourceType, _playerResources[(int)resource.ResourceType]));
            }

            ResourceChanged(newResources);
        }

        /// <summary>
        /// Adds resource and broadcasts the resource changed event.
        /// </summary>
        public void AddResource(Resource resource)
        {
            // update value
            _playerResources[(int)resource.ResourceType] += resource.Quantity;

            // inform subscribers
            ResourceChanged(new Resource(resource.ResourceType, _playerResources[(int)resource.ResourceType]));
        }

        /// <summary>
        /// Removes resources and broadcasts the ResourceChanged event.
        /// </summary>
        public void RemoveResources(List<Resource> resources)
        {
            var newResources = new List<Resource>(resources.Count);

            foreach (Resource resource in resources)
            {
                // update value
                RemoveResourceNoEventCall(resource);
                newResources.Add(new Resource(resource.ResourceType, _playerResources[(int)resource.ResourceType]));
            }

            // inform subscribers
            ResourceChanged(newResources);
        }

        /// <summary>
        /// Removes resource and broadcasts the ResourceChanged event.
        /// </summary>
        public void RemoveResource(Resource resource)
        {
            if (_playerResources[(int)resource.ResourceType] < resource.Quantity)
            {
                Debug.LogError("ResourceManager was ask to remove more resources than it has. Resource quantity cannot be a negative number.");
                return;
            }

            // update value
            _playerResources[(int)resource.ResourceType] -= resource.Quantity;

            // inform subscribers
            ResourceChanged(new Resource(resource.ResourceType, _playerResources[(int)resource.ResourceType]));
        }

        /// <summary>
        /// Removes resource and broadcasts the ResourceChanged event.
        /// </summary>
        public void RemoveResource(Resource? resource)
        {
            if (!resource.HasValue)
                throw new Exception("Resource should not be null in this context.");

            ResourceType resourceType = resource.Value.ResourceType;
            int quantity = resource.Value.Quantity;

            if (_playerResources[(int)resourceType] < quantity)
            {
                Debug.LogError("ResourceManager was ask to remove more resources than it has. Resource quantity cannot be a negative number.");
                return;
            }

            // update value
            _playerResources[(int)resourceType] -= quantity;

            // inform subscribers
            ResourceChanged(new Resource(resourceType, _playerResources[(int)resourceType]));
        }

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public bool IsEnoughResources(List<Resource> resources)
        {
            foreach (Resource resource in resources)
                if (!IsEnoughResource(resource))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public bool IsEnoughResource(Resource resource) => _playerResources[(int)resource.ResourceType] >= resource.Quantity;

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public bool IsEnoughResource(Resource? resource) => _playerResources[(int)resource.Value.ResourceType] >= resource.Value.Quantity;

        /// <summary>
        /// Adds resource without broadcasting the ResourceChanged event.
        /// Only for internal usage.
        /// </summary>
        void AddResourceNoEventCall(Resource resource) => _playerResources[(int)resource.ResourceType] += resource.Quantity;

        /// <summary>
        /// Removes resource without broadcasting the ResourceChanged event.
        /// Only for internal usage.
        /// </summary>
        void RemoveResourceNoEventCall(Resource resource)
        {
            if (_playerResources[(int)resource.ResourceType] < resource.Quantity)
            {
                Debug.LogError("ResourceManager was ask to remove more resources than it has. Resource quantity cannot be a negative number.");
                return;
            }

            _playerResources[(int)resource.ResourceType] -= resource.Quantity;
        }

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        void ResourceChanged(List<Resource> resources) => ResourceChangedEventHandler?.Invoke(
            this, 
            new ResourceChangedEventArgs { Resources = resources });

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        void ResourceChanged(Resource resource) => ResourceChangedEventHandler?.Invoke(
            this, 
            new ResourceChangedEventArgs { Resources = new List<Resource>(1) { resource } });
    }
}
