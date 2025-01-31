using Lip.Connection.Network.Packets.UserInteraction;
using Lip.Context;

namespace Lip.Daemon;

internal class UserInteractionImpl(Connection.Connection connection)
    : IUserInteraction
{
    public async Task<bool> Confirm(string format, params object[] args)
    {
        await connection.SendPacketAsync(UserInteractionPackets.Confirm, new ConfirmPacket()
        {
            Format = format,
            Args = from object arg in args select arg.ToString()
        });
        ConfirmResultPacket packet = await connection.RequestPacketAsync<UserInteractionPackets, ConfirmResultPacket>(UserInteractionPackets.ConfirmResult);
        return packet.Value;
    }

    public async Task<string?> PromptForInput(string format, params object[] args)
    {
        await connection.SendPacketAsync(UserInteractionPackets.PromptForInput, new PromptForInputPacket()
        {
            Format = format,
            Args = from object arg in args select arg.ToString()
        });
        PromptForInputResultPacket packet = await connection.RequestPacketAsync<UserInteractionPackets, PromptForInputResultPacket>(UserInteractionPackets.PromptForInputResult);
        return packet.Value;
    }

    public async Task<string> PromptForSelection(IEnumerable<string> options, string format, params object[] args)
    {
        await connection.SendPacketAsync(UserInteractionPackets.PromptForSelection, new PromptForSelectionPacket()
        {
            Format = format,
            Args = from object arg in args select arg.ToString(),
            Options = options
        });
        PromptForSelectionResultPacket packet = await connection.RequestPacketAsync<UserInteractionPackets, PromptForSelectionResultPacket>(UserInteractionPackets.PromptForSelectionResult);
        return packet.Value;
    }
    public async Task UpdateProgress(string id, float progress, string format, params object[] args)
    {
        await connection.SendPacketAsync(UserInteractionPackets.UpdateProgress, new UpdateProgressPacket()
        {
            Id = id,
            Progress = progress,
            Format = format,
            Args = from object arg in args select arg.ToString()
        });
    }
}
