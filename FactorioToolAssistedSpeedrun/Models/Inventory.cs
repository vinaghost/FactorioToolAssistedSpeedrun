namespace FactorioToolAssistedSpeedrun.Models
{
    public enum InventoryType
    {
        Input,
        Output,
        Fuel,
        Modules,
        Chest,
        Wreck,
    };

    public static class InventoryTypeExtensions
    {
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