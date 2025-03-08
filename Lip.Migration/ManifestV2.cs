using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Lip.Migration;

[ExcludeFromCodeCoverage]
public record ManifestV2
{
    [JsonPropertyName("format_version")]
    public required int FormatVersion { get; set; }

    [JsonPropertyName("tooth")]
    public required string Tooth { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("info")]
    public required InfoType Info { get; set; }

    [JsonPropertyName("asset_url")]
    public string? AssetUrl { get; set; }

    [JsonPropertyName("commands")]
    public CommandsType? Commands { get; set; }

    [JsonPropertyName("dependencies")]
    public Dictionary<string, string>? Dependencies { get; set; }

    [JsonPropertyName("prerequisites")]
    public Dictionary<string, string>? Prerequisites { get; set; }

    [JsonPropertyName("files")]
    public FilesType? Files { get; set; }

    [JsonPropertyName("platforms")]
    public List<Platform>? Platforms { get; set; }

    public record InfoType
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("author")]
        public required string Author { get; set; }

        [JsonPropertyName("tags")]
        public required List<string> Tags { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }
    }

    public record CommandsType
    {
        [JsonPropertyName("pre_install")]
        public List<string>? PreInstall { get; set; }

        [JsonPropertyName("post_install")]
        public List<string>? PostInstall { get; set; }

        [JsonPropertyName("pre_uninstall")]
        public List<string>? PreUninstall { get; set; }

        [JsonPropertyName("post_uninstall")]
        public List<string>? PostUninstall { get; set; }
    }

    public record FilesType
    {
        [JsonPropertyName("place")]
        public List<PlaceType>? Place { get; set; }

        [JsonPropertyName("preserve")]
        public List<string>? Preserve { get; set; }

        [JsonPropertyName("remove")]
        public List<string>? Remove { get; set; }
    }

    public record PlaceType
    {
        [JsonPropertyName("src")]
        public required string Src { get; set; }

        [JsonPropertyName("dest")]
        public required string Dest { get; set; }
    }

    public record Platform
    {
        [JsonPropertyName("goarch")]
        public string? GOARCH { get; set; }

        [JsonPropertyName("goos")]
        public required string GOOS { get; set; }

        [JsonPropertyName("asset_url")]
        public string? AssetUrl { get; set; }

        [JsonPropertyName("commands")]
        public CommandsType? Commands { get; set; }

        [JsonPropertyName("dependencies")]
        public Dictionary<string, string>? Dependencies { get; set; }

        [JsonPropertyName("prerequisites")]
        public Dictionary<string, string>? Prerequisites { get; set; }

        [JsonPropertyName("files")]
        public FilesType? Files { get; set; }
    }

}
