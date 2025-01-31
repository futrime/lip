using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Connection.Network.Packets.UserInteraction;

public partial class UpdateProgressPacket : IPacket<UpdateProgressPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(UpdateProgressPacket))]
    internal partial class UpdateProgressPacketGenerationContext : JsonSerializerContext
    {
    }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("progress")]
    public required float Progress { get; set; }

    [JsonPropertyName("format")]
    public required string Format { get; set; }

    [JsonPropertyName("args")]
    public required IEnumerable<string> Args { get; set; }

    public static UpdateProgressPacket Deserialize(byte[] data)
    {
        return JsonSerializer.Deserialize<UpdateProgressPacket>(
            data,
            UpdateProgressPacketGenerationContext.Default.UpdateProgressPacket) ??
            throw new JsonException("JSON bytes deserialized to null.");
    }

    public byte[] Serialize()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            this,
            UpdateProgressPacketGenerationContext.Default.UpdateProgressPacket);
    }
}
