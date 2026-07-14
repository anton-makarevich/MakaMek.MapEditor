using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Sanet.MakaMek.Core.Services;

namespace Sanet.MakaMek.MapEditor.Services;

public class AvaloniaExternalNavigationService : IExternalNavigationService
{
    public async Task OpenUrlAsync(string url)
    {
        var topLevel = GetTopLevel();
        if (topLevel?.Launcher == null) return;

        await topLevel.Launcher.LaunchUriAsync(new Uri(url));
    }

    public async Task OpenEmailAsync(string emailAddress, string subject)
    {
        var topLevel = GetTopLevel();
        if (topLevel?.Launcher == null) return;

        var uri = new Uri($"mailto:{emailAddress}?subject={Uri.EscapeDataString(subject)}");
        await topLevel.Launcher.LaunchUriAsync(uri);
    }

    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }
}
