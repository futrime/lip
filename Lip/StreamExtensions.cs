namespace Lip;

public static class StreamExtensions
{
    public static async Task<byte[]> ReadAsync(this Stream stream)
    {
        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}
