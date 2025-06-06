using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Binding;

namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class ModsPanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ModDto> _mods;
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);
    private ModFilterType _modFilter = ModFilterType.All;

    public static string[] ModFilterTypes { get; } =
    [
        XAML_ModFilterType_All,
        XAML_ModFilterType_Installed,
        XAML_ModFilterType_Enabled,
        XAML_ModFilterType_Disabled,
        XAML_ModFilterType_Outdated,
        XAML_ModFilterType_Incompatible
    ];

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial ModDto SelectedMod { get; set; } = null!;

    [ObservableProperty]
    public partial int SelectedModFilterIndex { get; set; }

    public ReadOnlyObservableCollection<ModDto> Mods => _mods;

    public ModsPanelViewModel()
    {
        var comparer = SortExpressionComparer<ModDto>
            .Descending(x => x.State is ModState.Duplicated)
            .ThenByDescending(x => x.State is ModState.Modified)
            .ThenByDescending(x => x is { IsLocal: true, IsDisabled: false })
            .ThenByDescending(x => x.IsLocal)
            .ThenByDescending(x => x is { State: ModState.Outdated, IsLocal: true })
            .ThenByDescending(x => x.IsInstallable)
            .ThenByAscending(x => x.Name);

        _sourceCache.Connect()
            .Filter(x => SearchText.IsNullOrEmpty()
                         || x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                         || x.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _modFilter != ModFilterType.Installed || x.IsLocal)
            .Filter(x => _modFilter != ModFilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => _modFilter != ModFilterType.Disabled || x is { IsDisabled: true, IsLocal: true })
            .Filter(x => _modFilter != ModFilterType.Outdated || x.State == ModState.Outdated)
            .Filter(x => _modFilter != ModFilterType.Incompatible || x is { State: ModState.Incompatible, IsLocal: true })
            .SortAndBind(out _mods, comparer)
            .Subscribe();
    }

    protected override async Task OnActivatedAsync(CompositeDisposable disposables)
    {
        await base.OnActivatedAsync(disposables).ConfigureAwait(false);

        await ModManageService.InitializeModsAsync(_sourceCache).ConfigureAwait(false);
        Logger.ZLogInformation($"{nameof(ModsPanelViewModel)} Initialized");
    }

    protected override void OnError(Exception ex)
    {
        base.OnError(ex);
        Logger.ZLogError(ex, $"{nameof(ModsPanelViewModel)} Initialize Failed");
    }

    [RelayCommand]
    private Task OpenConfigFileAsync()
    {
        Logger.ZLogInformation($"Opening config file for mod: {SelectedMod.Name}");
        return PlatformService.OpenFileAsync(Path.Combine(Config.UserDataFolder, SelectedMod.ConfigFile));
    }

    [RelayCommand]
    private async Task InstallModAsync()
    {
        Logger.ZLogInformation($"Installing mod: {SelectedMod.Name}");
        await ModManageService.InstallModAsync(SelectedMod).ConfigureAwait(false);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully installed");
    }

    [RelayCommand]
    private async Task UpdateModAsync()
    {
        Logger.ZLogInformation($"Updating mod: {SelectedMod.Name} from version {SelectedMod.LocalVersion} to version {SelectedMod.Version}");
        File.Delete(Path.Combine(Config.ModsFolder, SelectedMod.LocalFileName));
        await ModManageService.InstallModAsync(SelectedMod).ConfigureAwait(false);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully updated to version {SelectedMod.Version}");
    }

    [RelayCommand]
    private async Task ReinstallModAsync()
    {
        Logger.ZLogInformation($"Reinstalling mod: {SelectedMod.Name}");
        File.Delete(Path.Combine(Config.ModsFolder, SelectedMod.LocalFileName));
        await ModManageService.InstallModAsync(SelectedMod).ConfigureAwait(false);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully reinstalled");
    }

    [RelayCommand]
    private async Task UninstallModAsync()
    {
        Logger.ZLogInformation($"Uninstalling mod: {SelectedMod.Name}");
        await ModManageService.UninstallModAsync(SelectedMod).ConfigureAwait(false);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully uninstalled");
    }

    [RelayCommand]
    private Task ToggleModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Toggling mod: {mod.Name}");
        return ModManageService.ToggleModAsync(mod);
    }

    [UsedImplicitly]
    partial void OnSelectedModFilterIndexChanged(int value)
    {
        _modFilter = (ModFilterType)value;
        _sourceCache.Refresh();
    }

    [UsedImplicitly]
    partial void OnSearchTextChanged(string? value) => _sourceCache.Refresh();

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<ModsPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    #endregion Injections
}