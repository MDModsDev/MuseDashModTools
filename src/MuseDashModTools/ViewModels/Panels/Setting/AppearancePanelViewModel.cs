﻿namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed class AppearancePanelViewModel : ViewModelBase
{
    #region Injections

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    #endregion Injections
}