using static Lip.Core.Lip;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Lip.Connection.Network.Packets.LipOperation;

public partial class LipUninstallPacket : IPacket<LipUninstallPacket>
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(LipUninstallPacket))]
    private partial class LipUninstallPacketSourceGenerationContext : JsonSerializerContext
    {
    }

    public required List<string> Packages { get; init; }

    public required UninstallArgs Args { get; init; }

    public static LipUninstallPacket Deserialize(byte[] data)
    {
        return JsonSerializer.Deserialize<LipUninstallPacket>(
                data,
                LipUninstallPacketSourceGenerationContext.Default.LipUninstallPacket) ??
                throw new JsonException("JSON bytes deserialized to null.");
    }

    public byte[] Serialize() =>
        JsonSerializer.SerializeToUtf8Bytes(this, LipUninstallPacketSourceGenerationContext.Default.LipUninstallPacket);
}
