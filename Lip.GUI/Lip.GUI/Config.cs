using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lip.GUI;

public partial class ServerInfo : ObservableObject
{
    [ObservableProperty]
    public required partial string ServerName { get; set; }

    [ObservableProperty]
    [JsonPropertyName("host")]
    public required partial string ServerHost { get; set; }

    [ObservableProperty]
    [JsonPropertyName("port")]
    public required partial int? ServerPort { get; set; }

    [ObservableProperty]
    [JsonPropertyName("password")]
    public required partial string? ServerPassword { get; set; }

    [ObservableProperty]
    [JsonPropertyName("icon")]
    public required partial Guid? ServerIconId { get; set; }

    [ObservableProperty]
    [JsonPropertyName("text_color")]
    public required partial string? TextColor { get; set; }
}

public partial class Config : ObservableObject
{
    internal static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static Config Deserialize(string json) => JsonSerializer.Deserialize<Config>(json)!;

    public string Serialize()
    {
        lock (this)
        {
            return JsonSerializer.Serialize(this, s_options);
        }
    }

    [ObservableProperty]
    [JsonPropertyName("port")]
    public partial int Port { get; set; } = 2333;

    [ObservableProperty]
    [JsonPropertyName("servers")]
    public required partial ObservableCollection<ServerInfo> Servers { get; set; }
}
