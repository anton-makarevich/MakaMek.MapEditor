using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.Localization;
using Sanet.MakaMek.Presentation.ViewModels;
using Sanet.MakaMek.Presentation.ViewModels.Maps;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class NewMapViewModel : BaseViewModel
{
    public NewMapViewModel(
        MapGenerationService mapGenerationService,
        AvailableMapsService availableMapsService,
        ILogger<MapConfigViewModel> mapConfigLogger,
        ILocalizationService localizationService)
    {
        MapConfig = new MapConfigViewModel(
            mapGenerationService,
            availableMapsService,
            mapConfigLogger,
            localizationService);
    }

    public MapConfigViewModel MapConfig { get; }

    public IAsyncCommand EditMapCommand => field ??= new AsyncCommand(async () =>
    {
        var map = MapConfig.Map;
        if (map == null) return;

        var editViewModel = NavigationService.GetViewModel<EditMapViewModel>();
        if (editViewModel != null)
        {
            editViewModel.Initialize(map);
            await NavigationService.NavigateToViewModelAsync(editViewModel);
        }
    });

    public IAsyncCommand OpenAboutCommand => field ??= new AsyncCommand(async () =>
    {
        await NavigationService.NavigateToViewModelAsync<AboutViewModel>();
    });
}
