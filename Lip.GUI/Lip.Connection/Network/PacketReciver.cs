using System.Net.Sockets;

namespace Lip.Connection.Network;

/// <summary>
/// A class responsible for receiving packets from a network stream.
/// </summary>
public class PacketReciver
{
    /// <summary>
    /// Asynchronously receives a packet from the specified network stream.
    /// </summary>
    /// <typeparam name="TPacketType">The type of the packet type enumeration.</typeparam>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <param name="stream">The network stream to read from.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains a KeyValuePair of the packet type and the packet, or null if the operation was canceled or no data was received.</returns>
    public static async ValueTask<KeyValuePair<TPacketType, TPacket>> RecivePacketAsync<TPacketType, TPacket>(
        Connection? connection,
        NetworkStream stream,
        CancellationToken token = default)
        where TPacketType : struct, Enum
        where TPacket : class, IPacket<TPacket>, new()
    {
        Span<byte> typeBuffer = stackalloc byte[4];
        Span<byte> lengthBuffer = stackalloc byte[4];

        if (token.IsCancellationRequested) throw new TaskCanceledException();

        int bytesReceived = stream.Read(typeBuffer);
        if (bytesReceived is 0) throw new InvalidOperationException("No data received.");
        bytesReceived = stream.Read(lengthBuffer);
        if (bytesReceived is 0) throw new InvalidOperationException("No data received.");

        TPacketType type = (TPacketType)Enum.ToObject(typeof(TPacketType), BitConverter.ToInt32(typeBuffer));
        int length = BitConverter.ToInt32(lengthBuffer);

        byte[] dataBuffer = new byte[length];
        Memory<byte> memory = dataBuffer.AsMemory();
        int bytesRead = 0;

        while (bytesRead < length) bytesRead +=
                await stream.ReadAsync(memory.Slice(bytesRead), token).ConfigureAwait(false);

        if (Enum.IsDefined(typeof(TPacketType), bytesReceived) is false)
            throw new InvalidOperationException("Invalid packet type.");

        return KeyValuePair.Create(type, TPacket.Deserialize(connection is null ?
            dataBuffer :
            connection.DecryptData(dataBuffer)));
    }
}
