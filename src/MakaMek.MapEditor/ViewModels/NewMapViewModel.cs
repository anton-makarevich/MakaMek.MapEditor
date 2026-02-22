using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Generators;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class NewMapViewModel : BaseViewModel
{
    private readonly IBattleMapFactory _mapFactory;

    public NewMapViewModel(IBattleMapFactory mapFactory)
    {
        _mapFactory = mapFactory;
    }

    public int MapWidthMin => 5;
    public int MapWidthMax => 30;
    
    public int MapHeightMin => 6;
    public int MapHeightMax => 34;
    
    public int MapWidth
    {
        get;
        set => SetProperty(ref field, Math.Clamp(value, MapWidthMin, MapWidthMax));
    } = 15;

    public int MapHeight
    {
        get;
        set => SetProperty(ref field, Math.Clamp(value, MapHeightMin, MapHeightMax));
    } = 17;

    public bool IsPreGenerated
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int ForestCoverage
    {
        get;
        set => SetProperty(ref field, value);
    } = 20;

    public int LightWoodsPercentage
    {
        get;
        set => SetProperty(ref field, value);
    } = 30;

    public IAsyncCommand CreateMapCommand => field ??= new AsyncCommand(async () =>
    {
        ITerrainGenerator generator = !IsPreGenerated
            ? new SingleTerrainGenerator(MapWidth, MapHeight, new ClearTerrain())
            : new ForestPatchesGenerator(
                MapWidth,
                MapHeight,
                forestCoverage: ForestCoverage / 100.0,
                lightWoodsProbability: LightWoodsPercentage / 100.0);

        var map = _mapFactory.GenerateMap(MapWidth, MapHeight, generator);

        var editViewModel = NavigationService.GetViewModel<EditMapViewModel>();
        if (editViewModel != null)
        {
            editViewModel.Initialize(map);
            await NavigationService.NavigateToViewModelAsync(editViewModel);
        }
    });
}
