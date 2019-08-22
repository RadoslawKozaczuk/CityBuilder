using Assets.Scripts.DataModels;

namespace Assets.Scripts.DataModels
{
    public struct ResourceProductionData
    {
        public Resource Resource;

        /// <summary>
        /// In seconds.
        /// </summary>
        public float ProductionTime;

        public bool StartImidiately, Loop;

        public ResourceProductionData(Resource resource, float productionTime, bool startImidiately, bool loop)
        {
            Resource = resource;
            ProductionTime = productionTime;
            StartImidiately = startImidiately;
            Loop = loop;
        }
    }
}
