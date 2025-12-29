using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class ModifierTypeExtensions
    {
        public static readonly FrozenDictionary<string, ModifierType> Lookup = new Dictionary<string, ModifierType>(StringComparer.OrdinalIgnoreCase)
        {
            { "", ModifierType.None },
            { "all", ModifierType.All },
            { "walk_towards", ModifierType.WalkTowards},
            { "split", ModifierType.Split},
        }.ToFrozenDictionary();

        public static readonly FrozenDictionary<ModifierType, string> ReverseLookup = Lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static string ToLuaString(this ModifierType modifier)
        {
            if (modifier == ModifierType.None)
            {
                return "";
            }
            if (ReverseLookup.TryGetValue(modifier, out var str))
            {
                return $",  {str} = true,";
            }
            return "";
        }

        public static string ToModifierTypeString(this ModifierType modifier)
        {
            if (ReverseLookup.TryGetValue(modifier, out var str))
            {
                return str;
            }
            return "";
        }

        public static ModifierType ToModifierType(this string str)
        {
            if (Lookup.TryGetValue(str, out var modifier))
            {
                return modifier;
            }
            return ModifierType.None;
        }
    }
}