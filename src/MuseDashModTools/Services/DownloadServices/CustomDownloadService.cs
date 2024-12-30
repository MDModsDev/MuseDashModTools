namespace MuseDashModTools.Services;

public sealed class CustomDownloadService : ICustomDownloadService
{
    public Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task DownloadReleaseByTagAsync(string tag, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    #region Injections

    [UsedImplicitly]
    public ILogger<CustomDownloadService> Logger { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}