using System.Security.Cryptography;

namespace Lip.Connection.Network.Packets.ConnectionVerify;

public class PasswordPacket : IPacket<PasswordPacket>
{
    public required byte[] PasswordData { get; set; }

    public static PasswordPacket Deserialize(byte[] data) => new()
    {
        PasswordData = data
    };

    public byte[] Serialize() => PasswordData;

    public bool Verify(byte[] data, RSACryptoServiceProvider rsa)
        => rsa.Decrypt(PasswordData, false).SequenceEqual(data);
}
