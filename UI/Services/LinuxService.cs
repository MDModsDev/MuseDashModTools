using System.Collections.Immutable;
using System.Diagnostics;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public class LinuxService : IPlatformService
{
    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    private static readonly ImmutableList<string> LinuxPaths = new List<string>
        {
            ".local/share/Steam/steamapps/common/Muse Dash",
            ".steam/steam/steamapps/common/Muse Dash"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToImmutableList();

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    public string OsString => "Linux";

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public bool GetGamePath(out string? folderPath)
    {
        folderPath = LinuxPaths.FirstOrDefault(Directory.Exists);
        if (folderPath is null) return false;
        Logger.Information("Auto detected game path on Linux {Path}", folderPath);
        return true;
    }

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public string GetUpdaterFilePath(string folderPath) => Path.Combine(folderPath, "Updater");

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public void OpenLogFolder(string logPath) => Process.Start("xdg-open", logPath);

    public Task<bool> VerifyGameVersionAsync() => Task.FromResult(true);
}