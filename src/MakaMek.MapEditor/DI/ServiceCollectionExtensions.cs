using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.MapEditor.Services;
using Sanet.MakaMek.MapEditor.ViewModels;

namespace Sanet.MakaMek.MapEditor.DI;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<Sanet.MakaMek.Map.Factories.IBattleMapFactory, Sanet.MakaMek.Map.Factories.BattleMapFactory>();
        services.AddSingleton<IImageService, AvaloniaAssetImageService>();
        services.AddSingleton<IFileService, FileService>();
    }

    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<NewMapViewModel>();
        services.AddTransient<EditMapViewModel>();
    }
}
