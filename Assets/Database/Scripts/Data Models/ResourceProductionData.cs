namespace Assets.Database.DataModels
{
    // marked as read-only for optimization purposes
    public readonly struct ResourceProductionData
    {
        public readonly Resource Resource;

        /// <summary>
        /// In seconds.
        /// </summary>
        public readonly float ProductionTime;

        public readonly bool StartImidiately, Loop;

        public ResourceProductionData(Resource resource, float productionTime, bool startImidiately, bool loop)
        {
            Resource = resource;
            ProductionTime = productionTime;
            StartImidiately = startImidiately;
            Loop = loop;
        }
    }
}
