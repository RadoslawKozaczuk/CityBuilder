using UnityEngine;

namespace Assets.Database.DataModels
{
    public struct VehicleData
    {
        public readonly Vector2Int Size;
        public readonly VehicleType Type;

        public VehicleData(Vector2Int size, VehicleType type)
        {
            Size = size;
            Type = type;
        }
    }
}
