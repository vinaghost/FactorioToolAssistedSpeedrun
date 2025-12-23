using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models.Prototypes
{
    public class PrototypeData
    {
        [JsonPropertyName("technology")]
        public required Dictionary<string, TechnologyPrototype> Technologies { get; set; }

        [JsonPropertyName("item")]
        public required Dictionary<string, ItemPrototype> Items { get; set; }

        [JsonPropertyName("recipe")]
        public required Dictionary<string, RecipePrototype> Recipes { get; set; }

        [JsonPropertyName("ammo")]
        public required Dictionary<string, ItemPrototype> AmmoItems { get; set; }

        [JsonPropertyName("capsule")]
        public required Dictionary<string, ItemPrototype> Capsules { get; set; }

        [JsonPropertyName("gun")]
        public required Dictionary<string, ItemPrototype> Guns { get; set; }

        [JsonPropertyName("item-with-entity-data")]
        public required Dictionary<string, ItemPrototype> ItemsWithEntityData { get; set; }

        [JsonPropertyName("module")]
        public required Dictionary<string, ItemPrototype> Modules { get; set; }

        [JsonPropertyName("tool")]
        public required Dictionary<string, ItemPrototype> Tools { get; set; }

        [JsonPropertyName("armor")]
        public required Dictionary<string, ItemPrototype> Armors { get; set; }

        [JsonPropertyName("repair-tool")]
        public required Dictionary<string, ItemPrototype> RepairTools { get; set; }
    }
}