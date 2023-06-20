﻿using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;
using ILogger = Serilog.ILogger;

namespace MuseDashModToolsUI.Services;

public class SettingService : ISettingService
{
    private readonly IDialogueService _dialogueService;
    private readonly ILogger _logger;

    public SettingService(IDialogueService dialogueService, ILogger logger)
    {
        _dialogueService = dialogueService;
        _logger = logger;
        Task.Run(InitializeLanguageAndPath);
    }

    public Setting Settings { get; set; } = new();

    public async Task InitializeSettings()
    {
        try
        {
            if (!File.Exists("Settings.json"))
            {
                await _dialogueService.CreateErrorMessageBox("Warning", MsgBox_Content_ChoosePath.Localize());
                await OnChoosePath();
                return;
            }

            var text = await File.ReadAllTextAsync("Settings.json");
            var settings = JsonSerializer.Deserialize<Setting>(text)!;
            if (string.IsNullOrEmpty(settings.MuseDashFolder))
            {
                await _dialogueService.CreateErrorMessageBox(MsgBox_Title_Warning, MsgBox_Content_NullPath.Localize());
                await OnChoosePath();
                await InitializeSettings();
            }

            if (string.IsNullOrEmpty(settings.LanguageCode)) settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();

            Settings = settings.Clone();

            var updateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Update");
            var updaterPath = Path.Combine(updateDirectory, "Updater.exe");
            if (File.Exists(updaterPath))
                File.Delete(updaterPath);
            if (Directory.Exists(updateDirectory))
                Directory.Delete(updateDirectory);
        }
        catch (Exception ex)
        {
            await _dialogueService.CreateErrorMessageBox(ex.ToString());
        }
    }

    public async Task OnChoosePath()
    {
        while (true)
        {
            if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime) continue;
            var dialogue = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                { AllowMultiple = false, Title = FolderDialog_Title });
            if (dialogue.Count <= 0)
            {
                if (!string.IsNullOrEmpty(Settings.MuseDashFolder))
                    break;
                await _dialogueService.CreateErrorMessageBox(MsgBox_Content_InvalidPath);
                continue;
            }

            var path = dialogue[0].TryGetLocalPath();
            Settings.MuseDashFolder = path;
            Settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync("Settings.json", json);

            Locator.Current.GetRequiredService<ISettingsViewModel>().Initialize();
            break;
        }
    }

    private async Task InitializeLanguageAndPath()
    {
        if (!File.Exists("Settings.json"))
            return;
        var text = await File.ReadAllTextAsync("Settings.json");
        var setting = JsonNode.Parse(text);
        Settings.LanguageCode = setting?["LanguageCode"]?.ToString();
        Settings.MuseDashFolder = setting?["MuseDashFolder"]?.ToString();
    }
}