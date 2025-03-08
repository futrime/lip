using Downloader;
using Flurl;
using Lip.Core;

namespace Lip.Context;

public class Downloader(IUserInteraction userInteraction) : IDownloader
{
    private readonly IUserInteraction _userInteraction = userInteraction;

    public async Task DownloadFile(Url url, string destinationPath)
    {
        await using IDownload downloader = DownloadBuilder.New()
            .WithUrl(url)
            .WithFileLocation(destinationPath)
            .Build();

        downloader.DownloadProgressChanged += async (sender, e) =>
        {
            string urlString = url.ToString();
            if (urlString.Length > 50)
            {
                urlString = urlString[..15] + " ... " + urlString[^35..];
            }

            await _userInteraction.UpdateProgress(
                url,
                Convert.ToSingle(e.ProgressPercentage / 100),
                "Downloading {0} ({1:0.00} MB/s)",
                urlString,
                e.BytesPerSecondSpeed / 1024 / 1024
            );
        };

        await downloader.StartAsync();

        if (downloader.Status == DownloadStatus.Failed)
        {
            throw new InvalidOperationException($"Download failed for URL {url}");
        }
    }
}
