﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class GitHubService : IGitHubService
{
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases/latest";

    private const string PrimaryLink = "https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";
    private const string SecondaryLink = "https://gitee.com/lxymahatma/ModLinks/raw/main/";
    private const string ThirdLink = "https://ghproxy.com/https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";
    private readonly HttpClient _client;
    private readonly IDialogueService _dialogueService;

    public GitHubService(HttpClient client, IDialogueService dialogueService)
    {
        _client = client;
        _dialogueService = dialogueService;
    }

    public async Task<List<Mod>> GetModsAsync()
    {
        List<Mod> mods;
        try
        {
            mods = (await _client.GetFromJsonAsync<List<Mod>>(PrimaryLink + "ModLinks.json"))!;
        }
        catch
        {
            try
            {
                mods = (await _client.GetFromJsonAsync<List<Mod>>(SecondaryLink + "ModLinks.json"))!;
            }
            catch
            {
                mods = (await _client.GetFromJsonAsync<List<Mod>>(ThirdLink + "ModLinks.json"))!;
            }
        }

        return mods;
    }

    public async Task DownloadModAsync(string link, string path)
    {
        HttpResponseMessage result;
        try
        {
            result = await _client.GetAsync(PrimaryLink + link);
        }
        catch
        {
            try
            {
                result = await _client.GetAsync(SecondaryLink + link);
            }
            catch
            {
                result = await _client.GetAsync(ThirdLink + link);
            }
        }

        await using var fs = new FileStream(path, FileMode.OpenOrCreate);
        await result.Content.CopyToAsync(fs);
    }

    public async Task DownloadMelonLoader(string path, IProgress<double> downloadProgress)
    {
        HttpResponseMessage result;
        try
        {
            result = await _client.GetAsync(PrimaryLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
        }
        catch
        {
            try
            {
                result = await _client.GetAsync(SecondaryLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
            }
            catch
            {
                result = await _client.GetAsync(ThirdLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
            }
        }

        var totalLength = result.Content.Headers.ContentLength;
        var contentStream = await result.Content.ReadAsStreamAsync();
        await using var fs = new FileStream(path, FileMode.OpenOrCreate);
        var buffer = new byte[5 * 1024];
        var readLength = 0L;
        int length;
        while ((length = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None)) != 0)
        {
            readLength += length;
            if (totalLength > 0) downloadProgress.Report(Math.Round((double)readLength / totalLength.Value * 100, 2));

            fs.Write(buffer, 0, length);
        }
    }

    public async Task CheckUpdates(bool userClick = false)
    {
        _client.DefaultRequestHeaders.Add("User-Agent", "MuseDashModToolsUI");

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        try
        {
            var result = await _client.GetStringAsync(ReleaseInfoLink);
            var doc = JsonDocument.Parse(result);

            if (!doc.RootElement.TryGetProperty("tag_name", out var tagName))
                return;

            var tag = tagName.GetString();
            if (tag is null) return;
            if (!Version.TryParse(tag, out var version)) return;
            if (version <= currentVersion)
            {
                if (userClick)
                    await _dialogueService.CreateMessageBox(MsgBox_Title_Success, MsgBox_Content_LatestVersion.Localize());
                return;
            }

            var title = doc.RootElement.GetProperty("name").GetString();
            var body = doc.RootElement.GetProperty("body").GetString();
            var update = await _dialogueService.CreateConfirmMessageBox(MsgBox_Title_Notice,
                string.Format(MsgBox_Content_NewerVersion.Localize(), version, title, body));

            if (!update) return;
            var link = string.Empty;
            var assets = doc.RootElement.GetProperty("assets");
            var releases = assets.EnumerateArray();

            if (OperatingSystem.IsLinux())
            {
                var release = releases.FirstOrDefault(x => x.GetProperty("name").GetString()!.EndsWith("Linux.zip"));
                link = release.GetProperty("browser_download_url").GetString()!;
            }

            if (OperatingSystem.IsWindows())
            {
                var release = releases.FirstOrDefault(x => x.GetProperty("name").GetString()!.EndsWith("Windows.zip"));
                link = release.GetProperty("browser_download_url").GetString()!;
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            await LaunchUpdater(currentDirectory, new[] { link, currentDirectory + ".zip", currentDirectory });
            Environment.Exit(0);
        }
        catch
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_CheckUpdateFailed.Localize());
        }
    }

    private async Task LaunchUpdater(string currentDirectory, IEnumerable<string> launchArgs)
    {
        var updaterExePath = Path.Combine(currentDirectory, "Updater.exe");
        var updaterTargetFolder = Path.Combine(currentDirectory, "Update");
        var updaterTargetPath = Path.Combine(currentDirectory, "Update", "Updater.exe");
        if (!File.Exists(updaterExePath))
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_UpdaterNotFound.Localize());
            return;
        }

        if (!Directory.Exists(updaterTargetFolder)) Directory.CreateDirectory(updaterTargetFolder);

        try
        {
            File.Copy(updaterExePath, updaterTargetPath, true);
        }
        catch (Exception ex)
        {
            await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_CopyUpdaterFailed.Localize(), ex));
        }

        Process.Start(updaterTargetPath, launchArgs);
    }
}