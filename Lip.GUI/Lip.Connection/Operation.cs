using static Lip.Connection.Operation;

namespace Lip.Connection;

public sealed class Operation(OperationType type, OperationAction action)
{
    public delegate ValueTask OperationAction(ConnectionMode mode, OperationType type, string message);

    public enum OperationType { Connect, Disconnect }

    public OperationType Type { get; init; } = type;

    internal OperationAction Action { get; init; } = action;
}

public static class Operations
{

}
