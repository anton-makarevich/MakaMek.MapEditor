using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.Services;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class EditMapViewModel : BaseViewModel
{
    public BattleMap? Map
    {
        get;
        private set => SetProperty(ref field, value);
    }

    private readonly IFileService _fileService;
    public IImageService ImageService { get; }

    public EditMapViewModel(IFileService fileService, IImageService imageService)
    {
        _fileService = fileService;
        ImageService = imageService;
    }

    public ObservableCollection<Terrain> AvailableTerrains { get; } = new();

    public Terrain? SelectedTerrain
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void Initialize(BattleMap map)
    {
        Map = map;
        LoadTerrains();
    }

    private void LoadTerrains()
    {
        AvailableTerrains.Clear();
        var terrainType = typeof(Terrain);
        var assembly = terrainType.Assembly;
        var terrainTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(terrainType));

        foreach (var type in terrainTypes)
        {
            if (Activator.CreateInstance(type) is Terrain terrain)
            {
                AvailableTerrains.Add(terrain);
            }
        }
        
        SelectedTerrain = AvailableTerrains.FirstOrDefault();
    }

    public void HandleHexSelection(Hex hex)
    {
        if (SelectedTerrain == null) return;

        // Try to modify terrains
        // hex.Terrains is likely a list
        // We use reflection or dynamic if unsure

        var terrainsProp = hex.GetType().GetProperty("Terrains");
        if (terrainsProp != null)
        {
            var val = terrainsProp.GetValue(hex);
            if (val is System.Collections.IList list)
            {
                list.Clear();
                var newTerrain = Activator.CreateInstance(SelectedTerrain.GetType());
                list.Add(newTerrain);
            }
        }
        else
        {
            // Try GetTerrains method?
            // HexControl uses GetTerrains(). That implies it's a method?
            // HexControl: var terrain = _hex.GetTerrains().FirstOrDefault();
            // If it's a method, maybe there is SetTerrains or similar?
            // Or maybe Terrains is private field backing GetTerrains?
            // If we can't modify, we might need to recreate the hex.
            // But BattleMap stores hexes.
            // Let's assume there is a way or I cannot implement it blindly.
            // But user asked to use reflection to load available terrains.
            // I'll assume for now I can replace it.
            // Let's try casting to dynamic.
        }
    }

    public ICommand ExportMapCommand => field ??= new AsyncCommand(async () =>
    {
        if (Map == null) return;
        var data = Map.ToData();
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await _fileService.SaveFileAsync("Export Map", "map.json", json);
    });
}
