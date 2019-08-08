namespace Assets.Scripts.DataModels
{
    public struct Resource
    {
        public ResourceType ResourceType;
        public int Quantity;

        public Resource(ResourceType type, int quantity)
        {
            ResourceType = type;
            Quantity = quantity;
        }
    }
}
