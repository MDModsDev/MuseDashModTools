namespace MuseDashModToolsUI.Contracts.Downloads;

public interface IDownloadManager
{
    Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default);
    Task<bool> DownloadMelonLoaderAsync(CancellationToken cancellationToken = default);
    Task<bool> DownloadMelonLoaderDependenciesAsync(CancellationToken cancellationToken = default);
}