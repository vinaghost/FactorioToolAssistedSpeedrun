using FactorioToolAssistedSpeedrun.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Models
{
    public class Priority
    {
        public required PriorityType In { get; set; }
        public required PriorityType Out { get; set; }

        public static string DefaultString => $"{PriorityType.None.ToLuaString()}, {PriorityType.None.ToLuaString()}";

        public override string ToString()
        {
            return $"{In.ToLuaString()}, {Out.ToLuaString()}";
        }

        public static string ToString(Priority? priority)
        {
            return priority is null ? "" : priority.ToString();
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