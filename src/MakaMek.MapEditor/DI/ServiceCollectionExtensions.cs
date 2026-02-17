using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MakaMek.Services.Avalonia;

namespace Sanet.MakaMek.MapEditor.DI;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
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
        services.AddSingleton<Map.Factories.IBattleMapFactory, Map.Factories.BattleMapFactory>();
        services.AddSingleton<IImageService>(_ => new AvaloniaAssetImageService("avares://Sanet.MakaMek.MapEditor/Assets"));
        services.AddSingleton<IFileService, AvaloniaFileService>();
    }

    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<NewMapViewModel>();
        services.AddTransient<EditMapViewModel>();
    }
}
