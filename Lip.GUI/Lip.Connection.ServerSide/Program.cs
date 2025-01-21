using System.CommandLine;
using System.Net;
using Lip.Connection;
using Lip.Connection.Network;
using Lip.Connection.Network.Packets.Operations;
using Lip.Connection.Operations;
using Lip.Daemon;
using Microsoft.Extensions.Logging;

var cancelTokenSource = new CancellationTokenSource();
CancellationToken token = cancelTokenSource.Token;

var portOption = new Option<int>("--port", "The port to listen on.");
var passwordOption = new Option<string>("--password", "The password to use.");
var executePathOption = new Option<string>("--execute-path", "The path to execute.");

var startCommand = new Command("start", "Starts the server.")
{
    portOption,
    passwordOption,
    executePathOption,

};

startCommand.SetHandler(async (port, password, path) =>
{
    try
    {
        var connection = new Connection(ConnectionMode.Server, password, IPAddress.Any, port);
        var handler = new PacketHandler<OperationType>(connection);

        handler.SetHandler<OperationPacket>(OperationType.Install, PacketHandlers.OperationsHandler);

        connection.PacketHandler = handler;

        await connection.StartListener(token);
    }
    catch (Exception ex)
    {
        PacketHandlers.logger.LogError(ex, "Failed to start server.");
        return;
    }
}, portOption, passwordOption, executePathOption);



var stopCommand = new Command("stop", "Stops the server.");

stopCommand.SetHandler(cancelTokenSource.Cancel);



var rootCommand = new RootCommand("Lip connection server.")
{
    startCommand,
    stopCommand
};

await rootCommand.InvokeAsync(args);
