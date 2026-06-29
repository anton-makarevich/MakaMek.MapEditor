using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.Map.Services;
using Sanet.MakaMek.MapEditor.Models;
using Sanet.MakaMek.MapEditor.ViewModels.Wrappers;
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
    private readonly IPdfExportService _pdfExportService;
    public ITerrainAssetService AssetService { get; }

    public ITerrainBitmaskService TerrainBitmaskService { get; }
    
    public IScheduler? Scheduler { get; }

    private readonly PropertyChangedEventHandler? _hexConfigurationChangedHandler;

    public HexRenderConfigurationViewModel HexConfiguration { get; }

    public EditMapViewModel(IFileService fileService,
        IPdfExportService pdfExportService,
        ITerrainAssetService assetService,
        ILocalizationService localizationService,
        ILogger<EditMapViewModel> logger,
        ITerrainBitmaskService terrainBitmaskService,
        IScheduler? scheduler)
    {
        _fileService = fileService;
        _pdfExportService = pdfExportService;
        AssetService = assetService;
        LocalizationService = localizationService;
        Logger = logger;
        TerrainBitmaskService = terrainBitmaskService;
        Scheduler = scheduler;
        HexConfiguration = new HexRenderConfigurationViewModel();
        _hexConfigurationChangedHandler = (_, _) => NotifyPropertyChanged(nameof(HexConfiguration));
        HexConfiguration.PropertyChanged += _hexConfigurationChangedHandler;
    }

    public override void DetachHandlers()
    {
        base.DetachHandlers();
        HexConfiguration.PropertyChanged -= _hexConfigurationChangedHandler;
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
            if (field == value) return;
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(IsRaiseLevelActive));
            NotifyPropertyChanged(nameof(IsLowerLevelActive));
            NotifyPropertyChanged(nameof(IsIncreaseWaterDepthActive));
            NotifyPropertyChanged(nameof(IsDecreaseWaterDepthActive));
            NotifyPropertyChanged(nameof(IsCursorActive));
            if (value != ToolType.Cursor)
            {
                IsHexInfoVisible = false;
                HexViewModel = null;
            }
        }
    } = ToolType.Terrain;

    public bool IsRaiseLevelActive => ActiveEditMode == ToolType.RaiseLevel;
    public bool IsLowerLevelActive => ActiveEditMode == ToolType.LowerLevel;
    public bool IsIncreaseWaterDepthActive => ActiveEditMode == ToolType.IncreaseWaterDepth;
    public bool IsDecreaseWaterDepthActive => ActiveEditMode == ToolType.DecreaseWaterDepth;
    public bool IsCursorActive => ActiveEditMode == ToolType.Cursor;

    public HexViewModel? HexViewModel
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public bool IsHexInfoVisible
    {
        get;
        set => SetProperty(ref field, value);
    }

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
            ActiveEditMode = ToolType.RaiseLevel;
            return Task.CompletedTask;
        }
        var raiseTool = AvailableTools.FirstOrDefault(t => t.Type == ToolType.RaiseLevel);
        if (raiseTool != null)
            SelectedTool = raiseTool;
        else
            ActiveEditMode = ToolType.RaiseLevel;
        return Task.CompletedTask;
    });

    public IAsyncCommand LowerLevelCommand => field ??= new AsyncCommand(() =>
    {
        if (AvailableTools.Count == 0)
        {
            ActiveEditMode = ToolType.LowerLevel;
            return Task.CompletedTask;
        }
        var lowerTool = AvailableTools.FirstOrDefault(t => t.Type == ToolType.LowerLevel);
        if (lowerTool != null)
            SelectedTool = lowerTool;
        else
            ActiveEditMode = ToolType.LowerLevel;
        return Task.CompletedTask;
    });

    public IAsyncCommand IncreaseWaterDepthCommand => field ??= new AsyncCommand(() =>
    {
        if (AvailableTools.Count == 0)
        {
            ActiveEditMode = ToolType.IncreaseWaterDepth;
            return Task.CompletedTask;
        }
        var incTool = AvailableTools.FirstOrDefault(t => t.Type == ToolType.IncreaseWaterDepth);
        if (incTool != null)
            SelectedTool = incTool;
        else
            ActiveEditMode = ToolType.IncreaseWaterDepth;
        return Task.CompletedTask;
    });

    public IAsyncCommand DecreaseWaterDepthCommand => field ??= new AsyncCommand(() =>
    {
        if (AvailableTools.Count == 0)
        {
            ActiveEditMode = ToolType.DecreaseWaterDepth;
            return Task.CompletedTask;
        }
        var decTool = AvailableTools.FirstOrDefault(t => t.Type == ToolType.DecreaseWaterDepth);
        if (decTool != null)
            SelectedTool = decTool;
        else
            ActiveEditMode = ToolType.DecreaseWaterDepth;
        return Task.CompletedTask;
    });

    public virtual void Initialize(IBattleMap map)
    {
        Map = map;
        LoadTerrains();
    }

    // IMPORTANT: When adding new Terrain subclasses, manually add them here.
    // This list replaces reflection-based discovery for WASM compatibility.
    private static readonly IReadOnlyList<Terrain> KnownTerrains =
    [
        new ClearTerrain(),
        new LightWoodsTerrain(),
        new HeavyWoodsTerrain(),
        new RoughTerrain(),
        new WaterTerrain()
    ];

    private const string AssetBaseUri = "avares://Sanet.MakaMek.MapEditor/Assets";

    private void LoadTerrains()
    {
        AvailableTerrains.Clear();
        AvailableTools.Clear();

        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_RaiseLevel"), ToolType.RaiseLevel,
            imagePath: $"{AssetBaseUri}/tools/raise-level.png"));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_LowerLevel"), ToolType.LowerLevel,
            imagePath: $"{AssetBaseUri}/tools/lower-level.png"));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_IncreaseWaterDepth"), ToolType.IncreaseWaterDepth,
            imagePath: $"{AssetBaseUri}/tools/increase-depth.png"));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_DecreaseWaterDepth"), ToolType.DecreaseWaterDepth,
            imagePath: $"{AssetBaseUri}/tools/decrease-depth.png"));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_Cursor"), ToolType.Cursor,
            imagePath: $"{AssetBaseUri}/tools/cursor.png"));

        foreach (var terrain in KnownTerrains)
        {
            AvailableTerrains.Add(terrain);
            AvailableTools.Add(new ToolItem(terrain.Id.ToString(), ToolType.Terrain, terrain,
                imagePath: $"{AssetBaseUri}/terrain/{terrain.Id.ToString().ToLowerInvariant()}.png"));
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

            case ToolType.Cursor:
                if (HexViewModel == null)
                    HexViewModel = new HexViewModel(hex);
                else
                    HexViewModel.UpdateFromHex(hex);
                IsHexInfoVisible = true;
                return null;

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

    public async Task ExportMapAsPdf(byte[] pngBytes, int width, int height)
    {
        try
        {
            var pdfBytes = await _pdfExportService.GeneratePdfFromPngAsync(pngBytes, width, height);
            await _fileService.SaveBinaryFile(
                LocalizationService.GetString("EditMap_ExportPdfDialogTitle"),
                "map.pdf",
                pdfBytes,
                "pdf",
                LocalizationService.GetString("EditMap_PdfFilesFilter"));
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to export PDF");
        }
    }

    public IAsyncCommand ExportMapCommand => field ??= new AsyncCommand(async () =>
    {
        if (Map == null) return;
        var data = Map.ToData();
        var json = JsonSerializer.Serialize(data, options: new JsonSerializerOptions { WriteIndented = true });
        await _fileService.SaveJsonFile(LocalizationService.GetString("EditMap_ExportMapDialogTitle"), "map.json", json);
    }, onException: ex => Logger.LogError(ex, "Failed to export map"));

    public ILocalizationService LocalizationService { get; }

    private bool _isSettingsPanelVisible = true;
    public bool IsSettingsPanelVisible
    {
        get => _isSettingsPanelVisible;
        set => SetProperty(ref _isSettingsPanelVisible, value);
    }

    public IAsyncCommand ToggleSettingsPanelCommand => field ??= new AsyncCommand(() =>
    {
        IsSettingsPanelVisible = !IsSettingsPanelVisible;
        return Task.CompletedTask;
    });

    public IAsyncCommand CloseHexInfoCommand => field ??= new AsyncCommand(() =>
    {
        IsHexInfoVisible = false;
        HexViewModel = null;
        return Task.CompletedTask;
    });
}
