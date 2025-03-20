using Lip.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Lip.Core.Lip;

namespace Lip.Connection.Network.Packets.CustomOperation;

public partial class TestPackageInstalledResponsePacket : IPacket<TestPackageInstalledResponsePacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(TestPackageInstalledResponsePacket))]
    private partial class ListResultItemgenerationContext : JsonSerializerContext
    {
    }

    [JsonPropertyName("manifest")]
    public required ListResultItem? Result { get; set; }

    public static TestPackageInstalledResponsePacket Deserialize(byte[] data)
    {
        return JsonSerializer.Deserialize<TestPackageInstalledResponsePacket>(data) ??
            throw new JsonException("JSON bytes deserialized to null.");
    }

    public byte[] Serialize() =>
        JsonSerializer.SerializeToUtf8Bytes<TestPackageInstalledResponsePacket>(this);
}
