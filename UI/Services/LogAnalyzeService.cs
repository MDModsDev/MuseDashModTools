﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Serilog;
using ValveKeyValue;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public partial class LogAnalyzeService : ILogAnalyzeService
{
    private readonly FileSystemWatcher _watcher = new();
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public IModService ModService { get; init; }
    public ISavingService SavingService { get; init; }
    public Lazy<ILogAnalysisViewModel> LogAnalysisViewModel { get; init; }
    private string LogPath { get; set; } = string.Empty;
    private string LogContent { get; set; } = string.Empty;
    private StringBuilder LogErrorBuilder { get; } = new();

    public async Task AnalyzeLog()
    {
        CheckModVersion();
        if (LogErrorBuilder.Length > 0)
        {
            await MessageBoxService.CreateSuccessMessageBox(LogErrorBuilder.ToString(), Icon.Warning);
            LogErrorBuilder.Clear();
        }
        else
        {
            await MessageBoxService.CreateSuccessMessageBox("Log Analysis Completed");
            Logger.Information("Log Analysis Completed");
        }
    }

    public async Task<bool> CheckPirate()
    {
        if (!LogContent.Contains("ApplicationPath"))
        {
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoApplicationPath, Icon.Warning);
            return true;
        }

        var pathMatch = ApplicationPathRegex().Match(LogContent);
        if (!pathMatch.Success)
        {
            Logger.Information(@"Game path doesn't contain 'steamapps\common\Muse Dash\musedash.exe'");
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_GamePathError.Localize(), Icon.Warning);
            return true;
        }

        var steamPath = pathMatch.Groups[1].Value;
        var acfPath = Path.Combine(steamPath, "appmanifest_774171.acf");

        if (!File.Exists(acfPath))
        {
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoInstallRecord.Localize(), Icon.Warning);
            Logger.Information("Cannot find appmanifest_774171.acf");
            return true;
        }

        var stream = File.OpenRead(acfPath);
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var data = kv.Deserialize<AppState>(stream);

        if (data.Appid != 774171 || data.Name != "Muse Dash" || data.InstalledDepots.Keys.All(x => x != 774172))
        {
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoInstallRecord.Localize(), Icon.Warning);
            Logger.Information("Cannot find Muse Dash download record in file");
            return true;
        }

        if (data.InstalledDepots.Keys.Any(x => x == 1055810) && data.InstalledDepots[1055810]["dlcappid"] == 1055810)
        {
            Logger.Information("Muse Dash DLC purchased");
            return false;
        }

        await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoDlcPurchased.Localize(), Icon.Warning);
        Logger.Information("Cannot find Muse Dash DLC purchase record in file");
        return false;
    }

    public async Task<bool> CheckMelonLoaderVersion()
    {
        var version = MelonLoaderVersionRegex().Match(LogContent);
        if (!version.Success)
        {
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoMelonLoaderVersion, Icon.Warning);
            Logger.Information("MelonLoader Version not found");
            return false;
        }

        var melonLoaderVersion = version.Groups[1].Value;
        if (melonLoaderVersion == "0.5.7")
        {
            Logger.Information("Correct MelonLoader Version: {MelonLoaderVersion}", melonLoaderVersion);
            return true;
        }

        Logger.Information("Incorrect MelonLoader Version: {MelonLoaderVersion}", melonLoaderVersion);
        await MessageBoxService.CreateSuccessMessageBox(
            string.Format(MsgBox_Content_IncorrectMelonLoaderVersion.Localize(), melonLoaderVersion), Icon.Warning);
        return false;
    }

    public async Task<string> LoadLog()
    {
        if (!string.IsNullOrEmpty(SavingService.Settings.MelonLoaderFolder))
            LogPath = Path.Combine(SavingService.Settings.MelonLoaderFolder, "Latest.log");
        if (!File.Exists(LogPath)) return MsgBox_Content_NoLogFile.Localize();

        try
        {
            await using var stream = new FileStream(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            LogContent = await reader.ReadToEndAsync();
            Logger.Information("Read Log Success");
            StartLogFileMonitor();
            return LogContent;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Read Log Error");
            return string.Empty;
        }
    }

    private void CheckModVersion()
    {
        var modVersionMatches = ModVersionRegex().Matches(LogContent);
        if (modVersionMatches.Count == 0) return;

        foreach (var mod in modVersionMatches.Select(x => x.Groups))
        {
            var modName = mod[1].Value;
            var modVersion = mod[2].Value;
            var version = ModService.CompareVersion(modName, modVersion);
            if (version) LogErrorBuilder.AppendFormat(MsgBox_Content_OutdatedMod, modName).AppendLine();
        }
    }

    private void StartLogFileMonitor()
    {
        _watcher.Path = SavingService.Settings.MelonLoaderFolder;
        _watcher.Filter = "Latest.log";
        _watcher.Renamed += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.Changed += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.Created += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.Deleted += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.EnableRaisingEvents = true;
        Logger.Information("Log File Monitor Started");
    }

    [GeneratedRegex(@"ApplicationPath = (.*Steam\\steamapps)\\common\\Muse Dash\\musedash.exe")]
    private static partial Regex ApplicationPathRegex();

    [GeneratedRegex(@"MelonLoader v(\d+\.\d+\.\d+)")]
    private static partial Regex MelonLoaderVersionRegex();

    [GeneratedRegex(@"\[\d{2}:\d{2}:\d{2}.\d{3}] (.*?) v(\d+\.\d+\.\d+)", RegexOptions.Multiline)]
    private static partial Regex ModVersionRegex();
}