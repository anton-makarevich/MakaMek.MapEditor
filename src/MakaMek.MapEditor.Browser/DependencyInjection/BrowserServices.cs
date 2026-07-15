using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Services;
using Sanet.MakaMek.Services.Avalonia.Browser.Services;
using Sanet.MVVM.ExternalNavigation.Browser.Extensions;

namespace Sanet.MakaMek.MapEditor.Browser.DependencyInjection;

public static class BrowserServices
{
    public static void RegisterBrowserServices(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(
#if DEBUG
                LogLevel.Debug
#else
                LogLevel.Information
#endif
                );
        });

        // Register browser caching service for WASM platform
        services.AddSingleton<IFileCachingService, BrowserCachingService>();
        
        // Register CurrentThreadScheduler for WASM (single-threaded)
        services.AddSingleton<IScheduler>(CurrentThreadScheduler.Instance);
        services.AddBrowserExternalNavigation();
    }
}
