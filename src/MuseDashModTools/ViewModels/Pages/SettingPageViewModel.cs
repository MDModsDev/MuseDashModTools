﻿using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("About", AboutPanelName),
        new("Appearance", AppearancePanelName),
        new("Experience", ExperiencePanelName),
        new("File Management", FileManagementPanelName),
        new("Download", DownloadPanelName),
        new("Advanced", AdvancedPanelName)
    ];

    protected override void Initialize()
    {
        base.Initialize();
        Logger.ZLogInformation($"{nameof(SettingPageViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<SettingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    #endregion Injections
}