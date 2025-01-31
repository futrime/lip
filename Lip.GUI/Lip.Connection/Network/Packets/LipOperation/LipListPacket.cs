using System.Text.Json;
using System.Text.Json.Serialization;
using static Lip.Lip;

namespace Lip.Connection.Network.Packets.LipOperation;

public partial class LipListPacket : IPacket<LipListPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ListArgs))]
    private partial class ListArgsSourceGenerationContext : JsonSerializerContext
    {
    }

    public required ListArgs Args { get; set; }

    public static LipListPacket Deserialize(byte[] data)
    {
        return new()
        {
            Args = JsonSerializer.Deserialize<ListArgs>(
                data,
                ListArgsSourceGenerationContext.Default.ListArgs) ??
                throw new JsonException("JSON bytes deserialized to null.")
        };
    }

    public byte[] Serialize() =>
        JsonSerializer.SerializeToUtf8Bytes(this, ListArgsSourceGenerationContext.Default.ListArgs);
}
