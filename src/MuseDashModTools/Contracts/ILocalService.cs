namespace MuseDashModTools.Contracts;

public interface ILocalService
{
    Task CheckDotNetRuntimeInstallAsync();
    Task<string> GetMuseDashFolderAsync();
    IEnumerable<string> GetModFilePaths();
    HashSet<string?> GetLibFileNames();
    Task<bool> InstallMelonLoaderAsync();
    Task<bool> UninstallMelonLoaderAsync();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    void LaunchGame(bool isModded);
    bool ExtractZipFile(string zipPath, string extractPath);
}