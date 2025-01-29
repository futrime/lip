using System.IO.Abstractions;

namespace Lip;

public static class IFileSystemExtensions
{
    /// <summary>
    /// Create parent directory of the specified path if it does not exist.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="path">The path.</param>
    public static async Task CreateParentDirectoryAsync(this IFileSystem fileSystem, string path)
    {
        string? parentDirPath = fileSystem.Path.GetDirectoryName(path);

        // If path is a root directory, do not need to create parent directory.
        if (parentDirPath is null)
        {
            return;
        }
        if (!await fileSystem.Directory.ExistsAsync(parentDirPath))
        {
            await fileSystem.Directory.CreateDirectoryAsync(parentDirPath);
        }
    }

    /// <inheritdoc cref="IDirectory.CreateDirectory(string)" />
    public static async Task<IDirectoryInfo> CreateDirectoryAsync(this IDirectory dir, string path)
    {
        return await Task.Run(() => dir.CreateDirectory(path));
    }

    /// <inheritdoc cref="IDirectory.Delete(string, bool)" />
    public static async Task DeleteAsync(this IDirectory dir, string path, bool recursive)
    {
        await Task.Run(() => dir.Delete(path, recursive));
    }

    /// <inheritdoc cref="IDirectory.Exists(string)" />
    public static async Task<bool> ExistsAsync(this IDirectory dir, string path)
    {
        return await Task.Run(() => dir.Exists(path));
    }

    /// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories()" />
    public static async Task<IEnumerable<IDirectoryInfo>> EnumerateDirectoriesAsync(this IDirectoryInfo dirInfo)
    {
        return await Task.Run(() => dirInfo.EnumerateDirectories());
    }

    /// <inheritdoc cref="IDirectoryInfo.EnumerateFiles()" />
    public static async Task<IEnumerable<IFileInfo>> EnumerateFilesAsync(this IDirectoryInfo dirInfo)
    {
        return await Task.Run(() => dirInfo.EnumerateFiles());
    }

    /// <inheritdoc cref="IFile.Copy(string, string)" />
    public static async Task CopyAsync(this IFile file, string srcPath, string destPath)
    {
        await Task.Run(() => file.Copy(srcPath, destPath));
    }

    /// <inheritdoc cref="IFile.Create(string)" />
    public static async Task<FileSystemStream> CreateAsync(this IFile file, string path)
    {
        return await Task.Run(() => file.Create(path));
    }

    /// <inheritdoc cref="IFile.Exists(string)" />
    public static async Task<bool> ExistsAsync(this IFile file, string path)
    {
        return await Task.Run(() => file.Exists(path));
    }

    /// <inheritdoc cref="IFile.OpenRead(string)" />
    public static async Task<FileSystemStream> OpenReadAsync(this IFile file, string path)
    {
        return await Task.Run(() => file.OpenRead(path));
    }

    /// <inheritdoc cref="IPath.Exists(string)" />
    public static async Task<bool> ExistsAsync(this IPath path, string pathValue)
    {
        return await Task.Run(() => path.Exists(pathValue));
    }
}
