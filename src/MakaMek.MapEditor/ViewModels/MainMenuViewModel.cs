using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class MainMenuViewModel : BaseViewModel
{
    private readonly IFileService _fileService;
    private readonly IBattleMapFactory _mapFactory;
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
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(CanShowMenu));
        }
    }

    public bool IsLoading
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(CanShowMenu));
        }
    }

    public bool CanShowMenu => !IsLoading && !HasError;

    public MainMenuViewModel(IFileService fileService, IBattleMapFactory mapFactory, ILogger<MainMenuViewModel> logger, ITerrainAssetService terrainAssetService, ILocalizationService localizationService)
    {
        _fileService = fileService;
        _mapFactory = mapFactory;
        _logger = logger;
        _terrainAssetService = terrainAssetService;
        _localizationService = localizationService;
        
        // Initialize preloading
        _ = PreloadBiomes();
    }

    public IAsyncCommand CreateNewMapCommand => field ??= new AsyncCommand(() => 
        NavigationService.NavigateToViewModelAsync<NewMapViewModel>());

    public IAsyncCommand LoadMapCommand => field ??= new AsyncCommand(async () =>
    {
        try
        {
            var content = (await _fileService.OpenFile(_localizationService.GetString("MainMenu_LoadMap"))).Content;
            if (string.IsNullOrEmpty(content)) return;
            var data = JsonSerializer.Deserialize<BattleMapData>(content);
            if (data != null)
            {
                var map = _mapFactory.CreateFromData(data);
                var editViewModel = NavigationService.GetViewModel<EditMapViewModel>();
                if (editViewModel != null)
                {
                    editViewModel.Initialize(map);
                    await NavigationService.NavigateToViewModelAsync(editViewModel);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load map");
        }
    });

    /// <summary>
    /// Preloads biome data from all configured providers
    /// </summary>
    private async Task PreloadBiomes()
    {
        try
        {
            IsLoading = true;
            // Trigger initialization of the terrain caching service
            var biomes = await _terrainAssetService.GetLoadedBiomes();
            var biomeCount = biomes.Count();

            BiomeLoadingStatus = biomeCount == 0
                ? _localizationService.GetString("Status_NoBiomesFound")
                : string.Format(_localizationService.GetString("Status_BiomesLoaded"), biomeCount);

            if (biomeCount == 0)
                throw new Exception(_localizationService.GetString("Status_NoBiomesFound"));
        }
        catch (Exception ex)
        {
            HasError = true;
            BiomeLoadingStatus = string.Format(_localizationService.GetString("Status_ErrorLoadingBiomes"), ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
