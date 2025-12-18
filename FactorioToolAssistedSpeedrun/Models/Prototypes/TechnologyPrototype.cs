using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models.Prototypes
{
    public class TechnologyPrototype : PrototypeBase
    {
        [JsonPropertyName("unit")]
        public TechnologyUnit? Unit { get; set; }
    }

    public struct TechnologyUnit
    {
        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("count_formula")]
        public string? CountFormula { get; set; }

        [JsonPropertyName("ingredients")]
        [JsonConverter(typeof(ResearchIngredientListConverter))]
        public required List<ResearchIngredient> Ingredients { get; set; }

        [JsonPropertyName("time")]
        public required double Time { get; set; }
    }

    public record ResearchIngredient(string Item, int Amount);

    public class ResearchIngredientListConverter : JsonConverter<List<ResearchIngredient>>
    {
        public override List<ResearchIngredient> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = new List<ResearchIngredient>();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException();

                reader.Read();
                var item = reader.GetString();
                reader.Read();
                var amount = reader.GetInt32();
                reader.Read();

                if (reader.TokenType != JsonTokenType.EndArray)
                    throw new JsonException();

                list.Add(new ResearchIngredient(item!, amount));
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<ResearchIngredient> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var ingredient in value)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(ingredient.Item);
                writer.WriteNumberValue(ingredient.Amount);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}