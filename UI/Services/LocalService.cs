﻿using System.Diagnostics;
using System.Reflection;
using AssetsTools.NET.Extra;
using DialogHostAvalonia;
using MelonLoader;

namespace MuseDashModToolsUI.Services;

#pragma warning disable CS8618

public partial class LocalService : ILocalService
{
    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public Lazy<IDownloadWindowViewModel> DownloadWindowViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ISavingService> SavingService { get; init; }

    private bool IsValidPath { get; set; }

    public async Task CheckMelonLoaderInstallAsync()
    {
        var melonLoaderFolder = Path.Join(SavingService.Value.Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(SavingService.Value.Settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile)) return;
        var install = await MessageBoxService.WarningConfirmMessageBox(MsgBox_Content_InstallMelonLoader);
        if (install)
            await OnInstallMelonLoaderAsync();
    }

    public async Task CheckValidPathAsync()
    {
        Logger.Information("Checking valid path...");
        await CheckGameFileExist();

        try
        {
            if (!await PlatformService.VerifyGameVersionAsync()) return;
            await CreateFiles();
            IsValidPath = true;
            Logger.Information("Path verified {Path}", SavingService.Value.Settings.MuseDashFolder);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exe verify failed, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ExeVerifyFailed);
        }
    }

    public IEnumerable<string> GetModFiles(string path) => Directory.GetFiles(path)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll");

    public IEnumerable<string> GetBmsFiles(string path) => Directory.GetFiles(path, "*.bms");

    public async Task<bool> LaunchUpdaterAsync(string link)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var updaterTargetFolder = Path.Combine(currentDirectory, "Update");
        var updaterFilePath = PlatformService.GetUpdaterFilePath(currentDirectory);
        var updaterTargetPath = PlatformService.GetUpdaterFilePath(updaterTargetFolder);

        if (!await CheckUpdaterFilesExist(updaterFilePath, updaterTargetFolder)) return false;

        try
        {
            File.Copy(updaterFilePath, updaterTargetPath, true);
            Logger.Information("Copy Updater to Update folder success");
        }
        catch (Exception ex)
        {
            Logger.Information(ex, "Copy Updater to Update folder failed");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_CopyUpdaterFailed, ex);
        }

        Process.Start(updaterTargetPath, new[] { link, currentDirectory });
        return true;
    }

    public Mod? LoadMod(string filePath)
    {
        var mod = new Mod
        {
            IsDisabled = filePath.EndsWith(".disabled")
        };

        mod.FileName = mod.IsDisabled ? Path.GetFileName(filePath)[..^9] : Path.GetFileName(filePath);
        var assembly = Assembly.Load(File.ReadAllBytes(filePath));
        var attribute = MelonUtils.PullAttributeFromAssembly<MelonInfoAttribute>(assembly);
        if (attribute is null) return null;

        mod.Name = attribute.Name;
        mod.LocalVersion = attribute.Version;

        if (mod.Name == null || mod.LocalVersion == null) return null;

        mod.Author = attribute.Author;
        mod.HomePage = attribute.DownloadLink;
        mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(filePath);
        Logger.Information("Local mod {Name} loaded. File name {FileName}", mod.Name, mod.FileName);
        return mod;
    }

    public async Task OnInstallMelonLoaderAsync()
    {
        if (!IsValidPath) return;
        Logger.Information("Showing MelonLoader download window...");
        await DialogHost.Show(DownloadWindowViewModel.Value, "DownloadWindowDialog",
            (object _, DialogOpenedEventArgs _) => DownloadWindowViewModel.Value.InstallMelonLoader());
    }

    public async Task OnUninstallMelonLoaderAsync()
    {
        if (!IsValidPath) return;
        if (!await MessageBoxService.WarningConfirmMessageBox(MsgBox_Content_UninstallMelonLoader)) return;
        var versionFile = Path.Join(SavingService.Value.Settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(SavingService.Value.Settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(SavingService.Value.Settings.MelonLoaderFolder))
        {
            try
            {
                Directory.Delete(SavingService.Value.Settings.MelonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                Logger.Information("MelonLoader uninstalled successfully");
                await MessageBoxService.SuccessMessageBox(MsgBox_Content_UninstallMelonLoaderSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "MelonLoader uninstall failed, showing error message box...");
                await MessageBoxService.ErrorMessageBox(MsgBox_Content_UninstallMelonLoaderFailed);
            }
        }
        else
        {
            Logger.Error("MelonLoader folder not found, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoMelonLoaderFolder);
        }
    }

    public void OnLaunchGame(bool isModded)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "steam://rungameid/774171" + (isModded ? string.Empty : "//--no-mods"),
            UseShellExecute = true
        });
    }

    public async Task OpenCustomAlbumsFolderAsync()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            return;
        }

        Logger.Information("Opening Custom Albums folder...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SavingService.Value.Settings.CustomAlbumsFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenModsFolderAsync()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            return;
        }

        Logger.Information("Opening mods folder...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SavingService.Value.Settings.ModsFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenUserDataFolderAsync()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            return;
        }

        Logger.Information("Opening UserData folder...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SavingService.Value.Settings.UserDataFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenLogFolderAsync()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            return;
        }

        var logPath = Path.Combine(SavingService.Value.Settings.MelonLoaderFolder, "Latest.log");
        Logger.Information("Opening Log folder...");
        PlatformService.OpenLogFolder(logPath);
    }

    public async Task<string> ReadGameVersionAsync()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(SavingService.Value.Settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadIncludedClassPackage();
            if (!instance.file.Metadata.TypeTreeEnabled)
                assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];

            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)?.Get("bundleVersion").AsString!;
            Logger.Information("Game version read successfully: {BundleVersion}", bundleVersion);
            return bundleVersion;
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Read game version failed, showing error message box...");
            await MessageBoxService.FormatErrorMessageBox(MsgBox_Content_ReadGameVersionFailed, bundlePath);
            Environment.Exit(0);
        }

        return string.Empty;
    }
}