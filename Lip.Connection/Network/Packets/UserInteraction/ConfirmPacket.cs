using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Connection.Network.Packets.UserInteraction;

public partial class ConfirmPacket : IPacket<ConfirmPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfirmPacket))]
    internal partial class ConfirmPacketGenerationContext : JsonSerializerContext
    {
    }

    [JsonPropertyName("format")]
    public required string Format { get; set; }

    [JsonPropertyName("args")]
    public required IEnumerable<string> Args { get; set; }

    public static ConfirmPacket Deserialize(byte[] data)
    {
        return JsonSerializer.Deserialize<ConfirmPacket>(
            data,
            ConfirmPacketGenerationContext.Default.ConfirmPacket) ??
            throw new JsonException("JSON bytes deserialized to null.");
    }

    public byte[] Serialize()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            this,
            ConfirmPacketGenerationContext.Default.ConfirmPacket);
    }
}
