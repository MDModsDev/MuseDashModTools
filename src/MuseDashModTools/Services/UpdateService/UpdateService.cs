﻿namespace MuseDashModTools.Services;

public sealed partial class UpdateService : IUpdateService
{
    private const string ReleaseAPIUrl = GitHubAPIBaseUrl + RepoIdentifier + "releases";
    private const string LatestReleaseAPIUrl = GitHubAPIBaseUrl + RepoIdentifier + "releases/latest";
    private const string TagsRSSUrl = GitHubBaseUrl + RepoIdentifier + "tags.atom";

    private static readonly SemVersion _currentVersion = SemVersion.Parse(AppVersion);

    private IGitHubServiceBase CurrentDownloadService => Setting.DownloadSource switch
    {
        DownloadSource.GitHub => GitHubDownloadService,
        DownloadSource.GitHubMirror => GitHubMirrorDownloadService,
        DownloadSource.Custom => GitHubMirrorDownloadService,
        _ => throw new UnreachableException()
    };

    public Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return Setting.UpdateSource switch
        {
            UpdateSource.GitHubAPI => CheckGitHubAPIForUpdatesAsync(cancellationToken),
            UpdateSource.GitHubRSS => CheckGitHubRSSForUpdatesAsync(cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    #region Injections

    [UsedImplicitly]
    public HttpClient Client { get; init; } = null!;

    [UsedImplicitly]
    public MultiThreadDownloader Downloader { get; init; } = null!;

    [UsedImplicitly]
    public IGitHubDownloadService GitHubDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public IGitHubMirrorDownloadService GitHubMirrorDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}