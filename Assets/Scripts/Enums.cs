namespace Assets.Scripts
{
    // I am not sure if all will be used in game but I decided to have a bit more just in case
    public enum ResourceType { Clothes, Coal, Gold, Hammer, Iron, Rocks, Wood };

    public enum BuildingType { Residence, WoodProduction, SteelProduction, Tree, Bench, Rock };

    /// <summary>
    /// Set of keys used by the pending action parameter dictionary.
    /// </summary>
    public enum UIPendingActionParam { PreviousCell, CurrentCell, Building }

    public enum CommonMaterialType { HolographicGreen, HolographicRed }
}
