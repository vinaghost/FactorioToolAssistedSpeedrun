using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models
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