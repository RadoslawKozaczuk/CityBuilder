using Assets.Database;
using Assets.Database.DataModels;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.World
{
    public sealed class ResourceChangedEventArgs
    {
        public List<Resource> Resources;
    }

    /// <summary>
    /// ResourceManager is a singleton class responsible for storing resources.
    /// Subscribe to ResourceChangedEventHandler to be informed about any changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResourceManager : MonoBehaviour
    {
        static ResourceManager _instance;
        [SerializeField] Sprite[] _resourceIcons;

        /// <summary>
        /// Subscribe to this event to receive notifications each time resource number has changed.
        /// </summary>
        public static event EventHandler<ResourceChangedEventArgs> ResourceChangedEventHandler;

        readonly int[] _playerResources = new int[Enum.GetNames(typeof(ResourceType)).Length];
        readonly Repository _db = new Repository();

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
        /// Returns true if the player has enough resources to build this type of building, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnoughResources(BuildingType type)  => IsEnoughResources(_instance._db[type].BuildCost);

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnoughResources(Resource resource) => _instance._playerResources[(int)resource.ResourceType] >= resource.Quantity;

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// Null value is interpreted as zero resources and therefore gives true in return.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnoughResources(Resource? resource)
            => resource.HasValue ? _instance._playerResources[(int)resource.Value.ResourceType] >= resource.Value.Quantity : true;

        /// <summary>
        /// Returns true if the player has this amount of resources, false otherwise.
        /// </summary>
        public static bool IsEnoughResources(List<Resource> resources)
        {
            foreach (Resource resource in resources)
                if (!IsEnoughResources(resource))
                    return false;

            return true;
        }

        /// <summary>
        /// Adds resources and broadcasts ResourceChanged event.
        /// </summary>
        internal static void AddResources(List<Resource> resources)
        {
            var newResources = new List<Resource>(resources.Count);

            foreach (Resource resource in resources)
            {
                AddResourceNoEventCall(resource);
                newResources.Add(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
            }

            BroadcastResourceChanged(newResources);
        }

        /// <summary>
        /// Adds amount of resources necessary to build a building of the given type, and broadcasts ResourceChanged event.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddResources(BuildingType type) => AddResources(_instance._db[type].BuildCost);

        /// <summary>
        /// Adds resource and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddResources(Resource resource)
        {
            // update value
            _instance._playerResources[(int)resource.ResourceType] += resource.Quantity;

            // inform subscribers
            BroadcastResourceChanged(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
        }

        /// <summary>
        /// Adds resource and broadcasts ResourceChanged event to all subscribers.
        /// If resource is null nothing happens.
        /// </summary>
        internal static void AddResources(Resource? resource)
        {
            if (!resource.HasValue)
                return;

            AddResources(resource.Value);
        }

        /// <summary>
        /// Removes resources and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        internal static void RemoveResources(List<Resource> resources)
        {
            var newResources = new List<Resource>(resources.Count);

            foreach (Resource resource in resources)
            {
                // update value
                RemoveResourceNoEventCall(resource);
                newResources.Add(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
            }

            // inform subscribers
            BroadcastResourceChanged(newResources);
        }

        /// <summary>
        /// Removes resource amount equals to the construction cost of a building of the given type, 
        /// and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        internal static void RemoveResources(BuildingType type) => RemoveResources(_instance._db[type].BuildCost);

        /// <summary>
        /// Removes resource and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        internal static void RemoveResources(Resource resource)
        {
#if UNITY_EDITOR
            if (_instance._playerResources[(int)resource.ResourceType] < resource.Quantity)
                throw new System.ArgumentException("Resource quantity cannot be a negative number.", "resource");
#endif

            // update value
            _instance._playerResources[(int)resource.ResourceType] -= resource.Quantity;

            // inform subscribers
            BroadcastResourceChanged(new Resource(resource.ResourceType, _instance._playerResources[(int)resource.ResourceType]));
        }

        /// <summary>
        /// Removes resource and broadcasts ResourceChanged event to all subscribers.
        /// </summary>
        internal static void RemoveResources(Resource? resource)
        {
            if (!resource.HasValue)
                return;

            ResourceType resourceType = resource.Value.ResourceType;
            int quantity = resource.Value.Quantity;

#if UNITY_EDITOR
            if (_instance._playerResources[(int)resourceType] < quantity)
                throw new System.ArgumentException("Resource quantity cannot be a negative number.", "resource");
#endif

            // update value
            _instance._playerResources[(int)resourceType] -= quantity;

            // inform subscribers
            BroadcastResourceChanged(new Resource(resourceType, _instance._playerResources[(int)resourceType]));
        }

        /// <summary>
        /// Adds resource without broadcasting ResourceChanged event.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AddResourceNoEventCall(Resource resource) => _instance._playerResources[(int)resource.ResourceType] += resource.Quantity;

        /// <summary>
        /// Removes resource without broadcasting ResourceChanged event.
        /// </summary>
        static void RemoveResourceNoEventCall(Resource resource)
        {
#if UNITY_EDITOR
            if (_instance._playerResources[(int)resource.ResourceType] < resource.Quantity)
                throw new System.ArgumentException("Resource quantity cannot be a negative number.", "resource");
#endif

            _instance._playerResources[(int)resource.ResourceType] -= resource.Quantity;
        }

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        static void BroadcastResourceChanged(List<Resource> resources) => ResourceChangedEventHandler?.Invoke(
            _instance, 
            new ResourceChangedEventArgs { Resources = resources });

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        static void BroadcastResourceChanged(Resource resource) => ResourceChangedEventHandler?.Invoke(
            _instance, 
            new ResourceChangedEventArgs { Resources = new List<Resource>(1) { resource } });
    }
}
