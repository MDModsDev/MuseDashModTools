﻿namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new("About", AboutPanelName),
        new("Appearance", AppearancePanelName),
        new("Experience", ExperiencePanelName),
        new("File Management", FileManagementPanelName),
        new("Download", DownloadPanelName),
        new("Advanced", AdvancedPanelName)
    ];

    protected override Task OnActivatedAsync(CompositeDisposable disposables)
    {
        base.OnActivatedAsync(disposables);

        Logger.ZLogInformation($"{nameof(SettingPageViewModel)} Initialized");
        return Task.CompletedTask;
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<SettingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    #endregion Injections
}