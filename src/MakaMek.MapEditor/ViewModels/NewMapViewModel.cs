using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Services;
using Sanet.MakaMek.Presentation.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class NewMapViewModel : BaseViewModel
{
    public NewMapViewModel(
        IMapPreviewRenderer previewRenderer,
        IBattleMapFactory mapFactory,
        IMapResourceProvider mapResourceProvider,
        IFileService fileService,
        ILogger<NewMapViewModel> logger,
        IDispatcherService dispatcherService,
        ILocalizationService localizationService)
    {
        MapConfig = new MapConfigViewModel(
            previewRenderer,
            mapFactory,
            mapResourceProvider,
            fileService,
            logger,
            dispatcherService,
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

    public override void DetachHandlers()
    {
        base.DetachHandlers();
        MapConfig.Dispose();
    }
}
