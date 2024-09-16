using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace MuseDashModToolsUI.Utils;

public static class DesktopUtils
{
    public static IClassicDesktopStyleApplicationLifetime? GetCurrentDesktop() =>
        Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

    public static Window GetCurrentMainWindow()
    {
        var desktop = GetCurrentDesktop();
        if (desktop is null)
        {
            throw new InvalidOperationException("Desktop is null.");
        }

        if (desktop.MainWindow is null)
        {
            throw new InvalidOperationException("MainWindow is null.");
        }

        return desktop.MainWindow;
    }

    public static ILauncher GetLauncher() => GetCurrentMainWindow().Launcher;
}