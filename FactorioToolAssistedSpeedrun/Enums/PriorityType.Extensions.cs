using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class PriorityTypeExtensions
    {
        private static FrozenDictionary<string, PriorityType> _lookup { get; } =
           new Dictionary<string, PriorityType>()
           {
                { "left", PriorityType.Left },
                { "none", PriorityType.None },
                { "right", PriorityType.Right },
           }
           .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenDictionary<PriorityType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static string ToString(PriorityType priority)
        {
            if (_reverseLookup.TryGetValue(priority, out var str))
            {
                return str;
            }
            return "";
        }

        public static PriorityType FromString(string str)
        {
            if (_lookup.TryGetValue(str, out var priority))
            {
                return priority;
            }
            return PriorityType.None;
        }

        public static string ToLuaString(PriorityType priority)
        {
            if (_reverseLookup.TryGetValue(priority, out var str))
            {
                return $"\"{str}\"";
            }
            return "";
        }
    }
}