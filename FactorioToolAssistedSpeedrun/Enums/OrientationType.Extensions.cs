using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class OrientationTypeExtensions
    {
        private static FrozenDictionary<string, OrientationType> _lookup { get; } =
            new Dictionary<string, OrientationType>(StringComparer.OrdinalIgnoreCase)
            {
                { "North", OrientationType.North },
                { "East", OrientationType.East },
                { "South", OrientationType.South },
                { "West", OrientationType.West },
            }
            .ToFrozenDictionary();

        private static readonly FrozenDictionary<OrientationType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static OrientationType ToOrientationType(this string type)
        {
            if (_lookup.TryGetValue(type.Trim(), out var orientationType))
                return orientationType;
            return OrientationType.North;
        }

        public static string ToOrientationTypeString(this OrientationType orientationType)
        {
            if (_reverseLookup.TryGetValue(orientationType, out var str))
                return str;
            return _reverseLookup[OrientationType.North];
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