using System.Text;

namespace Lip.Connection.Network.Packets.ConnectionVerify;

public class RSAPublicKeyPacket : IPacket<RSAPublicKeyPacket>
{
    public string Key { get; set; } = "";

    public static RSAPublicKeyPacket Deserialize(byte[] data) => new()
    {
        Key = Encoding.UTF8.GetString(data)
    };

    public byte[] Serialize() => Encoding.UTF8.GetBytes(Key);
}
