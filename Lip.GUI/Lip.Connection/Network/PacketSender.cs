using System.Net.Sockets;

namespace Lip.Connection.Network;
/// <summary>
/// Provides functionality to send packets over a network stream.
/// </summary>
public class PacketSender
{
    /// <summary>
    /// Asynchronously sends a packet over the provided network stream.
    /// </summary>
    /// <typeparam name="TPacketType">The type of the packet type enum.</typeparam>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <param name="packetType">The type of the packet to send.</param>
    /// <param name="packet">The packet to send.</param>
    /// <param name="stream">The network stream to send the packet over.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async ValueTask SendPacketAsync<TPacketType, TPacket>(
        Connection? connection,
        NetworkStream stream,
        TPacketType packetType,
        TPacket packet,
        CancellationToken token = default)
        where TPacketType : struct, Enum
        where TPacket : class, IPacket<TPacket>
    {
        byte[] serializedPacket = connection is null ?
            packet.Serialize() :
            connection.EncryptData(packet.Serialize());

        await stream.WriteAsync(BitConverter.GetBytes(Convert.ToInt32(packetType)).AsMemory(), token);
        await stream.WriteAsync(BitConverter.GetBytes(serializedPacket.Length).AsMemory(), token);
        await stream.WriteAsync(serializedPacket, token);
    }
}
