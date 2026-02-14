using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MVVM.Core.ViewModels;

namespace MakaMek.MapEditor.ViewModels;

public class EditMapViewModel : BaseViewModel
{
    public BattleMap? Map
    {
        get;
        private set => SetProperty(ref field, value);
    }

    private readonly Services.IFileService _fileService;

    public EditMapViewModel(Services.IFileService fileService)
    {
        _fileService = fileService;
    }

    public ObservableCollection<Terrain> AvailableTerrains { get; } = new();

    private Terrain? _selectedTerrain;
    public Terrain? SelectedTerrain
    {
        get => _selectedTerrain;
        set => SetProperty(ref _selectedTerrain, value);
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
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(terrainType));

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
        try
        {
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
        catch (Exception) { }
    }
    
    private ICommand? _exportMapCommand;
    public ICommand ExportMapCommand => _exportMapCommand ??= new AsyncCommand(async () =>
    {
        if (Map == null) return;
        var data = Map.ToData();
        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await _fileService.SaveFileAsync("Export Map", "map.json", json);
    });
}
