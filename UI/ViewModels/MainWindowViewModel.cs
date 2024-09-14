namespace MuseDashModToolsUI.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isCollapsed;

    public static string Version => $"v{AppVersion}";

    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    public static PageNavItem[] PageNavItems =>
    [
        new() { Name = "Mod Manage" },
        new() { Name = "Setting" }
    ];

    [RelayCommand]
    private Task Test() => SavingService.LoadSettingAsync();

    [RelayCommand]
    private void Test2() => Logger.Information(Setting.MuseDashFolder);

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await SavingService.LoadSettingAsync().ConfigureAwait(true);
        GetCurrentDesktop()!.Exit += (_, _) => SavingService.SaveSettingAsync();
    }

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public ISavingService SavingService { get; init; } = null!;

    #endregion Injections
}