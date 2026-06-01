using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
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

    private int _loadedBiomes;
    
    public MainMenuViewModel(ILogger<MainMenuViewModel> logger, ITerrainAssetService terrainAssetService, ILocalizationService localizationService)
    {
        _logger = logger;
        _terrainAssetService = terrainAssetService;
        _localizationService = localizationService;
    }
    
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

    public IAsyncCommand RetryLoadCommand => field ??= new AsyncCommand(PreloadBiomes);

    public override void AttachHandlers()
    {
        base.AttachHandlers();
        if (IsLoading || _loadedBiomes > 0)
            return;
        PreloadBiomes().SafeFireAndForget();
    }

    private async Task PreloadBiomes()
    {
        try
        {
            HasError = false;
            IsLoading = true;
            var biomes = await _terrainAssetService.GetLoadedBiomes();
            _loadedBiomes = biomes.Count();

            BiomeLoadingStatus = _loadedBiomes == 0
                ? _localizationService.GetString("Status_NoBiomesFound")
                : string.Format(_localizationService.GetString("Status_BiomesLoaded"), _loadedBiomes);

            if (_loadedBiomes == 0)
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
