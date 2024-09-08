using System.Collections.Frozen;

namespace MuseDashModToolsUI.Services;

public sealed class LinuxService : IPlatformService
{
    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    private static readonly FrozenSet<string> LinuxPaths = new[]
        {
            ".local/share/Steam/steamapps/common/Muse Dash",
            ".steam/steam/steamapps/common/Muse Dash"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToFrozenSet();

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    public string OsString => "Linux";

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public bool GetGamePath(out string? folderPath)
    {
        folderPath = LinuxPaths.FirstOrDefault(Directory.Exists);
        if (folderPath is null)
        {
            Logger.Warning("Failed to auto detect game path on Linux");
            return false;
        }

        Logger.Information("Auto detected game path on Linux: {Path}", folderPath);
        return true;
    }

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public string GetUpdaterFilePath(string folderPath) => Path.Combine(folderPath, "Updater");

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public void OpenOrSelectFile(string path) => Process.Start("xdg-open", path);

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public bool SetPathEnvironmentVariable() => false;
}