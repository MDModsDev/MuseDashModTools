using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using DialogHostAvalonia;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Services;

public partial class GitHubService
{
    private async Task<List<Mod>?> GetModListFromSourceAsync(string downloadSource)
    {
        var url = downloadSource + "ModLinks.json";
        try
        {
            var mods = await Client.GetFromJsonAsync<List<Mod>>(url);
            Logger.Information("Get mod list from {Url} success", url);
            return mods;
        }
        catch
        {
            Logger.Warning("Get mod list from {Url} failed", url);
            return null;
        }
    }

    private async Task<HttpResponseMessage?> DownloadModFromSourceAsync(string downloadSource, string link, string path)
    {
        var url = downloadSource + link;
        try
        {
            var result = await Client.GetAsync(url);
            Logger.Information("Download mod from {Url} success", url);
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await result.Content.CopyToAsync(fs);
            return result;
        }
        catch
        {
            Logger.Warning("Download mod from {Url} failed", url);
            return null;
        }
    }

    private async Task<bool> DownloadMelonLoaderFromSourceAsync(IProgress<double> downloadProgress)
    {
        try
        {
            var totalLength = _melonLoaderResponseMessage!.Content.Headers.ContentLength;
            var contentStream = await _melonLoaderResponseMessage.Content.ReadAsStreamAsync();
            await using var fs = new FileStream(SavingService.Settings.MelonLoaderZipPath, FileMode.OpenOrCreate);
            var buffer = new byte[5 * 1024];
            var readLength = 0L;
            int length;
            while ((length = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None)) != 0)
            {
                readLength += length;
                if (totalLength > 0) downloadProgress.Report(Math.Round((double)readLength / totalLength.Value * 100, 2));

                fs.Write(buffer, 0, length);
            }

            Logger.Information("Download MelonLoader.zip success");
            return true;
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException)
            {
                Logger.Error(ex, "Download MelonLoader.zip failed");
                await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_InstallMelonLoaderFailed_Internet, ex);
                DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                return false;
            }

            Logger.Error(ex, "Download MelonLoader.zip failed");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_InstallMelonLoaderFailed, ex);
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return false;
        }
    }

    private async Task GetMelonLoaderResponseMessage()
    {
        await GetMelonLoaderResponseFromSource(DefaultDownloadSource);
        if (_melonLoaderResponseMessage is not null) return;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SavingService.Settings.DownloadSource))
        {
            await GetMelonLoaderResponseFromSource(pair.Value);
            if (_melonLoaderResponseMessage is not null) return;
        }
    }

    private async Task GetMelonLoaderResponseFromSource(string downloadSource)
    {
        var url = downloadSource + "MelonLoader.zip";
        try
        {
            _melonLoaderResponseMessage = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            Logger.Information("Get MelonLoader Download ResponseHeader from {Url} success", url);
        }
        catch
        {
            _melonLoaderResponseMessage = null;
            Logger.Warning("Get MelonLoader Download ResponseHeader from {Url} failed", url);
        }
    }

    private async Task LaunchUpdater(string link)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var updaterTargetFolder = Path.Combine(currentDirectory, "Update");
        var updaterExePath = GetUpdaterFilePath(currentDirectory);
        var updaterTargetPath = GetUpdaterFilePath(updaterTargetFolder);

        if (!await CheckUpdaterFilesExist(updaterExePath, updaterTargetFolder)) return;

        try
        {
            File.Copy(updaterExePath, updaterTargetPath, true);
            Logger.Information("Copy Updater to Update folder success");
        }
        catch (Exception ex)
        {
            Logger.Information(ex, "Copy Updater to Update folder failed");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_CopyUpdaterFailed, ex);
        }

        Process.Start(updaterTargetPath, new[] { link, currentDirectory });
    }

    private static string GetUpdaterFilePath(string folder)
    {
        if (OperatingSystem.IsWindows()) return Path.Combine(folder, "Updater.exe");
        if (OperatingSystem.IsLinux()) return Path.Combine(folder, "Updater");

        return Path.Combine(folder, "Updater.exe");
    }

    private async Task<bool> CheckUpdaterFilesExist(string updaterExePath, string updaterTargetFolder)
    {
        if (!File.Exists(updaterExePath))
        {
            Logger.Error("Updater not found");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_UpdaterNotFound);
            return false;
        }

        if (!Directory.Exists(updaterTargetFolder))
        {
            Directory.CreateDirectory(updaterTargetFolder);
            Logger.Information("Create Update target folder success");
        }

        return true;
    }

    #region CheckUpdates Private Methods

    private SemanticVersion? GetVersionFromTag(string tagName)
    {
        var tag = VersionRegex().Match(tagName);
        if (!tag.Success) return null;
        if (!SemanticVersion.TryParse(tag.Groups[1].Value, out var version)) return null;
        Logger.Information("Get latest version success: {Version}", version);
        return version;
    }

    private async Task<bool> SkipVersionCheck(SemanticVersion version, string currentVersion, bool userClick)
    {
        var current = SemanticVersion.Parse(currentVersion)!;
        if (!userClick) return version == SavingService.Settings.SkipVersion || version <= current;
        if (version > current) return false;
        await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_LatestVersion);
        return true;
    }

    private async Task<bool> UpdateRequired(string version, string title, string body)
    {
        var update = await MessageBoxService.CreateCustomMarkDownConfirmMessageBox(
            string.Format(MsgBox_Content_NewerVersion, version, title, body), 3);

        if (update == MsgBox_Button_NoNoAsk)
        {
            SavingService.Settings.SkipVersion = SemanticVersion.Parse(version);
            return false;
        }

        return update != MsgBox_Button_No && !string.IsNullOrEmpty(update);
    }

    private string GetDownloadLink(List<GitHubReleaseAsset> assets)
    {
        var osString = OperatingSystem.IsWindows() ? "Windows" : "Linux";

        var release = assets.Find(x => x.Name.Contains(osString, StringComparison.OrdinalIgnoreCase))!;
        var link = release.BrowserDownloadUrl;

        Logger.Information("Get {OsString} download link success {Link}", osString, link);
        return link;
    }

    [GeneratedRegex(@"v?(\d+\.\d+\.\d+-?\w*)")]
    private static partial Regex VersionRegex();

    #endregion
}