﻿using System.Net.Http;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class GitHubService : IGitHubService
{
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases";

    private const string PrimaryLink = "https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";

    private const string SecondaryLink = "https://ghproxy.com/https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";

    private const string ThirdLink = "https://gitee.com/lxymahatma/ModLinks/raw/main/";

    private HttpResponseMessage? _melonLoaderResponseMessage;
    private string DefaultDownloadSource => DownloadSourceDictionary[SavingService.Settings.DownloadSource];

    private static Dictionary<DownloadSources, string> DownloadSourceDictionary => new()
    {
        { DownloadSources.Github, PrimaryLink },
        { DownloadSources.GithubMirror, SecondaryLink },
        { DownloadSources.Gitee, ThirdLink }
    };

    public HttpClient Client { get; init; }
    public ILocalService LocalService { get; init; }
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ISavingService SavingService { get; init; }

    public async Task CheckUpdates(bool userClick = false)
    {
        Logger.Information("Checking updates...");
        Client.DefaultRequestHeaders.Add("User-Agent", "MuseDashModToolsUI");

        Logger.Information("Get current version success: {Version}", BuildInfo.Version);
        try
        {
            var releases = await Client.GetFromJsonAsync<List<GithubRelease>>(ReleaseInfoLink);
            Logger.Information("Get releases success");

            var release = SavingService.Settings.DownloadPrerelease ? releases[0] : releases.Find(x => !x.Prerelease)!;

            var version = GetVersionFromTag(release.TagName);
            if (version is null || await SkipVersionCheck(version, BuildInfo.Version, userClick)) return;

            var title = release.Name;
            var body = release.Body;
            if (!await UpdateRequired(release.TagName, title, body)) return;

            var link = GetDownloadLink(release.Assets);
            if (!await LocalService.LaunchUpdater(link)) return;
            Logger.Information("Launch updater success, exit...");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Check updates failed");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_CheckUpdateFailed);
        }
    }

    public async Task DownloadModAsync(string link, string path)
    {
        var defaultDownloadSource = DownloadSourceDictionary[SavingService.Settings.DownloadSource];
        var result = await DownloadModFromSourceAsync(defaultDownloadSource, link, path);
        if (result is not null) return;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SavingService.Settings.DownloadSource))
        {
            result = await DownloadModFromSourceAsync(pair.Value, link, path);
            if (result is not null) return;
        }
    }

    public async Task<bool> DownloadMelonLoader(IProgress<double> downloadProgress)
    {
        if (_melonLoaderResponseMessage is null) await GetMelonLoaderResponseMessage();
        if (_melonLoaderResponseMessage is null) return false;
        return await DownloadMelonLoaderFromSourceAsync(downloadProgress);
    }

    public async Task<long?> GetMelonLoaderFileSize()
    {
        await GetMelonLoaderResponseMessage();
        return _melonLoaderResponseMessage is not null ? _melonLoaderResponseMessage.Content.Headers.ContentLength : 0;
    }

    public async Task<List<Mod>?> GetModListAsync()
    {
        var mods = await GetModListFromSourceAsync(DefaultDownloadSource);
        if (mods is not null) return mods;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SavingService.Settings.DownloadSource))
        {
            mods = await GetModListFromSourceAsync(pair.Value);
            if (mods is not null) return mods;
        }

        await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_GetModListFailed);
        return null;
    }
}