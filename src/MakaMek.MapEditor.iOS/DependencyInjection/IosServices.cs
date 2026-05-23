using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Services;

namespace Sanet.MakaMek.MapEditor.iOS.DependencyInjection;

public static class IosServices
{
    public static void RegisterPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileCachingService, FileSystemCachingService>();
    }
}
