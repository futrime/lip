using static Lip.Core.Lip;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Lip.Connection.Network.Packets.LipOperation;

public partial class LipInstallPacket : IPacket<LipInstallPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(LipInstallPacket))]
    private partial class LipInstallPacketSourceGenerationContext : JsonSerializerContext
    {
    }

    public required List<string> Packages { get; init; }

    public required InstallArgs Args { get; init; }

    public static LipInstallPacket Deserialize(byte[] data)
    {
        return JsonSerializer.Deserialize<LipInstallPacket>(
                data,
                LipInstallPacketSourceGenerationContext.Default.LipInstallPacket) ??
                throw new JsonException("JSON bytes deserialized to null.");
    }

    public byte[] Serialize() =>
        JsonSerializer.SerializeToUtf8Bytes(this, LipInstallPacketSourceGenerationContext.Default.LipInstallPacket);
}
