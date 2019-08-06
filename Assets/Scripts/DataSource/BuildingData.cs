using Assets.Scripts.DataModels;
using System.Collections.Generic;

namespace Assets.Scripts.DataSource
{
    // this represents building data that comes from the data source
    public struct BuildingData
    {
        public string Name;
        public int SizeX, SizeY;
        public List<Resource> Cost;
    }
}
