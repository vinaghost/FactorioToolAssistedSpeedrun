using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models.Prototypes
{
    public class LocalePrototype
    {
        [JsonPropertyName("names")]
        public required Dictionary<string, string> Names { get; set; }
    }
}