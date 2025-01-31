using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Connection.Network.Packets;

internal partial record Packet
{
    public required int Length { get; set; }

    public required bool IsEncrypted { get; set; }

    public required string PacketIdTypeName { get; set; }

    public required long PacketId { get; set; }

    public required byte[] Data { get; set; }

    private partial record struct PacketData
    {
        [JsonSourceGenerationOptions(WriteIndented = true)]
        [JsonSerializable(typeof(PacketData))]
        internal partial class GenerationContext : JsonSerializerContext
        {
        }

        [JsonPropertyName("type_name")]
        public string PacketIdTypeName { get; set; }

        [JsonPropertyName("packet_id")]
        public long PacketId { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    public static async Task<Packet> ReadAsync(NetworkStream stream, Connection connection)
    {
        Span<byte> lengthBuffer = stackalloc byte[4];
        Span<byte> isEncryptedBuffer = stackalloc byte[1];
        Span<byte> typeBuffer = stackalloc byte[4];

        int bytesReceived = stream.Read(lengthBuffer);
        if (bytesReceived is 0) throw new InvalidOperationException("No data received.");
        bytesReceived = stream.Read(isEncryptedBuffer);
        if (bytesReceived is 0) throw new InvalidOperationException("No data received.");

        bool isEncrypted = BitConverter.ToBoolean(isEncryptedBuffer);
        int length = BitConverter.ToInt32(lengthBuffer);

        byte[] dataBuffer = new byte[length];
        int bytesRead = 0;

        while (bytesRead < length) bytesRead += await stream.ReadAsync(
            dataBuffer.AsMemory(bytesRead, dataBuffer.Length - bytesRead));

        if (isEncrypted) dataBuffer = connection.DecryptData(dataBuffer);

        PacketData data = JsonSerializer.Deserialize<PacketData>(
            dataBuffer,
            PacketData.GenerationContext.Default.PacketData);

        return new()
        {
            Length = length,
            IsEncrypted = isEncrypted,
            PacketIdTypeName = data.PacketIdTypeName,
            PacketId = data.PacketId,
            Data = Encoding.UTF8.GetBytes(data.Data)
        };
    }

    public async Task WriteAsync(NetworkStream stream, Connection connection)
    {
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(
            new PacketData
            {
                PacketIdTypeName = PacketIdTypeName,
                PacketId = PacketId,
                Data = Encoding.UTF8.GetString(Data)
            },
            PacketData.GenerationContext.Default.PacketData);

        if (IsEncrypted) data = connection.EncryptData(data);

        await stream.WriteAsync(BitConverter.GetBytes(data.Length));
        await stream.WriteAsync(BitConverter.GetBytes(IsEncrypted));
        await stream.WriteAsync(data);
    }
}
