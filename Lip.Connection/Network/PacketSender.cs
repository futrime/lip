using System.Net.Sockets;
using Lip.Connection.Network.Packets;

namespace Lip.Connection.Network;
/// <summary>
/// Provides functionality to send packets over a network stream.
/// </summary>
public class PacketSender(Connection connection)
{
    /// <summary>
    /// Asynchronously sends a packet over the provided network stream.
    /// </summary>
    /// <typeparam name="TPacketType">The type of the packet type enum.</typeparam>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <param name="packetType">The type of the packet to send.</param>
    /// <param name="packet">The packet to send.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async ValueTask SendPacketAsync<TPacketType, TPacket>(
        TPacketType packetType,
        TPacket packet,
        bool encryptData = true,
        CancellationToken token = default)
        where TPacketType : struct, Enum
        where TPacket : class, IPacket<TPacket>
    {
        await connection.SendPacketAsync(packetType, packet, encryptData, token);
    }
}
