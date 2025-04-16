using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Migration;

[ExcludeFromCodeCoverage]
public record Manifest
{
    [JsonPropertyName("format_version")]
    public required int FormatVersion { get; set; }

    [JsonPropertyName("format_uuid")]
    public required string FormatUuid { get; set; }

    [JsonPropertyName("tooth")]
    public required string Tooth { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("info")]
    public InfoType? Info { get; set; }

    [JsonPropertyName("variants")]
    public List<Variant>? Variants { get; set; }

    public record Asset
    {
        [JsonPropertyName("type")]
        public required TypeEnum Type { get; set; }

        [JsonPropertyName("urls")]
        public List<string>? Urls { get; set; }

        [JsonPropertyName("placements")]
        public List<Placement>? Placements { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TypeEnum
        {
            [JsonStringEnumMemberName("self")]
            Self,
            [JsonStringEnumMemberName("tar")]
            Tar,
            [JsonStringEnumMemberName("tgz")]
            Tgz,
            [JsonStringEnumMemberName("uncompressed")]
            Uncompressed,
            [JsonStringEnumMemberName("zip")]
            Zip,
        }
    }

    public record InfoType
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }
    }

    public record Placement
    {
        [JsonPropertyName("type")]
        public required TypeEnum Type { get; set; }

        [JsonPropertyName("src")]
        public required string Src { get; set; }

        [JsonPropertyName("dest")]
        public required string Dest { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TypeEnum
        {
            [JsonStringEnumMemberName("file")]
            File,
            [JsonStringEnumMemberName("dir")]
            Dir,
        }
    }

    public record ScriptsType
    {
        [JsonPropertyName("pre_install")]
        public List<string>? PreInstall { get; set; }

        [JsonPropertyName("install")]
        public List<string>? Install { get; set; }

        [JsonPropertyName("post_install")]
        public List<string>? PostInstall { get; set; }

        [JsonPropertyName("pre_pack")]
        public List<string>? PrePack { get; set; }

        [JsonPropertyName("post_pack")]
        public List<string>? PostPack { get; set; }

        [JsonPropertyName("pre_uninstall")]
        public List<string>? PreUninstall { get; set; }

        [JsonPropertyName("uninstall")]
        public List<string>? Uninstall { get; set; }

        [JsonPropertyName("post_uninstall")]
        public List<string>? PostUninstall { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
    }

    public record Variant
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("platform")]
        public string? Platform { get; set; }

        [JsonPropertyName("dependencies")]
        public Dictionary<string, string>? Dependencies { get; set; }

        [JsonPropertyName("assets")]
        public List<Asset>? Assets { get; set; }

        [JsonPropertyName("preserve_files")]
        public List<string>? PreserveFiles { get; set; }

        [JsonPropertyName("remove_files")]
        public List<string>? RemoveFiles { get; set; }

        [JsonPropertyName("scripts")]
        public ScriptsType? Scripts { get; set; }
    }
}