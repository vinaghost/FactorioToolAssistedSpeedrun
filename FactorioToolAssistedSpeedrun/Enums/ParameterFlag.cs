namespace FactorioToolAssistedSpeedrun.Enums
{
    [Flags]
    public enum ParameterFlag
    {
        None = 0,
        Point = 1 << 0,
        Amount = 1 << 1,
        Item = 1 << 2,
        Orientation = 1 << 3,
        Inventory = 1 << 4,
        Priority = 1 << 5,
        Modifier = 1 << 6,
    }
}