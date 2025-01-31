using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Connection.Network.Packets.UserInteraction;

public partial class PromptForSelectionPacket : IPacket<PromptForSelectionPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(PromptForSelectionPacket))]
    internal partial class PromptForSelectionPacketGenerationContext : JsonSerializerContext
    {
    }

    [JsonPropertyName("args")]
    public required IEnumerable<string> Options { get; set; }

    [JsonPropertyName("format")]
    public required string Format { get; set; }

    [JsonPropertyName("args")]
    public required IEnumerable<string> Args { get; set; }

    public static PromptForSelectionPacket Deserialize(byte[] data)
    {
        return JsonSerializer.Deserialize<PromptForSelectionPacket>(
            data,
            PromptForSelectionPacketGenerationContext.Default.PromptForSelectionPacket) ??
            throw new JsonException("JSON bytes deserialized to null.");
    }

    public byte[] Serialize()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            this,
            PromptForSelectionPacketGenerationContext.Default.PromptForSelectionPacket);
    }
}
