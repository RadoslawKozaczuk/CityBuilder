using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
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
        static ResourceManager _instance;

        // subscribers
        public static event EventHandler<ResourceChangedEventArgs> ResourceChangedEventHandler;

        [SerializeField] Sprite[] _resourceIcons;

        readonly int[] _playerResources = new int[Enum.GetNames(typeof(ResourceType)).Length];
        readonly DummyDatabase _db = new DummyDatabase();

        #region Unity life-cycle methods
        void Awake() => _instance = this;

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

        public static Sprite GetResourceIcon(ResourceType type) => _instance._resourceIcons[(int)type];
        public static Sprite GetResourceIcon(int id) => _instance._resourceIcons[id];

        /// <summary>
        /// Adds resources and broadcasts the ResourceChanged event.
        /// </summary>
        public static void AddResources(List<Resource> resources)
        {
            var newResources = new List<Resource>(resources.Count);

            foreach (Resource resource in resources)
            {
                AddResourceNoEventCall(resource);
                newResources.Add(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
            }

            ResourceChanged(newResources);
        }

        /// <summary>
        /// Adds resources and broadcasts the ResourceChanged event.
        /// </summary>
        public static void AddResources(BuildingType type) => AddResources(_instance._db[type].Cost);

        /// <summary>
        /// Adds resource and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        public static void AddResource(Resource resource)
        {
            // update value
            _instance._playerResources[(int)resource.ResourceType] += resource.Quantity;

            // inform subscribers
            ResourceChanged(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
        }

        /// <summary>
        /// Removes resources and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        public static void RemoveResources(List<Resource> resources)
        {
            var newResources = new List<Resource>(resources.Count);

            foreach (Resource resource in resources)
            {
                // update value
                RemoveResourceNoEventCall(resource);
                newResources.Add(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
            }

            // inform subscribers
            ResourceChanged(newResources);
        }

        /// <summary>
        /// Removes resource and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        public static void RemoveResource(Resource resource)
        {
            if (_instance._playerResources[(int)resource.ResourceType] < resource.Quantity)
            {
                Debug.LogError("ResourceManager was ask to remove more resources than it has. Resource quantity cannot be a negative number.");
                return;
            }

            // update value
            _instance._playerResources[(int)resource.ResourceType] -= resource.Quantity;

            // inform subscribers
            ResourceChanged(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
        }

        /// <summary>
        /// Removes resource and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        public static void RemoveResource(Resource? resource)
        {
            if (!resource.HasValue)
                throw new Exception("Resource should not be null in this context.");

            ResourceType resourceType = resource.Value.ResourceType;
            int quantity = resource.Value.Quantity;

            if (_instance._playerResources[(int)resourceType] < quantity)
            {
                Debug.LogError("ResourceManager was ask to remove more resources than it has. Resource quantity cannot be a negative number.");
                return;
            }

            // update value
            _instance._playerResources[(int)resourceType] -= quantity;

            // inform subscribers
            ResourceChanged(new Resource(resourceType, _instance._playerResources[(int)resourceType]));
        }

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public static bool IsEnoughResources(List<Resource> resources)
        {
            foreach (Resource resource in resources)
                if (!IsEnoughResource(resource))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public static bool IsEnoughResources(BuildingType type) => IsEnoughResources(_instance._db[type].Cost);

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public static bool IsEnoughResource(Resource resource) => _instance._playerResources[(int)resource.ResourceType] >= resource.Quantity;

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public static bool IsEnoughResource(Resource? resource) 
            => resource.HasValue ? _instance._playerResources[(int)resource.Value.ResourceType] >= resource.Value.Quantity : true;

        /// <summary>
        /// Adds resource without broadcasting the ResourceChanged event.
        /// </summary>
        static void AddResourceNoEventCall(Resource resource) => _instance._playerResources[(int)resource.ResourceType] += resource.Quantity;

        /// <summary>
        /// Removes resource without broadcasting the ResourceChanged event.
        /// </summary>
        static void RemoveResourceNoEventCall(Resource resource)
        {
            if (_instance._playerResources[(int)resource.ResourceType] < resource.Quantity)
            {
                Debug.LogError("ResourceManager was ask to remove more resources than it has. Resource quantity cannot be a negative number.");
                return;
            }

            _instance._playerResources[(int)resource.ResourceType] -= resource.Quantity;
        }

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        static void ResourceChanged(List<Resource> resources) => ResourceChangedEventHandler?.Invoke(
            _instance, 
            new ResourceChangedEventArgs { Resources = resources });

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        static void ResourceChanged(Resource resource) => ResourceChangedEventHandler?.Invoke(
            _instance, 
            new ResourceChangedEventArgs { Resources = new List<Resource>(1) { resource } });
    }
}
