using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lip.Connection.Network.Packets.ConnectionVerify;
public enum ConnectionVerifyPackets
{
    RSAPublicKey,
    Password,
    PasswordVerified,
    AesKey,
    AesKeyReceived
}
