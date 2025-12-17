using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models
{
    public class RecipePrototype : PrototypeBase
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = "crafting";

        [JsonConverter(typeof(NullOnEmptyObjectConverter<List<IngredientPrototype>>))]
        [JsonPropertyName("ingredients")]
        public List<IngredientPrototype>? Ingredients { get; set; }

        [JsonConverter(typeof(NullOnEmptyObjectConverter<List<ProductPrototype>>))]
        [JsonPropertyName("results")]
        public List<ProductPrototype>? Results { get; set; }
    }

    public class IngredientPrototype
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("amount")]
        public required int Amount { get; set; }
    }

    public class ProductPrototype
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("amount")]
        public required int Amount { get; set; }
    }

    public class NullOnEmptyObjectConverter<T> : JsonConverter<T?> where T : class
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Peek ahead to see if it's an empty object
                var clone = reader; // struct copy
                if (clone.Read() && clone.TokenType == JsonTokenType.EndObject)
                {
                    reader.Read(); // advance past EndObject
                    return null;
                }
            }
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}