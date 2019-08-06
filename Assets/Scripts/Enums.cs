namespace Assets.Scripts
{
    /// <summary>
    /// Regular mode allows to inspect building and schedule tasks.
    /// Build mode allows to build and reallocate buildings.
    /// </summary>
    public enum GameModes { RegularMode, BuildMode }

    // I am not sure if all will be used in game but I decided to have a bit more just in case
    public enum ResourceTypes { Clothes, Coal, Gold, Hammer, Iron, Rocks, Wood };

    public enum BuildingType { Residence, WoodProduction, SteelProduction, Tree, Bench, Rock };

    /// <summary>
    /// Set of keys used by the pending action parameter dictionary.
    /// </summary>
    public enum InterfacePendingActionParamType { PreviousCell, CurrentCell, BuildingType, BuildingData }
}
