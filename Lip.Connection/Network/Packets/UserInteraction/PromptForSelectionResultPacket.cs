using System.Text;

namespace Lip.Connection.Network.Packets.UserInteraction;

public class PromptForSelectionResultPacket : IPacket<PromptForSelectionResultPacket>
{
    public required string Value { get; set; }

    public static PromptForSelectionResultPacket Deserialize(byte[] data) => new() { Value = Encoding.UTF8.GetString(data) };

    public byte[] Serialize() => Encoding.UTF8.GetBytes(Value);
}
