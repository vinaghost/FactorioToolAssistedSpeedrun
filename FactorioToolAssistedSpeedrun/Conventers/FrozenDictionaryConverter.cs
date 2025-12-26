using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Conventers
{
    public class FrozenDictionaryConverter<TValue> : JsonConverter<FrozenDictionary<string, TValue>>
    {
        public override FrozenDictionary<string, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, TValue>>(ref reader, options);
            return dictionary?.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        }

        public override void Write(Utf8JsonWriter writer, FrozenDictionary<string, TValue> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (IDictionary<string, TValue>)value, options);
        }
    }
}