using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Connection.Network.Packets;

internal partial record Packet
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Packet))]
    private partial class GenerationContext : JsonSerializerContext { }

    [JsonPropertyName("is_encrypted")]
    public required bool IsEncrypted { get; set; }

    [JsonPropertyName("packet_id_typename")]
    public required string PacketIdTypeName { get; set; }

    [JsonPropertyName("packet_id")]
    public required long PacketId { get; set; }

    [JsonPropertyName("data")]
    public required byte[] Data { get; set; }

    public static async Task<Packet> ReadAsync(NetworkStream stream, Connection connection)
    {
        byte[] buffer = new byte[4];
        await stream.ReadExactlyAsync(buffer);
        int packetLength = BitConverter.ToInt32(buffer, 0);

        buffer = new byte[packetLength];
        await stream.ReadExactlyAsync(buffer);

        string json = Encoding.UTF8.GetString(buffer);
        Packet packet = JsonSerializer.Deserialize<Packet>(json, GenerationContext.Default.Packet) ??
            throw new JsonException("JSON bytes deserialized to null.");

        if (packet.IsEncrypted)
        {
            packet.Data = connection.DecryptData(packet.Data);
        }

        return packet;
    }

    public async Task WriteAsync(NetworkStream stream, Connection connection)
    {
        byte[] dataToSend = IsEncrypted ? connection.EncryptData(Data) : Data;
        string json = JsonSerializer.Serialize(this with { Data = dataToSend },
            GenerationContext.Default.Packet);
        byte[] buffer = Encoding.UTF8.GetBytes(json);

        byte[] lengthBuffer = BitConverter.GetBytes(buffer.Length);
        await stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length);
        await stream.WriteAsync(buffer, 0, buffer.Length);
    }
}
