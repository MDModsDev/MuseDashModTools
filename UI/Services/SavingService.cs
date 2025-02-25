﻿using System.IO.Abstractions;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class SavingService : ISavingService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IPlatformService _platformService;
    private bool _isSavedLoaded;

    private static string ConfigFolderPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools");

    private static string SettingPath => Path.Combine(ConfigFolderPath, "Settings.json");

    [UsedImplicitly]
    public IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public ISerializationService SerializationService { get; init; }

    [UsedImplicitly]
    public IUpdateUIService UpdateUIService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    public SavingService(IFileSystem fileSystem, ILogger logger, IPlatformService platformService)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _platformService = platformService;
        Load().ConfigureAwait(false);
    }

    public Setting Settings { get; } = new();
    public string ModLinksPath => Path.Combine(ConfigFolderPath, "ModLinks.json");
    public string ChartFolderPath => Path.Combine(ConfigFolderPath, "Charts");

    public async Task InitializeSettingsAsync()
    {
        _logger.Information("Initializing settings...");

        if (!_isSavedLoaded)
        {
            _logger.Warning("Didn't load setting from Settings.json, getting game path...");
            if (!await TryGetGameFolderPath())
            {
                await MessageBoxService.WarningMessageBox(MsgBox_Content_ChoosePath);
                await OnChooseGamePathAsync();
            }
        }

        await CheckSettingValidity();
        await UpdateUIService.InitializeAllTabsAsync();

        _logger.Information("Settings initialize finished");
    }

    public async Task SaveAsync()
    {
        var json = SerializationService.SerializeSetting(Settings);
        await _fileSystem.File.WriteAllTextAsync(SettingPath, json);
        _logger.Information("Settings saved to Settings.json");
    }

    public async Task<bool> OnChooseGamePathAsync()
    {
        var path = await GetChosenPath();
        if (!await CheckValidPath(path)) return false;

        _logger.Information("User chose path {Path}", path);
        Settings.MuseDashFolder = path;

        await CheckSettingValidity();
        return true;
    }
}