using System.IO.Abstractions;

namespace Lip;

public static class IFileSystemExtensions
{
    /// <summary>
    /// Create parent directory of the specified path if it does not exist.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="path">The path.</param>
    /// <returns>The parent directory of the specified path.</returns>
    public static IDirectoryInfo CreateParentDirectory(this IFileSystem fileSystem, string path)
    {
        string? parentDirPath = fileSystem.Path.GetDirectoryName(path);

        // If path is a root directory, do not need to create parent directory.
        if (parentDirPath is null)
        {
            return fileSystem.DirectoryInfo.New(path);
        }

        if (!fileSystem.Directory.Exists(parentDirPath))
        {
            return fileSystem.Directory.CreateDirectory(parentDirPath);
        }

        return fileSystem.DirectoryInfo.New(parentDirPath);
    }
}
