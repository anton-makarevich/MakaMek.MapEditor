using Microsoft.Extensions.DependencyInjection;
using MakaMek.MapEditor.ViewModels;

namespace MakaMek.MapEditor.DI;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<Sanet.MakaMek.Map.Factories.IBattleMapFactory, Sanet.MakaMek.Map.Factories.BattleMapFactory>();
        services.AddSingleton<Services.IImageService, Services.ImageService>();
        services.AddSingleton<Services.IImageService<Avalonia.Media.Imaging.Bitmap>, Services.ImageService>();
        services.AddSingleton<Services.IFileService, Services.FileService>();
    }

    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<NewMapViewModel>();
        services.AddTransient<EditMapViewModel>();
    }
}
