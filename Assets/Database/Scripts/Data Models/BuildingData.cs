using System.Collections.Generic;
using UnityEngine;

namespace Assets.Database.DataModels
{
    // this represents building's data that comes from the data source
    // marked as read-only for optimization purposes
    public readonly struct BuildingData
    {
        public readonly string Name;
        public readonly Vector2Int Size;
        public readonly List<Resource> BuildCost;
        public readonly BuildingType Type;
        public readonly ResourceProductionData? ResourceProductionData;
        public readonly bool AbleToReallocate;
        public readonly Resource? ReallocationCost;

        public BuildingData(string name, Vector2Int size, List<Resource> cost, BuildingType type, 
            ResourceProductionData? rpd, bool ableToReallocate, Resource? reallocationCost)
        {
#if UNITY_EDITOR
            if (size.x <= 0 || size.y <= 0)
                throw new System.ArgumentException("building dimensions cannot be lower than 1", "size");
#endif

            Name = name;
            Size = size;
            BuildCost = cost;
            Type = type;
            ResourceProductionData = rpd;
            AbleToReallocate = ableToReallocate;
            ReallocationCost = reallocationCost;
        }
    }
}
