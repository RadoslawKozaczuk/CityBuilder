using Assets.Database.DataModels;
using System.Collections.Generic;

namespace Assets.Database.Interfaces
{
    interface IRepository
    {
        BuildingData this[BuildingType type] { get; }
        VehicleData this[VehicleType type] { get; }

        List<BuildingData> AllBuildings { get; }
    }
}
