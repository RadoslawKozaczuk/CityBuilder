﻿using Assets.Database.DataModels;
using System;
using System.Collections.Generic;

namespace Assets.Database.Interfaces
{
    internal interface IDatabase
    {
        /// <summary>
        /// Returns ID of the newly created building.
        /// </summary>
        int CreateBuilding(BuildingData data);
        BuildingData ReadBuilding(int id);
        IEnumerable<BuildingData> ReadBuildings(Func<BuildingData, bool> condition);
        void UpdateBuilding(int id, BuildingData data);
        void DeleteBuilding(int id);

        /// <summary>
        /// Returns ID of the newly created vehicle.
        /// </summary>
        int CreateVehicle(VehicleData data);
        VehicleData ReadVehicle(int id);
        IEnumerable<VehicleData> ReadVehicles(Func<VehicleData, bool> condition);
        void UpdateVehicle(int id, VehicleData data);
        void DeleteVehicle(int id);
    }
}
