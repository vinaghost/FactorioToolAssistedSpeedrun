using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models.Prototypes
{
    public class RecipePrototype : PrototypeBase
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = "crafting";

        [JsonConverter(typeof(NullOnEmptyObjectConverter))]
        [JsonPropertyName("ingredients")]
        public List<ProductPrototype>? Input { get; set; }

        [JsonConverter(typeof(NullOnEmptyObjectConverter))]
        [JsonPropertyName("results")]
        public List<ProductPrototype>? Output { get; set; }
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

    public class NullOnEmptyObjectConverter : JsonConverter<List<ProductPrototype>?>
    {
        public override List<ProductPrototype>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            return JsonSerializer.Deserialize<List<ProductPrototype>>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, List<ProductPrototype>? value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}