﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using DynamicData;
using DynamicData.Binding;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class ModManageViewModel : ViewModelBase, IModManageViewModel
{
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name!);
    private readonly FileSystemWatcher _watcher = new();
    [ObservableProperty] private FilterType _categoryFilterType;
    [ObservableProperty] private string _filter;

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

    public ReadOnlyObservableCollection<Mod> Mods => _mods;

    public ModManageViewModel()
    {
        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(_filter) ||
                         x.Name!.Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
                         x.XamlDescription.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _categoryFilterType != FilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => _categoryFilterType != FilterType.Outdated || x.State == UpdateState.Outdated)
            .Filter(x => _categoryFilterType != FilterType.Installed || x.IsLocal)
            .Filter(x => _categoryFilterType != FilterType.Incompatible || x is { IsIncompatible: true, IsLocal: true })
            .Sort(SortExpressionComparer<Mod>.Ascending(t => t.Name!))
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
    partial void OnCategoryFilterTypeChanged(FilterType value)
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
    private async Task OnInstallMod(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnInstallMod, item);

    [RelayCommand]
    private async Task OnReinstallMod(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnReinstallMod, item);

    [RelayCommand]
    private async Task OnToggleMod(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnToggleMod, item);

    [RelayCommand]
    private async Task OnDeleteMod(Mod item) => await ExecuteWithWatcherDisabled(ModService.OnDeleteMod, item);

    [RelayCommand]
    private async Task OnInstallMelonLoader() => await LocalService.OnInstallMelonLoader();

    [RelayCommand]
    private async Task OnUninstallMelonLoader() => await LocalService.OnUninstallMelonLoader();

    [RelayCommand]
    private void OnLaunchVanillaGame() => LocalService.OnLaunchGame(false);

    [RelayCommand]
    private void OnLaunchModdedGame() => LocalService.OnLaunchGame(true);

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
    private async Task OpenUserDataFolder() => await LocalService.OpenUserDataFolder();

    [RelayCommand]
    private async Task OpenModsFolder() => await LocalService.OpenModsFolder();

    [RelayCommand]
    private async Task OnCheckUpdate() => await GitHubService.CheckUpdates(true);

    [RelayCommand]
    private void OnFilterAll() => CategoryFilterType = FilterType.All;

    [RelayCommand]
    private void OnFilterInstalled() => CategoryFilterType = FilterType.Installed;

    [RelayCommand]
    private void OnFilterEnabled() => CategoryFilterType = FilterType.Enabled;

    [RelayCommand]
    private void OnFilterOutdated() => CategoryFilterType = FilterType.Outdated;

    [RelayCommand]
    private void OnFilterIncompatible() => CategoryFilterType = FilterType.Incompatible;

    #endregion
}