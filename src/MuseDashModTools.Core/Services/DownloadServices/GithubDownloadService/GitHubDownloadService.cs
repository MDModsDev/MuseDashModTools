using System.Net.Http.Json;

namespace MuseDashModTools.Core;

internal sealed partial class GitHubDownloadService : IGitHubDownloadService
{
    private const string RawModLinksUrl = GitHubRawContentBaseUrl + ModLinksBaseUrl;
    private const string ModJsonUrl = RawModLinksUrl + "Mods.json";
    private const string LibJsonUrl = RawModLinksUrl + "Libs.json";
    private const string ModsFolderUrl = RawModLinksUrl + "Mods/";
    private const string LibsFolderUrl = RawModLinksUrl + "Libs/";
    private const string MelonLoaderUrl = GitHubBaseUrl + MelonLoaderBaseUrl;
    private const string UnityDependencyUrl = GitHubRawContentBaseUrl + UnityDependencyBaseUrl;
    private const string Cpp2ILUrl = GitHubBaseUrl + Cpp2ILBaseUrl;

    public async Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading MelonLoader and Dependencies from GitHub...");

        Downloader.DownloadStarted += onDownloadStarted;
        Downloader.DownloadProgressChanged += ReportProgress;

        try
        {
            await Downloader.DownloadFileTaskAsync(MelonLoaderUrl, Config.MelonLoaderZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(UnityDependencyUrl, Config.UnityDependencyZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(Cpp2ILUrl, Config.Cpp2ILZipPath, cancellationToken).ConfigureAwait(false);
            Logger.ZLogInformation($"MelonLoader and Dependencies downloaded from GitHub successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download MelonLoader from GitHub");
            return false;
        }
        finally
        {
            Downloader.DownloadStarted -= onDownloadStarted;
            Downloader.DownloadProgressChanged -= ReportProgress;
        }

        void ReportProgress(object? sender, DownloadProgressChangedEventArgs args) => downloadProgress.Report(args.ProgressPercentage);
    }

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading mod {mod.Name} from GitHub...");

        if (mod.FileName.IsNullOrEmpty())
        {
            Logger.ZLogError($"Mod {mod.Name} does not have file name");
            return false;
        }

        var downloadLink = ModsFolderUrl + mod.FileName;
        var path = Path.Combine(Config.ModsFolder, mod.FileName);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            var fs = new FileStream(path, FileMode.OpenOrCreate);
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download mod {mod.Name} from GitHub");
            return false;
        }
    }

    public async Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading lib {lib.Name} from GitHub...");

        var downloadLink = LibsFolderUrl + lib.FileName;
        var path = Path.Combine(Config.UserLibsFolder, lib.FileName);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            var fs = new FileStream(path, FileMode.OpenOrCreate);
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download lib {lib.Name} from GitHub");
            return false;
        }
    }

    public async Task DownloadReleaseByTagAsync(string tag, string osString, CancellationToken cancellationToken = default)
    {
        var downloadUrl = $"{ModToolsReleaseDownloadBaseUrl}{tag}/MuseDashModTools-{osString}.zip";

        try
        {
            await Downloader.DownloadFileTaskAsync(downloadUrl,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MuseDashModTools.zip"),
                cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download new version from GitHub");
        }
    }

    public async Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default)
    {
        if (ReadmeCache.TryGetValue(repoId, out var readme))
        {
            Logger.ZLogInformation($"Using cached Readme for {repoId}");
            return readme;
        }

        Logger.ZLogInformation($"Attempting to fetch Readme for {repoId}");
        readme = await FetchReadmeFromBranchesAsync(repoId, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(readme))
        {
            ReadmeCache[repoId] = readme;
            return readme;
        }

        Logger.ZLogInformation($"Branch readme fetch failed");
        return null;
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching mods from GitHub {ModJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Mod>(ModJsonUrl, Default.Mod, cancellationToken)
            .Catch<Mod?, Exception>(ex =>
            {
                Logger.ZLogError(ex, $"Failed to fetch mods from GitHub");
                return AsyncEnumerable.Empty<Mod?>();
            });
    }

    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching libs from GitHub {LibJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Lib>(LibJsonUrl, Default.Lib, cancellationToken)
            .Catch<Lib?, Exception>(ex =>
            {
                Logger.ZLogError(ex, $"Failed to fetch libs from GitHub");
                return AsyncEnumerable.Empty<Lib?>();
            });
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required MultiThreadDownloader Downloader { get; init; }

    [UsedImplicitly]
    public required ILogger<GitHubDownloadService> Logger { get; init; }

    #endregion Injections
}