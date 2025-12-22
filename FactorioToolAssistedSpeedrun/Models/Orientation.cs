namespace FactorioToolAssistedSpeedrun.Models
{
    public enum OrientationType
    {
        North,
        East,
        South,
        West,
    }

    public static class OrientationTypeExtensions
    {
        public static string GetOrientationDefines(this OrientationType orientation)
        {
            return orientation switch
            {
                OrientationType.North => "defines.direction.north",
                OrientationType.East => "defines.direction.east",
                OrientationType.South => "defines.direction.south",
                OrientationType.West => "defines.direction.west",
                _ => throw new Exception(nameof(GetOrientationDefines)),
            };
        }
    }
}