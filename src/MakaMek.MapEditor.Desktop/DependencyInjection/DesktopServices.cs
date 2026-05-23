using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Services;

namespace Sanet.MakaMek.MapEditor.Desktop.DependencyInjection;

public static class DesktopServices
{
    public static void RegisterPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileCachingService, FileSystemCachingService>();
    }
}
