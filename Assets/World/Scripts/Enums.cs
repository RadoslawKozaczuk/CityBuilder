namespace Assets.World
{
    /// <summary>
    /// Indicates if this command should add or remove resources.
    /// For example when player wants to purchase a building use Remove parameter, in case of sale use Remove.
    /// </summary>
    public enum ResourceOperationType { Add, Remove }
}
