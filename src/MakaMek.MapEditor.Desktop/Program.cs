using Avalonia;
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
            .UseDependencyInjection(_ => {})
            .WithInterFont()
            .LogToTrace();
}
