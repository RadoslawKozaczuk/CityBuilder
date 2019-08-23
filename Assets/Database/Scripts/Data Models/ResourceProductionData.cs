namespace Assets.Database.DataModels
{
    // this struct is marked readonly for optimization purposes
    // it is not like it is important but since we know it should not be changed at all why not to do it
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
