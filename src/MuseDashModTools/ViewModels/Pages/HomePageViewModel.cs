using System.Collections.ObjectModel;
using Ursa.Common;
using Ursa.Controls.Options;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class HomePageViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task ShowDonationDrawer()
    {
        var options = new DrawerOptions
        {
            Position = Position.Right,
            Title = "原神启动",
            IsCloseButtonVisible = true,
            CanLightDismiss = true,
            MaxWidth = 700,
            MinWidth = 700,
            Buttons = DialogButton.None,
            CanResize = false
        };

        await Drawer.ShowModal<DonationDialog, DonationDialogViewModel>(new DonationDialogViewModel(), "DonationDrawerHost", options);
    }

    [RelayCommand]
    private void LaunchModdedGame()
    {
    }

    [RelayCommand]
    private void LaunchVanillaGame()
    {
    }
}