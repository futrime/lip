using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Lip.Connection.Network.Packets.ConnectionVerify;
public class PasswordVerifiedPacket : IPacket<PasswordVerifiedPacket>
{
    public bool Value { get; set; }

    public static PasswordVerifiedPacket Deserialize(byte[] data) => new() { Value = BitConverter.ToBoolean(data) };

    public byte[] Serialize() => BitConverter.GetBytes(Value);
}
