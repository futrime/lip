using System.Text.Json;
using System.Text.Json.Serialization;
using static Lip.Lip;

namespace Lip.Connection.Network.Packets.LipOperation;

public partial class LipInitPacket : IPacket<LipInitPacket>
{

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(InitArgs))]
    private partial class InitArgsSourceGenerationContext : JsonSerializerContext
    {
    }

    public required InitArgs Args { get; init; }

    public static LipInitPacket Deserialize(byte[] data)
    {
        return new()
        {
            Args = JsonSerializer.Deserialize<InitArgs>(
                data,
                InitArgsSourceGenerationContext.Default.InitArgs) ??
                throw new JsonException("JSON bytes deserialized to null.")
        };
    }
    public byte[] Serialize() =>
        JsonSerializer.SerializeToUtf8Bytes(this, InitArgsSourceGenerationContext.Default.InitArgs);
}
