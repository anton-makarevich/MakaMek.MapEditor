using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Generators;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MVVM.Core.ViewModels;

namespace MakaMek.MapEditor.ViewModels;

public class NewMapViewModel : BaseViewModel
{
    private readonly IBattleMapFactory _mapFactory;

    public NewMapViewModel(IBattleMapFactory mapFactory)
    {
        _mapFactory = mapFactory;
    }

    public int MapWidth
    {
        get;
        set => SetProperty(ref field, value);
    } = 15;

    public int MapHeight
    {
        get;
        set => SetProperty(ref field, value);
    } = 17;

    public bool IsPreGenerated
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int ForestCoverage
    {
        get;
        init => SetProperty(ref field, value);
    } = 20;

    public int LightWoodsPercentage
    {
        get;
        init => SetProperty(ref field, value);
    } = 30;

    public ICommand CreateMapCommand => field ??= new AsyncCommand(async () =>
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
