using System.Net.Sockets;

namespace Lip.Connection.Network;

public interface IPacketHandler
{
    //public void Start(NetworkStream stream, Action? onBytesRecived = null, CancellationToken token = default);
    public Type PacketType { get; }

    public void OnPacketReceived(Connection connection, Enum type, byte[] data);
}


/// <summary>
/// Handles packets of a specified type.
/// </summary>
/// <typeparam name="TPacketType">The type of the packet.</typeparam>
public class PacketHandler<TPacketType>
    : IPacketHandler
    where TPacketType : Enum
{
    public Type PacketType => typeof(TPacketType);

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
    private sealed class PacketHandlerBinding<TPacket>(Action<Connection, TPacketType, TPacket> action)
        where TPacket : class, IPacket<TPacket>
    {
        /// <summary>
        /// Invokes the action with the specified packet.
        /// </summary>
        /// <param name="packet">The packet to handle.</param>
        public void Invoke(Connection connection, Enum type, IPacket packet)
            => action(connection, (TPacketType)type, (TPacket)packet);
    }

    /// <summary>
    /// A dictionary that maps packet types to their deserialization function pointers and handlers.
    /// </summary>
    private readonly Dictionary<TPacketType, (DeserializeFunctionPointer fptr, Action<Connection, Enum, IPacket> action)> _handlers = [];

    /// <summary>
    /// Sets a handler for a specified packet type.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <param name="type">The packet type.</param>
    /// <param name="handler">The handler action.</param>
    public void SetHandler<TPacket>(TPacketType type, Action<Connection, TPacketType, TPacket> handler)
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
    public void SetHandlers<TPacket>(IDictionary<TPacketType, Action<Connection, TPacketType, TPacket>> handlers)
        where TPacket : class, IPacket<TPacket>
    {
        foreach (var handler in handlers)
            SetHandler(handler.Key, handler.Value);
    }

    /// <summary>
    /// Starts reading packets from the network stream.
    /// </summary>
    /// <param name="stream">The network stream.</param>
    /// <param name="token">The cancellation token.</param>
    public unsafe void OnPacketReceived(Connection connection, Enum type, byte[] data)
    {
        (DeserializeFunctionPointer fptr, Action<Connection, Enum, IPacket> action) = _handlers[(TPacketType)type];
        IPacket packet = fptr.Target(data);
        Task.Run(() => _handlers[(TPacketType)type].action(connection, type, packet));
    }
}
