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
    }
}