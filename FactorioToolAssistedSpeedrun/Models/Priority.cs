using FactorioToolAssistedSpeedrun.Enums;

namespace FactorioToolAssistedSpeedrun.Models
{
    public class Priority
    {
        public required PriorityType In { get; set; }
        public required PriorityType Out { get; set; }

        public static string DefaultString => $"{PriorityTypeExtensions.ToLuaString(PriorityType.None)}, {PriorityTypeExtensions.ToLuaString(PriorityType.None)}";

        public string ToLuaString()
        {
            return $"{PriorityTypeExtensions.ToLuaString(In)}, {PriorityTypeExtensions.ToLuaString(Out)}";
        }

        public static string ToLuaString(Priority? priority)
        {
            return priority is null ? "" : priority.ToLuaString();
        }

        public static Priority? FromString(string str)
        {
            var parts = str.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                return null;
            }

            return new Priority
            {
                In = PriorityTypeExtensions.FromString(parts[0]),
                Out = PriorityTypeExtensions.FromString(parts[1])
            };
        }
    }
}