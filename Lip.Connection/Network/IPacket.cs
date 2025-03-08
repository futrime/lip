namespace Lip.Connection.Network;

/// <summary>
/// Represents a network packet that can be disposed.
/// </summary>
public interface IPacket : IDisposable
{
}

/// <summary>
/// Represents a generic network packet that can be serialized and deserialized.
/// </summary>
/// <typeparam name="T">The type of the packet.</typeparam>
public interface IPacket<T> : IPacket
    where T : class, IPacket<T>
{
    /// <summary>
    /// Serializes the packet to a byte array.
    /// </summary>
    /// <returns>A byte array representing the serialized packet.</returns>
    public abstract byte[] Serialize();

    /// <summary>
    /// Deserializes a byte array to a packet of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="data">The byte array to deserialize.</param>
    /// <returns>A packet of type <typeparamref name="T"/>.</returns>
    public static abstract T Deserialize(byte[] data);

    /// <summary>
    /// Disposes the packet and suppresses finalization.
    /// </summary>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
