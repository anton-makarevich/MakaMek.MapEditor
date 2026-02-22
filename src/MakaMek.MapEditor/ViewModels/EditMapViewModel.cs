using System.Collections.ObjectModel;
using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class EditMapViewModel : BaseViewModel
{
    public IBattleMap? Map
    {
        get;
        private set => SetProperty(ref field, value);
    }

    private readonly IFileService _fileService;
    public IImageService ImageService { get; }

    public EditMapViewModel(IFileService fileService, IImageService imageService, ILogger<EditMapViewModel> logger)
    {
        _fileService = fileService;
        ImageService = imageService;
        Logger = logger;
    }
    
    public ILogger<EditMapViewModel> Logger { get; }

    public ObservableCollection<Terrain> AvailableTerrains { get; } = new();

    public Terrain? SelectedTerrain
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void Initialize(IBattleMap map)
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
        hex.ReplaceTerrains([SelectedTerrain]);
    }

    public IAsyncCommand ExportMapCommand => field ??= new AsyncCommand(async () =>
    {
        if (Map == null) return;
        var data = Map.ToData();
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await _fileService.SaveFile("Export Map", "map.json", json);
    });
}
