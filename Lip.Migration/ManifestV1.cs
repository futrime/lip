using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Migration;

[ExcludeFromCodeCoverage]
public record ManifestV1
{
    [JsonPropertyName("format_version")]
    public required int FormatVersion { get; set; }

    [JsonPropertyName("tooth")]
    public required string Tooth { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("dependencies")]
    public Dictionary<string, List<List<string>>>? Dependencies { get; set; }

    [JsonPropertyName("information")]
    public InformationType? Information { get; set; }

    [JsonPropertyName("placement")]
    public List<PlacementType>? Placement { get; set; }

    [JsonPropertyName("possession")]
    public List<string>? Possession { get; set; }

    [JsonPropertyName("commands")]
    public List<Command>? Commands { get; set; }

    public record InformationType
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Data { get; set; }
    }

    public record PlacementType
    {
        [JsonPropertyName("source")]
        public required string Source { get; set; }

        [JsonPropertyName("destination")]
        public required string Destination { get; set; }

        [JsonPropertyName("GOOS")]
        public string? GOOS { get; set; }

        [JsonPropertyName("GOARCH")]
        public string? GOARCH { get; set; }
    }

    public record Command
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("commands")]
        public required List<string> Commands { get; set; }

        [JsonPropertyName("GOOS")]
        public required string GOOS { get; set; }

        [JsonPropertyName("GOARCH")]
        public string? GOARCH { get; set; }
    }
}