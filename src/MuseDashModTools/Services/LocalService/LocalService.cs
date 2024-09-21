using System.IO.Compression;
using System.Text;
using AsmResolver.DotNet;
using CliWrap;

namespace MuseDashModTools.Services;

public sealed partial class LocalService : ILocalService
{
    public async Task CheckDotNetRuntimeInstallAsync()
    {
        var outputStringBuilder = new StringBuilder();
        await Cli.Wrap("dotnet")
            .WithArguments("--list-runtimes")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputStringBuilder))
            .ExecuteAsync()
            .ConfigureAwait(false);

        if (!outputStringBuilder.ToString().Contains("Microsoft.WindowsDesktop.App 6."))
        {
            Logger.Information("DotNet Runtime not found, showing error message box...");
            await ErrorMessageBoxAsync(MsgBox_Content_DotNetRuntimeNotFound).ConfigureAwait(true);
        }
    }

    public IEnumerable<string> GetModFilePaths() => Directory.GetFiles(Setting.ModsFolder)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll");

    public HashSet<string?> GetLibFileNames() => Directory.GetFiles(Setting.UserLibsFolder).Select(Path.GetFileNameWithoutExtension).ToHashSet();

    public async Task<bool> InstallMelonLoaderAsync()
    {
        if (!FileSystemService.CheckFileExists(Setting.MelonLoaderZipPath))
        {
            await ErrorMessageBoxAsync("MelonLoader zip file not found").ConfigureAwait(true);
            return false;
        }

        if (!ExtractZipFile(Setting.MelonLoaderZipPath, Setting.MuseDashFolder))
        {
            await ErrorMessageBoxAsync("Failed to unzip MelonLoader").ConfigureAwait(true);
            return false;
        }

        if (!FileSystemService.TryDeleteFile(Setting.MelonLoaderZipPath))
        {
            await ErrorMessageBoxAsync("Failed to delete MelonLoader zip file").ConfigureAwait(true);
            return false;
        }

        Logger.Information("MelonLoader installed successfully");
        await SuccessMessageBoxAsync("MelonLoader installed successfully").ConfigureAwait(true);
        return true;
    }

    public async Task<bool> UninstallMelonLoaderAsync()
    {
        var dobbyPath = Path.Combine(Setting.MuseDashFolder, "dobby.dll");
        var noticePath = Path.Combine(Setting.MuseDashFolder, "NOTICE.txt");
        var versionPath = Path.Combine(Setting.MuseDashFolder, "version.dll");

        foreach (var path in new[] { dobbyPath, noticePath, versionPath })
        {
            if (FileSystemService.TryDeleteFile(path, DeleteOption.IgnoreIfNotFound))
            {
                continue;
            }

            await ErrorMessageBoxAsync($"Failed to delete {Path.GetFileName(path)}").ConfigureAwait(true);
            return false;
        }

        if (!FileSystemService.TryDeleteDirectory(Setting.MelonLoaderFolder, DeleteOption.IgnoreIfNotFound))
        {
            await ErrorMessageBoxAsync("Failed to delete MelonLoader folder").ConfigureAwait(true);
            return false;
        }

        Logger.Information("MelonLoader uninstalled successfully");
        await SuccessMessageBoxAsync("MelonLoader uninstalled successfully").ConfigureAwait(true);
        return true;
    }

    public async Task<string> GetMuseDashFolderAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await CheckValidPathAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder).ConfigureAwait(true);
            Logger.Information("Selected MuseDash folder: {MuseDashFolder}", path);
        }

        return path;
    }

    public async Task<ModDto?> LoadModFromPathAsync(string filePath)
    {
        var mod = new ModDto
        {
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            FileExtension = Path.GetExtension(filePath)
        };

        var module = ModuleDefinition.FromFile(filePath);
        if (module.Assembly is null)
        {
            Logger.Error("Invalid mod file: {FilePath}", filePath);
            return null;
        }

        var attribute = module.Assembly.FindCustomAttributes("MelonLoader", "MelonInfoAttribute").SingleOrDefault();
        if (attribute is null)
        {
            Logger.Warning("{FilePath} is not a mod file but inside Mods folder", filePath);
            return null;
        }

        mod.Name = attribute.Signature!.FixedArguments[1].ToString();
        mod.Version = attribute.Signature!.FixedArguments[2].ToString();
        mod.Author = attribute.Signature!.FixedArguments[3].ToString();
        mod.SHA256 = await HashUtils.ComputeSHA256HashFromPathAsync(filePath).ConfigureAwait(false);

        return mod;
    }

    public void LaunchGame(bool isModded)
    {
        var launchArguments = new StringBuilder();
        if (!isModded)
        {
            launchArguments.Append("//--no-mods");
        }
        else if (!Setting.ShowConsole)
        {
            launchArguments.Append("//--melonloader.hideconsole");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "steam://rungameid/774171" + launchArguments,
            UseShellExecute = true
        });

        Logger.Information("Launching game with launch arguments: {LaunchArguments}", launchArguments);
    }

    public bool ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to unzip file {ZipPath} to {ExtractPath}", zipPath, extractPath);
            return false;
        }
    }

    #region Injections

    [UsedImplicitly]
    public IFileSystemService FileSystemService { get; init; } = null!;

    [UsedImplicitly]
    public IFileSystemPickerService FileSystemPickerService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}