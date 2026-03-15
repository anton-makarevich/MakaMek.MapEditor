using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Map.Data;
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

    private string _biomeLoadingStatus = string.Empty;
    private bool _hasError;
    private bool _isLoading;

    public string BiomeLoadingStatus
    {
        get => _biomeLoadingStatus;
        private set => SetProperty(ref _biomeLoadingStatus, value);
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public MainMenuViewModel(IFileService fileService, IBattleMapFactory mapFactory, ILogger<MainMenuViewModel> logger, ITerrainAssetService terrainAssetService)
    {
        _fileService = fileService;
        _mapFactory = mapFactory;
        _logger = logger;
        _terrainAssetService = terrainAssetService;
        
        // Initialize preloading
        _ = PreloadBiomes();
    }

    public IAsyncCommand CreateNewMapCommand => field ??= new AsyncCommand(() => 
        NavigationService.NavigateToViewModelAsync<NewMapViewModel>());

    public IAsyncCommand LoadMapCommand => field ??= new AsyncCommand(async () =>
    {
        try
        {
            var content = (await _fileService.OpenFile("Load Map")).Content;
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
                ? "No biomes found"
                : $"{biomeCount} biomes loaded";

            if (biomeCount == 0)
                throw new Exception("No biomes found");
        }
        catch (Exception ex)
        {
            HasError = true;
            BiomeLoadingStatus = $"Error loading biomes: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
