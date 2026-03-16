using System.Collections.ObjectModel;
using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Assets.Services;
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
    public ITerrainAssetService AssetService { get; }

    public EditMapViewModel(IFileService fileService, ITerrainAssetService assetService, ILogger<EditMapViewModel> logger)
    {
        _fileService = fileService;
        AssetService = assetService;
        Logger = logger;
    }
    
    public ILogger<EditMapViewModel> Logger { get; }

    public ObservableCollection<Terrain> AvailableTerrains { get; } = [];

    public Terrain? SelectedTerrain
    {
        get;
        set => SetProperty(ref field, value);
    }

    public EditMode ActiveEditMode
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(IsRaiseLevelActive));
            NotifyPropertyChanged(nameof(IsLowerLevelActive));
        }
    } = EditMode.Terrain;

    public bool IsRaiseLevelActive => ActiveEditMode == EditMode.RaiseLevel;
    public bool IsLowerLevelActive => ActiveEditMode == EditMode.LowerLevel;

    public IAsyncCommand RaiseLevelCommand => field ??= new AsyncCommand(() =>
    {
        ActiveEditMode = ActiveEditMode == EditMode.RaiseLevel ? EditMode.Terrain : EditMode.RaiseLevel;
        return Task.CompletedTask;
    });

    public IAsyncCommand LowerLevelCommand => field ??= new AsyncCommand(() =>
    {
        ActiveEditMode = ActiveEditMode == EditMode.LowerLevel ? EditMode.Terrain : EditMode.LowerLevel;
        return Task.CompletedTask;
    });

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

    /// <summary>
    /// Handles hex selection based on the current edit mode.
    /// In Terrain mode, replaces the hex's terrains and returns null.
    /// In RaiseLevel/LowerLevel mode, creates a new immutable Hex with the changed level,
    /// replaces it in the map, and returns the new Hex.
    /// </summary>
    public Hex? HandleHexSelection(Hex hex)
    {
        switch (ActiveEditMode)
        {
            case EditMode.Terrain:
                if (SelectedTerrain == null) return null;
                hex.ReplaceTerrains([SelectedTerrain]);
                return null;

            case EditMode.RaiseLevel:
                return ReplaceHexWithNewLevel(hex, hex.Level + 1);

            case EditMode.LowerLevel:
                return ReplaceHexWithNewLevel(hex, hex.Level - 1);

            default:
                return null;
        }
    }

    private Hex? ReplaceHexWithNewLevel(Hex oldHex, int newLevel)
    {
        if (Map == null) return null;

        var newHex = new Hex(oldHex.Coordinates, newLevel);
        foreach (var terrain in oldHex.GetTerrains())
        {
            newHex.AddTerrain(terrain);
        }

        Map.AddHex(newHex);
        return newHex;
    }

    /// <summary>
    /// Returns edge update data for all valid on-map neighbors of the given coordinates.
    /// Used by the view to update neighbor HexControls after a level change.
    /// </summary>
    public IEnumerable<(HexCoordinates Coordinates, IReadOnlyList<HexEdge> Edges)> GetEdgeUpdatesForNeighbors(
        HexCoordinates coordinates)
    {
        if (Map == null) yield break;

        foreach (var neighborCoords in coordinates.GetAllNeighbours())
        {
            if (!Map.IsOnMap(neighborCoords)) continue;
            var edges = Map.GetHexEdges(neighborCoords);
            yield return (neighborCoords, edges);
        }
    }

    public IAsyncCommand ExportMapCommand => field ??= new AsyncCommand(async () =>
    {
        if (Map == null) return;
        var data = Map.ToData();
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await _fileService.SaveFile("Export Map", "map.json", json);
    }, onException: ex => Logger.LogError(ex, "Failed to export map"));
}
