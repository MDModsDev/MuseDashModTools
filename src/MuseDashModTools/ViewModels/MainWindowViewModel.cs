using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : PageViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new(XAML_Page_Home, "Home", HomePageName),
        new(XAML_Page_Modding, "Wrench", ModdingPageName),
        new(XAML_Page_Charting, "Disc", ChartingPageName),
        new(XAML_Page_Setting, "Setting", SettingPageName)
    ];

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Initialize();
        await SavingService.LoadSettingAsync().ConfigureAwait(true);
        GetCurrentApplication().RequestedThemeVariant = AvaloniaResources.ThemeVariants[Setting.Theme];
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        Logger.ZLogInformation($"MainWindow Initialized");
    }

    protected override void Navigate(NavItem? value)
    {
        Content = value?.NavigateKey switch
        {
            HomePageName => NavigationService.NavigateTo<HomePage>(),
            ModdingPageName => NavigationService.NavigateTo<ModdingPage>(),
            ChartingPageName => NavigationService.NavigateTo<ChartingPage>(),
            SettingPageName => NavigationService.NavigateTo<SettingPage>(),
            _ => throw new UnreachableException()
        };
    }

    #region Injections

    [UsedImplicitly]
    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<MainWindowViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public ISavingService SavingService { get; init; } = null!;

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public IUpdateService UpdateService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}