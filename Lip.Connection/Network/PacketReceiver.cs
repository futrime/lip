using System.Net.Sockets;
using Lip.Connection.Network.Packets;

namespace Lip.Connection.Network;

/// <summary>
/// A class responsible for receiving packets from a network stream.
/// </summary>
public class PacketReceiver(Connection connection)
{
    /// <summary>
    /// Asynchronously receives a packet from the specified network stream.
    /// </summary>
    /// <typeparam name="TPacketType">The type of the packet type enumeration.</typeparam>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains a KeyValuePair of the packet type and the packet, or null if the operation was canceled or no data was received.</returns>
    public async ValueTask<TPacket> RecivePacketAsync<TPacketType, TPacket>(
        TPacketType type,
        CancellationToken token = default)
        where TPacketType : struct, Enum
        where TPacket : class, IPacket<TPacket>
    {
        return await connection.RequestPacketAsync<TPacketType, TPacket>(type);
    }
}
