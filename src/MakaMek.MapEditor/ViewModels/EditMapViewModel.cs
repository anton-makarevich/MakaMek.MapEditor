using System.Collections.ObjectModel;
using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.Map.Services;
using Sanet.MakaMek.MapEditor.Models;
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

    public ITerrainBitmaskService TerrainBitmaskService { get; }

    public EditMapViewModel(IFileService fileService,
        ITerrainAssetService assetService,
        ILocalizationService localizationService,
        ILogger<EditMapViewModel> logger,
        ITerrainBitmaskService terrainBitmaskService)
    {
        _fileService = fileService;
        AssetService = assetService;
        LocalizationService = localizationService;
        Logger = logger;
        TerrainBitmaskService = terrainBitmaskService;
    }
    
    public ILogger<EditMapViewModel> Logger { get; }

    public ObservableCollection<Terrain> AvailableTerrains { get; } = [];

    public ObservableCollection<ToolItem> AvailableTools { get; } = [];

    public Terrain? SelectedTerrain
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ActiveEditMode = ToolType.Terrain;
        }
    }

    public ToolType ActiveEditMode
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(IsRaiseLevelActive));
            NotifyPropertyChanged(nameof(IsLowerLevelActive));
            NotifyPropertyChanged(nameof(IsIncreaseWaterDepthActive));
            NotifyPropertyChanged(nameof(IsDecreaseWaterDepthActive));
        }
    } = ToolType.Terrain;

    public bool IsRaiseLevelActive => ActiveEditMode == ToolType.RaiseLevel;
    public bool IsLowerLevelActive => ActiveEditMode == ToolType.LowerLevel;
    public bool IsIncreaseWaterDepthActive => ActiveEditMode == ToolType.IncreaseWaterDepth;
    public bool IsDecreaseWaterDepthActive => ActiveEditMode == ToolType.DecreaseWaterDepth;

    private ToolItem? _selectedTool;
    public ToolItem? SelectedTool
    {
        get => _selectedTool;
        set
        {
            SetProperty(ref _selectedTool, value);
            if (value == null) return;

            ActiveEditMode = value.Type;
            if (value.Type == ToolType.Terrain)
                SelectedTerrain = value.Terrain;
        }
    }

    public IAsyncCommand RaiseLevelCommand => field ??= new AsyncCommand(() =>
    {
        if (AvailableTools.Count == 0)
        {
            ActiveEditMode = ActiveEditMode == ToolType.RaiseLevel ? ToolType.Terrain : ToolType.RaiseLevel;
            return Task.CompletedTask;
        }
        SelectedTool = ActiveEditMode == ToolType.RaiseLevel
            ? AvailableTools.FirstOrDefault(t => t.Type == ToolType.Terrain && t.Terrain == SelectedTerrain)
            : AvailableTools.FirstOrDefault(t => t.Type == ToolType.RaiseLevel);
        return Task.CompletedTask;
    });

    public IAsyncCommand LowerLevelCommand => field ??= new AsyncCommand(() =>
    {
        if (AvailableTools.Count == 0)
        {
            ActiveEditMode = ActiveEditMode == ToolType.LowerLevel ? ToolType.Terrain : ToolType.LowerLevel;
            return Task.CompletedTask;
        }
        SelectedTool = ActiveEditMode == ToolType.LowerLevel
            ? AvailableTools.FirstOrDefault(t => t.Type == ToolType.Terrain && t.Terrain == SelectedTerrain)
            : AvailableTools.FirstOrDefault(t => t.Type == ToolType.LowerLevel);
        return Task.CompletedTask;
    });

    public IAsyncCommand IncreaseWaterDepthCommand => field ??= new AsyncCommand(() =>
    {
        if (AvailableTools.Count == 0)
        {
            ActiveEditMode = ActiveEditMode == ToolType.IncreaseWaterDepth ? ToolType.Terrain : ToolType.IncreaseWaterDepth;
            return Task.CompletedTask;
        }
        SelectedTool = ActiveEditMode == ToolType.IncreaseWaterDepth
            ? AvailableTools.FirstOrDefault(t => t.Type == ToolType.Terrain && t.Terrain == SelectedTerrain)
            : AvailableTools.FirstOrDefault(t => t.Type == ToolType.IncreaseWaterDepth);
        return Task.CompletedTask;
    });

    public IAsyncCommand DecreaseWaterDepthCommand => field ??= new AsyncCommand(() =>
    {
        if (AvailableTools.Count == 0)
        {
            ActiveEditMode = ActiveEditMode == ToolType.DecreaseWaterDepth ? ToolType.Terrain : ToolType.DecreaseWaterDepth;
            return Task.CompletedTask;
        }
        SelectedTool = ActiveEditMode == ToolType.DecreaseWaterDepth
            ? AvailableTools.FirstOrDefault(t => t.Type == ToolType.Terrain && t.Terrain == SelectedTerrain)
            : AvailableTools.FirstOrDefault(t => t.Type == ToolType.DecreaseWaterDepth);
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
        AvailableTools.Clear();

        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_RaiseLevel"), ToolType.RaiseLevel));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_LowerLevel"), ToolType.LowerLevel));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_IncreaseWaterDepth"), ToolType.IncreaseWaterDepth));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_DecreaseWaterDepth"), ToolType.DecreaseWaterDepth));

        var terrainType = typeof(Terrain);
        var assembly = terrainType.Assembly;
        var terrainTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(terrainType));

        foreach (var type in terrainTypes)
        {
            if (Activator.CreateInstance(type) is Terrain terrain)
            {
                AvailableTerrains.Add(terrain);
                AvailableTools.Add(new ToolItem(terrain.Id.ToString(), ToolType.Terrain, terrain));
            }
        }

        SelectedTerrain = AvailableTerrains.FirstOrDefault();
        SelectedTool = AvailableTools.FirstOrDefault(t => t.Type == ToolType.Terrain && t.Terrain == SelectedTerrain);
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
            case ToolType.Terrain:
                if (SelectedTerrain == null) return null;
                hex.ReplaceTerrains([SelectedTerrain]);
                return null;

            case ToolType.RaiseLevel:
                return ReplaceHexWithNewLevel(hex, hex.Level + 1);

            case ToolType.LowerLevel:
                return ReplaceHexWithNewLevel(hex, hex.Level - 1);

            case ToolType.IncreaseWaterDepth:
                return UpdateHexWithNewWaterDepth(hex, -1);

            case ToolType.DecreaseWaterDepth:
                return UpdateHexWithNewWaterDepth(hex, 1);

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

    private Hex? UpdateHexWithNewWaterDepth(Hex hex, int depthDelta)
    {
        if (Map == null) return null;

        var waterTerrain = hex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        if (waterTerrain == null)
            return hex;
        var newDepth = waterTerrain.Height + depthDelta;
        if (newDepth > 0)
            return hex;
        hex.RemoveTerrain(MakaMekTerrains.Water);
        hex.AddTerrain(new WaterTerrain(newDepth));

        return hex;
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
        await _fileService.SaveFile(LocalizationService.GetString("EditMap_ExportMapDialogTitle"), "map.json", json);
    }, onException: ex => Logger.LogError(ex, "Failed to export map"));

    public ILocalizationService LocalizationService { get; }
}
