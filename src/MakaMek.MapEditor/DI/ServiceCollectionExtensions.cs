using MakaMek.MapEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using MakaMek.MapEditor.ViewModels;

namespace MakaMek.MapEditor.DI;

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
