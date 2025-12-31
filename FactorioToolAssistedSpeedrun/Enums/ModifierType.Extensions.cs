using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class ModifierTypeExtensions
    {
        public static readonly FrozenDictionary<string, ModifierType> Lookup = new Dictionary<string, ModifierType>()
        {
            { "all", ModifierType.All },
            { "walk_towards", ModifierType.WalkTowards},
            { "split", ModifierType.Split},
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        public static readonly FrozenDictionary<ModifierType, string> ReverseLookup = Lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static string ToLuaString(ModifierType modifier)
        {
            if (ReverseLookup.TryGetValue(modifier, out var str))
            {
                return $"{str} = true";
            }
            return "";
        }

        public static string ToString(ModifierType? modifier)
        {
            if (!modifier.HasValue)
                return "";
            if (ReverseLookup.TryGetValue(modifier.Value, out var str))
            {
                return str;
            }
            return "";
        }

        public static ModifierType? FromString(string str)
        {
            if (Lookup.TryGetValue(str, out var modifier))
            {
                return modifier;
            }
            return null;
        }
    }
}