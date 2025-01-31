using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip;

/// <summary>
/// Represents the runtime configuration.
/// </summary>
public record RuntimeConfig
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IndentSize = 4,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };

    [JsonPropertyName("cache")]
    public string Cache { get; init; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lip", "cache");

    [JsonPropertyName("color")]
    public bool Color { get; init; } = true;

    [JsonPropertyName("github_proxy")]
    public string GitHubProxy { get; init; } = string.Empty;

    [JsonPropertyName("go_module_proxy")]
    public string GoModuleProxy { get; init; } = "https://goproxy.io";

    [JsonPropertyName("https_proxy")]
    public string HttpsProxy { get; init; } = string.Empty;

    [JsonPropertyName("noproxy")]
    public string NoProxy { get; init; } = string.Empty;

    [JsonPropertyName("proxy")]
    public string Proxy { get; init; } = string.Empty;

    public static RuntimeConfig FromJsonBytes(byte[] bytes)
    {
        try
        {
            return JsonSerializer.Deserialize<RuntimeConfig>(
                bytes,
                s_jsonSerializerOptions
            ) ?? throw new JsonException("JSON bytes deserialized to null.");
        }
        catch (Exception ex) when (ex is JsonException)
        {
            throw new JsonException("Runtime config bytes deserialization failed.", ex);
        }
    }

    public byte[] ToJsonBytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this, s_jsonSerializerOptions);
    }
}
