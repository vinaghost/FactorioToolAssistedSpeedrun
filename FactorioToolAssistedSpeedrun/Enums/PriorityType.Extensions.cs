using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class PriorityTypeExtensions
    {
        public static string ToLuaString(this PriorityType priorityType)
        {
            return priorityType switch
            {
                PriorityType.Left => "\"left\"",
                PriorityType.None => "\"none\"",
                PriorityType.Right => "\"right\"",
                _ => "\"none\"",
            };
        }

        public static PriorityType FromString(string str)
        {
            return str.ToLower() switch
            {
                "left" => PriorityType.Left,
                "none" => PriorityType.None,
                "right" => PriorityType.Right,
                _ => PriorityType.None,
            };
        }
    }
}