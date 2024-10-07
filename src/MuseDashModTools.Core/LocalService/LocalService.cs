using System.IO.Compression;
using System.Text;
using AsmResolver.DotNet;
using AssetsTools.NET.Extra;
using CliWrap;

namespace MuseDashModTools.Core;

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
            await MessageBoxService.ErrorMessageBoxAsync(MsgBox_Content_DotNetRuntimeNotFound).ConfigureAwait(true);
        }
    }

    public IEnumerable<string> GetModFilePaths() => Directory.GetFiles(Setting.ModsFolder)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll");

    public HashSet<string?> GetLibFileNames() => Directory.GetFiles(Setting.UserLibsFolder).Select(Path.GetFileNameWithoutExtension).ToHashSet();

    public async Task<bool> InstallMelonLoaderAsync()
    {
        if (!FileSystemService.CheckFileExists(Setting.MelonLoaderZipPath))
        {
            await MessageBoxService.ErrorMessageBoxAsync("MelonLoader zip file not found").ConfigureAwait(true);
            return false;
        }

        if (!ExtractZipFile(Setting.MelonLoaderZipPath, Setting.MuseDashFolder))
        {
            await MessageBoxService.ErrorMessageBoxAsync("Failed to unzip MelonLoader").ConfigureAwait(true);
            return false;
        }

        if (!FileSystemService.TryDeleteFile(Setting.MelonLoaderZipPath))
        {
            await MessageBoxService.ErrorMessageBoxAsync("Failed to delete MelonLoader zip file").ConfigureAwait(true);
            return false;
        }

        Logger.Information("MelonLoader installed successfully");
        await MessageBoxService.SuccessMessageBoxAsync("MelonLoader installed successfully").ConfigureAwait(true);
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

            await MessageBoxService.ErrorMessageBoxAsync($"Failed to delete {Path.GetFileName(path)}").ConfigureAwait(true);
            return false;
        }

        if (!FileSystemService.TryDeleteDirectory(Setting.MelonLoaderFolder, DeleteOption.IgnoreIfNotFound))
        {
            await MessageBoxService.ErrorMessageBoxAsync("Failed to delete MelonLoader folder").ConfigureAwait(true);
            return false;
        }

        Logger.Information("MelonLoader uninstalled successfully");
        await MessageBoxService.SuccessMessageBoxAsync("MelonLoader uninstalled successfully").ConfigureAwait(true);
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

    public ModDto? LoadModFromPath(string filePath)
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
        mod.LocalVersion = attribute.Signature!.FixedArguments[2].ToString();
        mod.Author = attribute.Signature!.FixedArguments[3].ToString();
        mod.SHA256 = HashUtils.ComputeSHA256HashFromPath(filePath);

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

    public async ValueTask<string> ReadGameVersionAsync()
    {
        var assetsManager = new AssetsManager();
        assetsManager.LoadClassPackage(ResourceService.GetAssetAsStream("classdata.tpk"));
        var bundlePath = Path.Combine(Setting.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];
            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)["bundleVersion"].AsString;

            Logger.Information("Game version read successfully: {BundleVersion}", bundleVersion);
            assetsManager.UnloadAll();
            return bundleVersion;
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Read game version failed, showing error message box...");
            await MessageBoxService.FormatErrorMessageBoxAsync("Reading Game Version failed", bundlePath).ConfigureAwait(true);
            Environment.Exit(0);
        }

        return string.Empty;
    }

    public bool ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            Logger.Information("Successfully extracted {ZipPath} to {ExtractPath}", zipPath, extractPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to extract file {ZipPath} to {ExtractPath}", zipPath, extractPath);
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
    public IMessageBoxService MessageBoxService { get; init; } = null!;

    [UsedImplicitly]
    public IResourceService ResourceService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}