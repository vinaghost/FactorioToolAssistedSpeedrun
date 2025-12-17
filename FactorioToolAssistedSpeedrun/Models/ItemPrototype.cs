using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models
{
    public class ItemPrototype : PrototypeBase
    {
        [JsonPropertyName("stack_size")]
        public int StackSize { get; set; }

        [JsonPropertyName("place_as_tile ")]
        public PlaceAsTile? PlaceAsTile { get; set; }

        [JsonPropertyName("place_result")]
        public string? PlaceResult { get; set; }
    }

    public struct PlaceAsTile
    {
        [JsonPropertyName("result")]
        public required string TileId { get; set; }
    }
}