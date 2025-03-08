using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Connection.Network.Packets.UserInteraction;

public partial class PromptForInputPacket : IPacket<PromptForInputPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(PromptForInputPacket))]
    internal partial class PromptForInputPacketGenerationContext : JsonSerializerContext
    {
    }

    [JsonPropertyName("default_value")]
    public required string DefaultValue { get; set; }

    [JsonPropertyName("format")]
    public required string Format { get; set; }

    [JsonPropertyName("args")]
    public required IEnumerable<string> Args { get; set; }

    public static PromptForInputPacket Deserialize(byte[] data)
    {
        return JsonSerializer.Deserialize<PromptForInputPacket>(
            data,
            PromptForInputPacketGenerationContext.Default.PromptForInputPacket) ??
            throw new JsonException("JSON bytes deserialized to null.");
    }

    public byte[] Serialize()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            this,
            PromptForInputPacketGenerationContext.Default.PromptForInputPacket);
    }
}
