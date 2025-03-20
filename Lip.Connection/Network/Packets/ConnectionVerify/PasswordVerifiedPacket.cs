namespace Lip.Connection.Network.Packets.ConnectionVerify;
public class PasswordVerifiedPacket : IPacket<PasswordVerifiedPacket>
{
    public required bool Value { get; set; }

    public static PasswordVerifiedPacket Deserialize(byte[] data) => new() { Value = BitConverter.ToBoolean(data) };

    public byte[] Serialize() => BitConverter.GetBytes(Value);
}
