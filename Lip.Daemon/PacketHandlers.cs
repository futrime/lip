using Lip.Connection.Network.Packets.CustomOperation;
using Lip.Connection.Network.Packets.LipOperation;
using Lip.Context;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Lip.Daemon;

internal static class PacketHandlers
{
    public static readonly ILogger<Connection.Connection> logger;

    private static Core.Lip? s_lip;

    static PacketHandlers()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        logger = loggerFactory.CreateLogger<Connection.Connection>();
    }

    public static void LipConstructionPacketHandler(
        Connection.Connection connection,
        LipOperationPackets _,
        LipConstructPacket packet) => Task.Run(async () =>
        {
            var userInteraction = new UserInteractionImpl(connection);

            s_lip = Core.Lip.Create(packet.Config, new Context.Context()
            {
                CommandRunner = new CommandRunner(),
                Downloader = new Context.Downloader(userInteraction),
                FileSystem = new FileSystem(),
                Git = await StandaloneGit.Create(),
                Logger = logger,
                UserInteraction = new UserInteractionImpl(connection),
                WorkingDir = Directory.GetCurrentDirectory()
            });
        });

    public static void TestPackageInstalledPacketHandler(
        Connection.Connection connection,
        CustomOperationPackets type,
        TestPackageInstalledPacket packet) => Task.Run(async () =>
        {
            //TODO

            var packages = await s_lip!.List(new());

            var result = packages.FirstOrDefault(
                p => p.Specifier.Identifier.ToothPath == packet.Identifier);//??

            connection.EnqueuePacketToSend(
                CustomOperationPackets.TestPackageInstalledResponse,
                new TestPackageInstalledResponsePacket()
                {
                    Result = result
                });
        });

    public static void InstallPacketHandler(
        Connection.Connection connection,
        CustomOperationPackets type,
        LipInstallPacket packet) => Task.Run(async () =>
        {
            await s_lip!.Install(packet.Packages, packet.Args);
        });

    public static void UninstallPacketHandler(
        Connection.Connection connection,
        CustomOperationPackets type,
        LipUninstallPacket packet) => Task.Run(async () =>
        {
            await s_lip!.Uninstall(packet.Packages, packet.Args);
        });
}
