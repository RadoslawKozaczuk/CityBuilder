namespace Assets.Database.DataModels
{
    // marked as read-only for optimization purposes
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
