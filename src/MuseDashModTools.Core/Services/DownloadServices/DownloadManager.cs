namespace MuseDashModTools.Core;

internal sealed class DownloadManager : IDownloadManager
{
    private IDownloadService CurrentDownloadService => Config.DownloadSource switch
    {
        DownloadSource.GitHub => GitHubDownloadService,
        DownloadSource.GitHubMirror => GitHubMirrorDownloadService,
        DownloadSource.Gitee => GiteeDownloadService,
        DownloadSource.Custom => CustomDownloadService,
        _ => throw new UnreachableException()
    };

    public async Task<bool> DownloadFileAsync(
        string url,
        string filePath,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        EventHandler<DownloadProgressChangedEventArgs>? progressHandler = null;

        if (onDownloadStarted is not null)
        {
            Downloader.DownloadStarted += onDownloadStarted;
        }

        if (downloadProgress is not null)
        {
            progressHandler = (_, e) => downloadProgress.Report(e.ProgressPercentage);
            Downloader.DownloadProgressChanged += progressHandler;
        }

        try
        {
            await Downloader.DownloadFileTaskAsync(url, filePath, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download file from {url} to {filePath}");
            return false;
        }
        finally
        {
            if (onDownloadStarted is not null)
            {
                Downloader.DownloadStarted -= onDownloadStarted;
            }

            if (progressHandler is not null)
            {
                Downloader.DownloadProgressChanged -= progressHandler;
            }
        }
    }

    public Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        return Config.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            // For Gitee and Custom Download Source, because they don't choose GitHub for other downloads, so we will use GitHubMirror
            DownloadSource.Gitee => GitHubMirrorDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            DownloadSource.Custom => GitHubMirrorDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default) =>
        CurrentDownloadService.DownloadModAsync(mod, cancellationToken);

    public Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default) =>
        CurrentDownloadService.DownloadLibAsync(lib, cancellationToken);

    public Task DownloadReleaseByTagAsync(string tag, string osString, CancellationToken cancellationToken = default)
    {
        return Config.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadReleaseByTagAsync(tag, osString, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadReleaseByTagAsync(tag, osString, cancellationToken),
            DownloadSource.Gitee => GiteeDownloadService.DownloadReleaseByTagAsync(tag, osString, cancellationToken),
            // For Custom Download Source, because they don't choose GitHub or GitHub Mirror for other downloads, so we will use Gitee
            DownloadSource.Custom => GiteeDownloadService.DownloadReleaseByTagAsync(tag, osString, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default)
    {
        return Config.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            // For Gitee and Custom Download Source, because they don't choose GitHub for other downloads, so we will use GitHubMirror
            DownloadSource.Gitee => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            DownloadSource.Custom => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default) =>
        CurrentDownloadService.GetModListAsync(cancellationToken);

    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default) =>
        CurrentDownloadService.GetLibListAsync(cancellationToken);

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required MultiThreadDownloader Downloader { get; init; }

    [UsedImplicitly]
    public required ICustomDownloadService CustomDownloadService { get; init; }

    [UsedImplicitly]
    public required IGiteeDownloadService GiteeDownloadService { get; init; }

    [UsedImplicitly]
    public required IGitHubDownloadService GitHubDownloadService { get; init; }

    [UsedImplicitly]
    public required IGitHubMirrorDownloadService GitHubMirrorDownloadService { get; init; }

    [UsedImplicitly]
    public required ILogger<DownloadManager> Logger { get; init; }

    #endregion Injections
}