using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MuseDashModToolsUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SwitchTab(object? sender, SelectionChangedEventArgs e)
    {
        var tabStrip = (TabStrip)sender!;
        //Locator.Current.GetRequiredService<IMainWindowViewModel>().SwitchTab(tabStrip.SelectedIndex);
    }
}