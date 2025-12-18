using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models.Prototypes
{
    public class PrototypeBase
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("order")]
        public string Order { get; set; } = "-";

        [JsonPropertyName("parameter")]
        public bool Parameter { get; set; }

        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }
    }
}