using Flurl;

namespace Lip.Context;

public interface IDownloader
{
    Task DownloadFile(Url url, string destinationPath);
}
