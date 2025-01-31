using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Connection.Network.Packets.LipOperation;

public partial class LipConstructPacket : IPacket<LipConstructPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(RuntimeConfig))]
    private partial class RuntimeConfigSourceGenerationContext : JsonSerializerContext
    {
    }

    public required RuntimeConfig Config { get; set; }

    public static LipConstructPacket Deserialize(byte[] data)
    {
        return new()
        {
            Config = JsonSerializer.Deserialize<RuntimeConfig>(
                data,
                RuntimeConfigSourceGenerationContext.Default.RuntimeConfig) ??
                throw new JsonException("JSON bytes deserialized to null.")
        };
    }

    public byte[] Serialize() =>
        JsonSerializer.SerializeToUtf8Bytes(this, RuntimeConfigSourceGenerationContext.Default.RuntimeConfig);
}
