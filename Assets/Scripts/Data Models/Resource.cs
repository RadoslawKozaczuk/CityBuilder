namespace Assets.Scripts.DataModels
{
    public struct Resource
    {
        public ResourceTypes ResourceType;
        public int Quantity;

        public Resource(ResourceTypes type, int quantity)
        {
            ResourceType = type;
            Quantity = quantity;
        }
    }
}
