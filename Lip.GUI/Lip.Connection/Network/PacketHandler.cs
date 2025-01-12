using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Lip.Connection.Network;

public interface IPacketHandler
{
    public void Start(NetworkStream stream, Action? onBytesRecived = null, CancellationToken token = default);
}


/// <summary>
/// Handles packets of a specified type.
/// </summary>
/// <typeparam name="TPacketType">The type of the packet.</typeparam>
public class PacketHandler<TPacketType>(Connection connection)
    : IPacketHandler
    where TPacketType : Enum
{
    /// <summary>
    /// A record struct that holds a function pointer for deserialization.
    /// </summary>
    private readonly unsafe struct DeserializeFunctionPointer
    {
        /// <summary>
        /// The function pointer for deserialization.
        /// </summary>
        public required delegate* managed<byte[], IPacket> Target { get; init; }
    }

    /// <summary>
    /// Binds a packet handler to an action.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    private sealed class PacketHandlerBinding<TPacket>(Action<TPacketType, TPacket> action)
        where TPacket : class, IPacket<TPacket>
    {
        /// <summary>
        /// Invokes the action with the specified packet.
        /// </summary>
        /// <param name="packet">The packet to handle.</param>
        public void Invoke(int type, IPacket packet)
            => action((TPacketType)Enum.ToObject(typeof(TPacketType), type), (TPacket)packet);
    }

    /// <summary>
    /// A dictionary that maps packet types to their deserialization function pointers and handlers.
    /// </summary>
    private readonly Dictionary<TPacketType, (DeserializeFunctionPointer fptr, Action<int, IPacket> action)> _handlers = [];

    /// <summary>
    /// Sets a handler for a specified packet type.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <param name="type">The packet type.</param>
    /// <param name="handler">The handler action.</param>
    public void SetHandler<TPacket>(TPacketType type, Action<TPacketType, TPacket> handler)
        where TPacket : class, IPacket<TPacket>
    {
        _handlers[type] = (
            fptr: new DeserializeFunctionPointer() { Target = &TPacket.Deserialize },
            action: new PacketHandlerBinding<TPacket>(handler).Invoke);
    }

    /// <summary>
    /// Sets a handler for a specified packet type.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <param name="handlers">The packet handlers.</param>
    public void SetHandlers<TPacket>(IDictionary<TPacketType, Action<TPacketType, TPacket>> handlers)
        where TPacket : class, IPacket<TPacket>
    {
        foreach (KeyValuePair<TPacketType, Action<TPacketType, TPacket>> handler in handlers)
            SetHandler(handler.Key, handler.Value);
    }

    /// <summary>
    /// Starts reading packets from the network stream.
    /// </summary>
    /// <param name="stream">The network stream.</param>
    /// <param name="token">The cancellation token.</param>
    public void Start(NetworkStream stream, Action? onBytesRecived = null, CancellationToken token = default) => Task.Run(() =>
    {
        Span<byte> typeBuffer = stackalloc byte[4];
        Span<byte> lengthBuffer = stackalloc byte[4];

        while (true)
        {
            if (token.IsCancellationRequested) return;

            int bytesReceived = stream.Read(typeBuffer);
            if (bytesReceived is 0) break;
            bytesReceived = stream.Read(lengthBuffer);
            if (bytesReceived is 0) break;

            int typeValue = BitConverter.ToInt32(typeBuffer);
            TPacketType type = (TPacketType)Enum.ToObject(typeof(TPacketType), typeValue);
            int length = BitConverter.ToInt32(lengthBuffer);

            byte[] dataBuffer = new byte[length];
            int bytesRead = 0;

            while (bytesRead < length) bytesRead += stream.Read(
                dataBuffer,
                bytesRead,
                dataBuffer.Length - bytesRead);

            IPacket packet;
            unsafe { packet = _handlers[type].fptr.Target(connection.DecryptData(dataBuffer)); }
            _ = Task.Run(() => _handlers[type].action(typeValue, packet)).ConfigureAwait(false);
        }

        onBytesRecived?.Invoke();
    }, token);
}
