using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class InventoryTypeExtensions
    {
        private static FrozenDictionary<string, InventoryType> _lookup { get; } =
            new Dictionary<string, InventoryType>()
            {
                { "input", InventoryType.Input},
                { "output", InventoryType.Output },
                { "fuel", InventoryType.Fuel },
                { "modules", InventoryType.Modules },
                { "chest", InventoryType.Chest },
                { "wreck", InventoryType.Wreck },
            }
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenDictionary<InventoryType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static InventoryType? FromString(string type)
        {
            if (_lookup.TryGetValue(type.Trim(), out var inventory))
                return inventory;
            return null;
        }

        public static string ToString(InventoryType? inventory)
        {
            if (!inventory.HasValue)
                return "";
            if (_reverseLookup.TryGetValue(inventory.Value, out var str))
                return str;
            return "";
        }

        public static string GetInventoryDefines(this InventoryType inventory, string entity = "")
        {
            switch (inventory)
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