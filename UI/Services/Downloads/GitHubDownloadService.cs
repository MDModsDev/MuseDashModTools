using System.Net.Http.Json;

namespace MuseDashModToolsUI.Services.Downloads;

public sealed class GitHubDownloadService : GitHubServiceBase, IGitHubDownloadService
{
    private const string RawGitHubUrl = "https://raw.githubusercontent.com/";
    private const string ReleaseGitHubUrl = "https://github.com/";
    private const string ModLinksUrl = RawGitHubUrl + ModLinksBaseUrl + "ModLinks.json";
    private const string MelonLoaderUrl = ReleaseGitHubUrl + MelonLoaderBaseUrl;
    private const string UnityDependencyUrl = RawGitHubUrl + UnityDependencyBaseUrl;
    private const string Cpp2ILUrl = ReleaseGitHubUrl + Cpp2ILBaseUrl;


    public async Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Fetching mods from GitHub {Url}...", ModLinksUrl);

        try
        {
            var mods = await Client.GetFromJsonAsync<Mod[]>(ModLinksUrl, cancellationToken).ConfigureAwait(false);
            Logger.Information("Mods fetched from GitHub successfully");
            return mods;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch mods from GitHub");
            return null;
        }
    }

    public async Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Downloading MelonLoader and Dependencies from GitHub...");

        Downloader.DownloadStarted += onDownloadStarted;
        Downloader.DownloadProgressChanged += (_, e) => downloadProgress.Report(e.ProgressPercentage);

        try
        {
            await Downloader.DownloadFileTaskAsync(MelonLoaderUrl, Setting.MelonLoaderZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(UnityDependencyUrl, Setting.UnityDependencyZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(Cpp2ILUrl, Setting.Cpp2ILZipPath, cancellationToken).ConfigureAwait(false);
            Logger.Information("MelonLoader and Dependencies downloaded from GitHub successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to download MelonLoader from GitHub");
            return false;
        }
    }

    #region Injections

    [UsedImplicitly]
    public HttpClient Client { get; init; } = null!;

    [UsedImplicitly]
    public MultiThreadDownloader Downloader { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}