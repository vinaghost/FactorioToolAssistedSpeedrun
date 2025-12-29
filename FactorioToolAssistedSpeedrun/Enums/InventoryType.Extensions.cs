using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class InventoryTypeExtensions
    {
        private static FrozenDictionary<string, InventoryType> _lookup { get; } =
            new Dictionary<string, InventoryType>(StringComparer.OrdinalIgnoreCase)
            {
                { "Input", InventoryType.Input},
                { "Output", InventoryType.Output },
                { "Fuel", InventoryType.Fuel },
                { "Modules", InventoryType.Modules },
                { "Chest", InventoryType.Chest },
                { "Wreck", InventoryType.Wreck },
            }
            .ToFrozenDictionary();

        private static readonly FrozenDictionary<InventoryType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static InventoryType ToInventoryType(this string type)
        {
            if (_lookup.TryGetValue(type.Trim(), out var iInventoryType))
                return iInventoryType;
            return InventoryType.Chest;
        }

        public static string ToInventoryTypeString(this InventoryType inventoryType)
        {
            if (_reverseLookup.TryGetValue(inventoryType, out var str))
                return str;
            return _reverseLookup[InventoryType.Chest];
        }

        public static string GetInventoryDefines(this InventoryType type, string entity = "")
        {
            switch (type)
            {
                case InventoryType.Input:
                    if (entity == "lab")
                    {
                        return "defines.inventory.lab_input";
                    }
                    else
                    {
                        return "defines.inventory.crafter_input";
                    }

                case InventoryType.Output:
                    return "defines.inventory.crafter_output";

                case InventoryType.Fuel:
                    return "defines.inventory.fuel";

                case InventoryType.Modules:
                    return entity switch
                    {
                        "beacon" => "defines.inventory.beacon_modules",
                        "lab" => "defines.inventory.lab_modules",
                        "electric-mining-drill" or "pumpjack" => "defines.inventory.mining_drill_modules",
                        _ => "defines.inventory.crafter_modules",
                    };

                case InventoryType.Chest:
                case InventoryType.Wreck:
                    return "defines.inventory.chest";

                default:
                    throw new Exception(nameof(GetInventoryDefines));
            }
        }
    }
}