using Lip.Connection.Network.Packets.Operations;
using Lip.Connection.Operations;
using Microsoft.Extensions.Logging;

namespace Lip.Daemon;

internal static class PacketHandlers
{
    public static readonly ILogger<Connection.Connection> logger;

    static PacketHandlers()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        logger = loggerFactory.CreateLogger<Connection.Connection>();
    }

    public static void OperationsHandler(OperationType type, OperationPacket packet)
    {
        switch (type)
        {
            case OperationType.Init:

                //var lipInstance = new Lip(
                //    runtimeConfig: new(),
                //    fileSystem: new System.IO.Abstractions.FileSystem(),
                //    logger: logger,
                //    )

                break;
        }
    }
}
