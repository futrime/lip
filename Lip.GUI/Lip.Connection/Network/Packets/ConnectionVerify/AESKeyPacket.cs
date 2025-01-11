namespace Lip.Connection.Network.Packets.ConnectionVerify;
public class AESKeyPacket : IPacket<AESKeyPacket>
{
    public byte[] Key { get; set; } = [];

    public static AESKeyPacket Deserialize(byte[] data) => new() { Key = data };
    public byte[] Serialize() => Key;
}
