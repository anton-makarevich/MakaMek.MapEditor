using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Services;

namespace Sanet.MakaMek.MapEditor.Android.DependencyInjection;

public static class AndroidServices
{
    public static void RegisterPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileCachingService, FileSystemCachingService>();
    }
}
