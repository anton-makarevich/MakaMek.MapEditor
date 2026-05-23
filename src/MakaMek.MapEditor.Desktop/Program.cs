using Avalonia;
using Sanet.MakaMek.MapEditor.Desktop.DependencyInjection;
using Sanet.MVVM.DI.Avalonia.Extensions;

namespace Sanet.MakaMek.MapEditor.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseDependencyInjection(services => services.RegisterPlatformServices())
            .WithInterFont()
            .LogToTrace();
}
