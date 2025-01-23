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

    [JsonPropertyName("git")]
    public string Git { get; init; } = "git";

    [JsonPropertyName("github_proxy")]
    public string GitHubProxy { get; init; } = "";

    [JsonPropertyName("go_module_proxy")]
    public string GoModuleProxy { get; init; } = "https://goproxy.io";

    [JsonPropertyName("https_proxy")]
    public string HttpsProxy { get; init; } = "";

    [JsonPropertyName("noproxy")]
    public string NoProxy { get; init; } = "";

    [JsonPropertyName("proxy")]
    public string Proxy { get; init; } = "";

    [JsonPropertyName("script_shell")]
    public string ScriptShell { get; init; } = OperatingSystem.IsWindows()
        ? "cmd.exe"
        : "/bin/sh";

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

    public byte[] ToBytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this, s_jsonSerializerOptions);
    }
}
