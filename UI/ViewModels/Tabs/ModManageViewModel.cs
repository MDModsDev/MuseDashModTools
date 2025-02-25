﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using DynamicData;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class ModManageViewModel : ViewModelBase, IModManageViewModel
{
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name);
    private readonly FileSystemWatcher _watcher = new();
    [ObservableProperty] private ModFilterType _categoryModFilterType;
    [ObservableProperty] private string _filter;
    public ReadOnlyObservableCollection<Mod> Mods => _mods;

    [UsedImplicitly]
    public IGitHubService GitHubService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IModService ModService { get; init; }

    [UsedImplicitly]
    public ISavingService SavingService { get; init; }

    public ModManageViewModel()
    {
        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(Filter) ||
                         x.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase) ||
                         x.XamlDescription.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => CategoryModFilterType != ModFilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => CategoryModFilterType != ModFilterType.Outdated || x.State == UpdateState.Outdated)
            .Filter(x => CategoryModFilterType != ModFilterType.Installed || x.IsLocal)
            .Filter(x => CategoryModFilterType != ModFilterType.Incompatible || x is { IsIncompatible: true, IsLocal: true })
            .SortBy(x => x.Name)
            .Bind(out _mods)
            .Subscribe();
    }

    public async Task Initialize()
    {
        await ModService.InitializeModList(_sourceCache, Mods);
        StartModsDllMonitor();
        Logger.Information("Mod Manage Window Initialized");
    }

    private async Task ExecuteWithWatcherDisabled(Func<Mod, Task> func, Mod item)
    {
        _watcher.EnableRaisingEvents = false;
        await func(item);
        _watcher.EnableRaisingEvents = true;
    }

    [UsedImplicitly]
    partial void OnFilterChanged(string value) => _sourceCache.Refresh();

    [UsedImplicitly]
    partial void OnCategoryModFilterTypeChanged(ModFilterType value)
    {
        Logger.Information("Category Filter Changed: {Filter}", value);
        _sourceCache.Refresh();
    }

    // TODO Only load added/removed/modified mods instead of all (lxy, 2023/9/23) Planning Time: 2 months
    private void StartModsDllMonitor()
    {
        _watcher.Path = SavingService.Settings.ModsFolder;
        _watcher.Filters.Add("*.dll");
        _watcher.Filters.Add("*.disabled");
        _watcher.Renamed += async (_, _) => await ModService.InitializeModList(_sourceCache, Mods);
        _watcher.Changed += async (_, _) => await ModService.InitializeModList(_sourceCache, Mods);
        _watcher.Created += async (_, _) => await ModService.InitializeModList(_sourceCache, Mods);
        _watcher.Deleted += async (_, _) => await ModService.InitializeModList(_sourceCache, Mods);
        _watcher.EnableRaisingEvents = true;
        Logger.Information("Mods Dll File Monitor Started");
    }

    #region Commands

    [RelayCommand]
    private async Task OnInstallModAsync(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnInstallModAsync, item);

    [RelayCommand]
    private async Task OnReinstallModAsync(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnReinstallModAsync, item);

    [RelayCommand]
    private async Task OnToggleModAsync(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnToggleModAsync, item);

    [RelayCommand]
    private async Task OnDeleteModAsync(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnDeleteModAsync, item);

    [RelayCommand]
    private async Task OnInstallMelonLoaderAsync() => await LocalService.OnInstallMelonLoaderAsync();

    [RelayCommand]
    private async Task OnUninstallMelonLoaderAsync() => await LocalService.OnUninstallMelonLoaderAsync();

    [RelayCommand]
    private void OpenConfigFile(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Path.Combine(SavingService.Settings.UserDataFolder, path),
            UseShellExecute = true
        });
        Logger.Information("Open config file: {ConfigFile}", path);
    }

    [RelayCommand]
    private void OpenUrl(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
        Logger.Information("Open Url: {Url}", path);
    }

    [RelayCommand]
    private async Task OpenUserDataFolderAsync() => await LocalService.OpenUserDataFolderAsync();

    [RelayCommand]
    private async Task OpenModsFolderAsync() => await LocalService.OpenModsFolderAsync();

    [RelayCommand]
    private async Task OnCheckUpdateAsync() => await GitHubService.CheckUpdates(true);

    [RelayCommand]
    private void OnFilterAll() => CategoryModFilterType = ModFilterType.All;

    [RelayCommand]
    private void OnFilterInstalled() => CategoryModFilterType = ModFilterType.Installed;

    [RelayCommand]
    private void OnFilterEnabled() => CategoryModFilterType = ModFilterType.Enabled;

    [RelayCommand]
    private void OnFilterOutdated() => CategoryModFilterType = ModFilterType.Outdated;

    [RelayCommand]
    private void OnFilterIncompatible() => CategoryModFilterType = ModFilterType.Incompatible;

    #endregion
}