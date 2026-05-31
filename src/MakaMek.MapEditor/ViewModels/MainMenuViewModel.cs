using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Localization;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class MainMenuViewModel : BaseViewModel
{
    private readonly ILogger<MainMenuViewModel> _logger;
    private readonly ITerrainAssetService _terrainAssetService;
    private readonly ILocalizationService _localizationService;

    public string BiomeLoadingStatus
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public bool HasError
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public bool IsLoading
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public MainMenuViewModel(ILogger<MainMenuViewModel> logger, ITerrainAssetService terrainAssetService, ILocalizationService localizationService)
    {
        _logger = logger;
        _terrainAssetService = terrainAssetService;
        _localizationService = localizationService;
    }

    public override void AttachHandlers()
    {
        base.AttachHandlers();
        PreloadBiomes().SafeFireAndForget();
    }

    private async Task PreloadBiomes()
    {
        try
        {
            IsLoading = true;
            var biomes = await _terrainAssetService.GetLoadedBiomes();
            var biomeCount = biomes.Count();

            BiomeLoadingStatus = biomeCount == 0
                ? _localizationService.GetString("Status_NoBiomesFound")
                : string.Format(_localizationService.GetString("Status_BiomesLoaded"), biomeCount);

            if (biomeCount == 0)
                throw new Exception(_localizationService.GetString("Status_NoBiomesFound"));

            await NavigationService.NavigateToViewModelAsync<NewMapViewModel>();
        }
        catch (Exception ex)
        {
            HasError = true;
            BiomeLoadingStatus = string.Format(_localizationService.GetString("Status_ErrorLoadingBiomes"), ex.Message);
            _logger.LogError(ex, "Error loading biomes");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
