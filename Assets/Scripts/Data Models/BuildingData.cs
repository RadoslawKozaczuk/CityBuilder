using Assets.Scripts.DataModels;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DataModels
{
    // this represents building data that comes from the data source
    public struct BuildingData
    {
        public string Name;
        public Vector2Int Size;
        public List<Resource> Cost;
        public BuildingType Type;
        public ResourceProductionData ResourceProductionData;
        public bool AbleToReallocate;
        public Resource? ReallocationCost;
    }
}
