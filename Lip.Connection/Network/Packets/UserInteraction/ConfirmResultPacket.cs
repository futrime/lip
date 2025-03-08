namespace Lip.Connection.Network.Packets.UserInteraction;

public class ConfirmResultPacket : IPacket<ConfirmResultPacket>
{
    public required bool Value { get; set; }

    public static ConfirmResultPacket Deserialize(byte[] data) => new() { Value = BitConverter.ToBoolean(data) };

    public byte[] Serialize() => BitConverter.GetBytes(Value);
}
