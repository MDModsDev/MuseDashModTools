﻿namespace MuseDashModTools.ViewModels;

public partial class NavViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedItem { get; set; }

    [UsedImplicitly]
    public virtual IReadOnlyList<NavItem> NavItems { get; } = null!;

    protected virtual void Navigate(NavItem? value)
    {
    }

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        SelectedItem = NavItems[0];
        return Task.CompletedTask;
    }

    [UsedImplicitly]
    partial void OnSelectedItemChanged(NavItem? value)
    {
        Navigate(value);
    }
}