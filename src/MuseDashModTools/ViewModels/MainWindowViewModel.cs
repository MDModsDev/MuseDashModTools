namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(XAML_Page_Home, HomePageName, "Home"),
        new(XAML_Page_Modding, ModdingPageName, "Wrench"),
        new(XAML_Page_Charting, ChartingPageName, "Disc"),
        new(XAML_Page_Setting, SettingPageName, "Setting")
    ];

    protected override async Task OnActivatedAsync(CompositeDisposable disposables)
    {
        await base.OnActivatedAsync(disposables).ConfigureAwait(true);
        await SettingService.LoadAsync().ConfigureAwait(true);
        GetCurrentApplication().RequestedThemeVariant = AvaloniaResources.ThemeVariants[Config.Theme];
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
    }

    protected override void OnError(Exception ex)
    {
        base.OnError(ex);
        Logger.ZLogError(ex, $"{nameof(MainWindowViewModel)} Initialize Failed");
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

#if RELEASE
    [UsedImplicitly]
    public required IUpdateService UpdateService { get; init; }
#endif

    #endregion Injections
}