using System.Reactive.Concurrency;
using ReactiveUI.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Services;
using Sanet.MVVM.ExternalNavigation.Desktop.Extensions;

namespace Sanet.MakaMek.MapEditor.Desktop.DependencyInjection;

public static class DesktopServices
{
    public static void RegisterPlatformServices(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(
#if DEBUG
                LogLevel.Debug
#else
                LogLevel.Information
#endif
            );
        });
        services.AddSingleton<IFileCachingService, FileSystemCachingService>();
        services.AddSingleton<IScheduler>(AvaloniaScheduler.Instance);
        services.AddDesktopExternalNavigation();
    }
}
