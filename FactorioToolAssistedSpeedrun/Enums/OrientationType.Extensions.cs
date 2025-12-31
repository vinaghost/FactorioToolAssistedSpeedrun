using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class OrientationTypeExtensions
    {
        private static FrozenDictionary<string, OrientationType> _lookup { get; } =
            new Dictionary<string, OrientationType>()
            {
                { "north", OrientationType.North },
                { "east", OrientationType.East },
                { "south", OrientationType.South },
                { "west", OrientationType.West },
            }
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenDictionary<OrientationType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static OrientationType? FromString(string str)
        {
            if (_lookup.TryGetValue(str, out var orientation))
                return orientation;
            return null;
        }

        public static string ToString(OrientationType? orientation)
        {
            if (!orientation.HasValue)
                return "";
            if (_reverseLookup.TryGetValue(orientation.Value, out var str))
                return str;
            return "";
        }

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