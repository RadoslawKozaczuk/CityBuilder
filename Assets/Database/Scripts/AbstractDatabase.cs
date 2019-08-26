using Assets.Database.DataModels;
using System;
using System.Collections.Generic;

namespace Assets.Database
{
    public abstract class AbstractDatabase
    {
        protected BuildingData[] _buildingsTable;
        protected VehicleData[] _vehiclesTable;

        internal abstract int CreateBuilding(BuildingData data);
        internal abstract BuildingData ReadBuilding(int id);
        internal abstract IEnumerable<BuildingData> ReadBuildings(Func<BuildingData, bool> condition);
        internal abstract void UpdateBuilding(int id, BuildingData data);
        internal abstract void DeleteBuilding(int id);

        internal abstract int CreateVehicle(VehicleData data);
        internal abstract VehicleData ReadVehicle(int id);
        internal abstract IEnumerable<VehicleData> ReadVehicles(Func<VehicleData, bool> condition);
        internal abstract void UpdateVehicle(int id, VehicleData data);
        internal abstract void DeleteVehicle(int id);
    }
}
