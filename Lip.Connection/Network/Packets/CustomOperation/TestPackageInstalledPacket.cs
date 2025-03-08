using System.Text;

namespace Lip.Connection.Network.Packets.CustomOperation;

public class TestPackageInstalledPacket : IPacket<TestPackageInstalledPacket>
{
    public required string Identifier { get; set; }

    public static TestPackageInstalledPacket Deserialize(byte[] data) => new()
    {
        Identifier = Encoding.UTF8.GetString(data)
    };

    public byte[] Serialize() => Encoding.UTF8.GetBytes(Identifier);
}
