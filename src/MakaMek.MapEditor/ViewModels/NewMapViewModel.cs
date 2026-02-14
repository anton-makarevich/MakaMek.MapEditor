using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Generators;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MVVM.Core.ViewModels;
using Sanet.MVVM.Core.Services;

namespace MakaMek.MapEditor.ViewModels;

public class NewMapViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IBattleMapFactory _mapFactory;

    public NewMapViewModel(INavigationService navigationService, IBattleMapFactory mapFactory)
    {
        _navigationService = navigationService;
        _mapFactory = mapFactory;
    }

    private int _mapWidth = 15;
    public int MapWidth
    {
        get => _mapWidth;
        set => SetProperty(ref _mapWidth, value);
    }

    private int _mapHeight = 17;
    public int MapHeight
    {
        get => _mapHeight;
        set => SetProperty(ref _mapHeight, value);
    }

    private bool _isPreGenerated;
    public bool IsPreGenerated
    {
        get => _isPreGenerated;
        set => SetProperty(ref _isPreGenerated, value);
    }

    private int _forestCoverage = 20;
    public int ForestCoverage
    {
        get => _forestCoverage;
        set => SetProperty(ref _forestCoverage, value);
    }

    private int _lightWoodsPercentage = 30;
    public int LightWoodsPercentage
    {
        get => _lightWoodsPercentage;
        set => SetProperty(ref _lightWoodsPercentage, value);
    }

    private ICommand? _createMapCommand;
    public ICommand CreateMapCommand => _createMapCommand ??= new AsyncCommand(async () =>
    {
        ITerrainGenerator generator = !IsPreGenerated
            ? new SingleTerrainGenerator(MapWidth, MapHeight, new ClearTerrain())
            : new ForestPatchesGenerator(
                MapWidth,
                MapHeight,
                forestCoverage: ForestCoverage / 100.0,
                lightWoodsProbability: LightWoodsPercentage / 100.0);

        var map = _mapFactory.GenerateMap(MapWidth, MapHeight, generator);

        var editViewModel = _navigationService.GetViewModel<EditMapViewModel>();
        if (editViewModel != null)
        {
            editViewModel.Initialize(map);
            await _navigationService.NavigateToViewModelAsync(editViewModel);
        }
    });
}
