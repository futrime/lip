using Lip.Connection;
using Lip.Connection.Network;
using Lip.Connection.Network.Packets.CustomOperation;
using Lip.Connection.Network.Packets.LipOperation;
using Microsoft.Extensions.Logging;

namespace Lip.Daemon;

internal static class PacketHandlers
{
    public static readonly ILogger<Connection.Connection> logger;

    private static Lip? s_lip;

    static PacketHandlers()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        logger = loggerFactory.CreateLogger<Connection.Connection>();
    }

    public static void LipConstructionPacketHandler(
        Connection.Connection connection,
        LipOperationPackets type,
        LipConstructPacket packet)
    {
        s_lip = new Lip(packet.Config, new ContextImpl());
    }

    public static void TestPackageInstalledPacketHandler(
        Connection.Connection connection,
        CustomOperationPackets type,
        TestPackageInstalledPacket packet)
    {
        //TODO

        //var packages = s_lip.List(new());

        connection.EnqueuePacketToSend<CustomOperationPackets, TestPackageInstalledResponsePacket>(
            CustomOperationPackets.TestPackageInstalledResponse,
            new TestPackageInstalledResponsePacket()
            {
                Manifest = null
            });
    }
}
