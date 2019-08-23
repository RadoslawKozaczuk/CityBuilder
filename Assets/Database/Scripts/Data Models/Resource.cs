namespace Assets.Database.DataModels
{
    public readonly struct Resource
    {
        public readonly ResourceType ResourceType;
        public readonly int Quantity;

        public Resource(ResourceType type, int quantity)
        {
            ResourceType = type;
            Quantity = quantity;
        }
    }
}
