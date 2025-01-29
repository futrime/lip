using System.IO.Abstractions;

namespace Lip;

public static class FileSystemExtensions
{
    public static void CreateParentDirectory(this IFileSystem fileSystem, string path)
    {
        string? parentDirPath = fileSystem.Path.GetDirectoryName(path);

        // If path is a root directory, do not need to create parent directory.
        if (parentDirPath is null)
        {
            return;
        }
        if (!fileSystem.Directory.Exists(parentDirPath))
        {
            fileSystem.Directory.CreateDirectory(parentDirPath);
        }
    }
}
