using System.Text;
using Lip.Connection.Network;
using Lip.Connection.Operations;
using static Lip.Connection.Operations.Operation;

namespace Lip.Connection.Network.Packets.Operations;

public class OperationPacket : IPacket<OperationPacket>
{
    public required OperationType Operation { get; init; }

    public required string? Message { get; init; }

    public static OperationPacket Deserialize(byte[] data)
    {
        if (data is null || data.Length < sizeof(int))
        {
            throw new ArgumentException("Invalid data for deserialization", nameof(data));
        }

        int operationType = BitConverter.ToInt32(data, 0);
        string message = Encoding.UTF8.GetString(data, sizeof(int), data.Length - sizeof(int));

        return new OperationPacket
        {
            Operation = (OperationType)operationType,
            Message = message
        };
    }

    public byte[] Serialize()
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(Message ?? string.Empty);
        byte[] data = new byte[messageBytes.Length + sizeof(int)];
        Buffer.BlockCopy(messageBytes, 0, data, sizeof(int), messageBytes.Length);
        Buffer.BlockCopy(BitConverter.GetBytes((int)Operation), 0, data, 0, sizeof(int));
        return data;
    }
}
