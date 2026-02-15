using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.MapEditor.Services;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MakaMek.Services.Avalonia;

namespace Sanet.MakaMek.MapEditor.DI;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<Sanet.MakaMek.Map.Factories.IBattleMapFactory, Sanet.MakaMek.Map.Factories.BattleMapFactory>();
        services.AddSingleton<IImageService>(_ => new AvaloniaAssetImageService("avares://Sanet.MakaMek.MapEditor/Assets"));
        services.AddSingleton<IFileService, FileService>();
    }

    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<NewMapViewModel>();
        services.AddTransient<EditMapViewModel>();
    }
}
