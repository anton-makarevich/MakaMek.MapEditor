using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Services;

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
        services.AddSingleton<IScheduler>(TaskPoolScheduler.Default);
    }
}
