using Lip.Connection.Network;
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
        PacketHandler<LipOperationPackets> handler,
        LipOperationPackets type,
        LipConstructPacket packet)
    {
        s_lip = new Lip(packet.Config, new ContextImpl());
    }
}
