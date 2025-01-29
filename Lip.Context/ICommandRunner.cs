namespace Lip.Context;

public interface ICommandRunner
{
    Task<int> Run(string command, string workingDirectory);
}
