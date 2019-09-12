using UnityEngine;

namespace Assets.Database.DataModels
{
    // marked as read-only for optimization purposes
    public readonly struct VehicleData
    {
        public readonly Vector2Int Size;
        public readonly VehicleType Type;

        /// <summary>
        /// Measured in Unity units per second.
        /// For example if CELL_SIZE = 10 then speed = 10 gives us 1 cell per second.
        /// </summary>
        public readonly float Speed;

        public VehicleData(Vector2Int size, VehicleType type, float speed)
        {
#if UNITY_EDITOR
            if (size.x <= 0 || size.y <= 0)
                throw new System.ArgumentException("Vehicle dimensions cannot be lower than 1.", "size");
#endif

            Size = size;
            Type = type;
            Speed = speed;
        }
    }
}
