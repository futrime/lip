namespace Lip.Connection.Network.Packets.ConnectionVerify;

public class AESKeyReceivedPacket : IPacket<AESKeyReceivedPacket>
{
    public required bool Value { get; set; }

    public static AESKeyReceivedPacket Deserialize(byte[] data) => new() { Value = BitConverter.ToBoolean(data) };

    public byte[] Serialize() => BitConverter.GetBytes(Value);
}
