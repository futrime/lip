using System.Collections.Concurrent;

namespace Lip.Connection.Operations;

public enum OperationType
{
    Init,
    Install
}

public sealed partial class Operation
{
    private static readonly ConcurrentDictionary<OperationType, Operation> s_operations = [];

    public static IReadOnlyDictionary<OperationType, Operation> Operations => s_operations;

    private Operation(OperationType type, OperationAction action)
    {
        Type = type;
        Action = action;

        s_operations[type] = this;
    }

    private delegate ValueTask OperationAction(ConnectionMode mode, OperationType type, string message);

    public OperationType Type { get; init; }

    private OperationAction Action { get; init; }

    public static Operation Init { get; } = new Operation(OperationType.Init, InitAction);

    public static Operation Install { get; } = new Operation(OperationType.Install, InstallAction);

    public ValueTask Execute(ConnectionMode mode, string message) => Action(mode, Type, message);
}
