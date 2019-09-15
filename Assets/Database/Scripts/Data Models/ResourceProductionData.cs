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
        public readonly bool StartImmediately, Loop;

        public ResourceProductionData(Resource resource, float productionTime, bool startImmediately, bool loop)
        {
            Resource = resource;
            ProductionTime = productionTime;
            StartImmediately = startImmediately;
            Loop = loop;
        }
    }
}
