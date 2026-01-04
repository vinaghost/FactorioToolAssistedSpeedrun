using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class ModifierTypeExtensions
    {
        private static readonly FrozenDictionary<string, ModifierType> _lookup = new Dictionary<string, ModifierType>()
        {
            { "all", ModifierType.All },
            { "walk_towards", ModifierType.WalkTowards},
            { "split", ModifierType.Split},
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenDictionary<ModifierType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static bool TryGetValue(string str, out ModifierType modifier)
        {
            return _lookup.TryGetValue(str, out modifier);
        }

        public static string ToLuaString(ModifierType modifier)
        {
            if (_reverseLookup.TryGetValue(modifier, out var str))
            {
                return $"{str} = true";
            }
            return "";
        }

        public static string ToString(ModifierType? modifier)
        {
            if (!modifier.HasValue)
                return "";
            if (_reverseLookup.TryGetValue(modifier.Value, out var str))
            {
                return str;
            }
            return "";
        }

        public static ModifierType? FromString(string str)
        {
            if (_lookup.TryGetValue(str, out var modifier))
            {
                return modifier;
            }
            return null;
        }
    }
}