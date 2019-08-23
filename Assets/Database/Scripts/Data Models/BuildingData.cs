using System.Collections.Generic;
using UnityEngine;

namespace Assets.Database.DataModels
{
    // this represents building data that comes from the data source
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
