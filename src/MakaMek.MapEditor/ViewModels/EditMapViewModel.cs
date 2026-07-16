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
using Sanet.MVVM.Core.Models;
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
            NotifyPropertyChanged(nameof(IsCursorActive));
            if (value != ToolType.Cursor)
            {
                IsHexInfoVisible = false;
                HexViewModel = null;
                _currentHex = null;
            }
        }
    } = ToolType.Terrain;

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

    private Hex? _currentHex;
    public event Action<Hex>? HexUpdated;

    public IAsyncCommand RaiseLevelCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;

        var updatedHex = ReplaceHexWithNewLevel(_currentHex, _currentHex.Level + 1);
        if (updatedHex == null) return Task.CompletedTask;

        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public IAsyncCommand LowerLevelCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;

        var updatedHex = ReplaceHexWithNewLevel(_currentHex, _currentHex.Level - 1);
        if (updatedHex == null) return Task.CompletedTask;

        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public IAsyncCommand IncreaseWaterDepthCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;
        if (!_currentHex.HasTerrain(MakaMekTerrains.Water)) return Task.CompletedTask;

        var updatedHex = UpdateHexWithNewWaterDepth(_currentHex, -1);
        if (updatedHex == null) return Task.CompletedTask;

        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public IAsyncCommand DecreaseWaterDepthCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;
        if (!_currentHex.HasTerrain(MakaMekTerrains.Water)) return Task.CompletedTask;

        var updatedHex = UpdateHexWithNewWaterDepth(_currentHex, 1);
        if (updatedHex == null) return Task.CompletedTask;

        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public IAsyncCommand RaiseBridgeLevelCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;

        var bridge = _currentHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        var road = _currentHex.GetTerrain(MakaMekTerrains.Road);

        if (bridge == null && road == null) return Task.CompletedTask;

        Hex updatedHex;
        if (road != null)
        {
            updatedHex = new Hex(_currentHex.Coordinates, _currentHex.Level);
            foreach (var terrain in _currentHex.GetTerrains())
            {
                if (terrain is RoadTerrain) continue;
                updatedHex.AddTerrain(terrain);
            }
            updatedHex.AddTerrain(new BridgeTerrain(1, DefaultConstructionFactor));
        }
        else
        {
            updatedHex = new Hex(_currentHex.Coordinates, _currentHex.Level);
            foreach (var terrain in _currentHex.GetTerrains())
            {
                if (terrain is BridgeTerrain)
                    updatedHex.AddTerrain(new BridgeTerrain(bridge!.Height + 1, bridge.ConstructionFactor));
                else
                    updatedHex.AddTerrain(terrain);
            }
        }

        Map.AddHex(updatedHex);
        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public IAsyncCommand LowerBridgeLevelCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;

        var bridge = _currentHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        if (bridge == null) return Task.CompletedTask;

        var newHeight = bridge.Height - 1;

        Hex updatedHex;
        if (newHeight <= 0 && !_currentHex.HasTerrain(MakaMekTerrains.Water))
        {
            updatedHex = new Hex(_currentHex.Coordinates, _currentHex.Level);
            foreach (var terrain in _currentHex.GetTerrains())
            {
                if (terrain is BridgeTerrain) continue;
                updatedHex.AddTerrain(terrain);
            }
            updatedHex.AddTerrain(new RoadTerrain());
        }
        else
        {
            updatedHex = new Hex(_currentHex.Coordinates, _currentHex.Level);
            foreach (var terrain in _currentHex.GetTerrains())
            {
                if (terrain is BridgeTerrain)
                    updatedHex.AddTerrain(new BridgeTerrain(Math.Max(DefaultBridgeHeight, newHeight), bridge.ConstructionFactor));
                else
                    updatedHex.AddTerrain(terrain);
            }
        }

        Map.AddHex(updatedHex);
        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public IAsyncCommand IncreaseConstructionFactorCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;

        var bridge = _currentHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        if (bridge == null) return Task.CompletedTask;

        var newCf = Math.Min(MaxConstructionFactor, bridge.ConstructionFactor + ConstructionFactorStep);
        var updatedHex = new Hex(_currentHex.Coordinates, _currentHex.Level);
        foreach (var terrain in _currentHex.GetTerrains())
        {
            if (terrain is BridgeTerrain)
                updatedHex.AddTerrain(new BridgeTerrain(bridge.Height, newCf));
            else
                updatedHex.AddTerrain(terrain);
        }

        Map.AddHex(updatedHex);
        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public IAsyncCommand DecreaseConstructionFactorCommand => field ??= new AsyncCommand(() =>
    {
        if (_currentHex == null) return Task.CompletedTask;
        if (Map == null) return Task.CompletedTask;

        var bridge = _currentHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        if (bridge == null) return Task.CompletedTask;

        var newCf = Math.Max(MinConstructionFactor, bridge.ConstructionFactor - ConstructionFactorStep);
        var updatedHex = new Hex(_currentHex.Coordinates, _currentHex.Level);
        foreach (var terrain in _currentHex.GetTerrains())
        {
            if (terrain is BridgeTerrain)
                updatedHex.AddTerrain(new BridgeTerrain(bridge.Height, newCf));
            else
                updatedHex.AddTerrain(terrain);
        }

        Map.AddHex(updatedHex);
        _currentHex = updatedHex;
        HexViewModel?.UpdateFromHex(updatedHex);
        HexUpdated?.Invoke(updatedHex);
        return Task.CompletedTask;
    });

    public virtual void Initialize(IBattleMap map)
    {
        Map = map;
        LoadTerrains();
    }

    // IMPORTANT: When adding new Terrain subclasses, manually add them here.
    // This list replaces reflection-based discovery for WASM compatibility.
    public const int DefaultBridgeHeight = 0;
    public const int DefaultConstructionFactor = 60;
    public const int MinConstructionFactor = 0;
    public const int MaxConstructionFactor = 500;
    public const int ConstructionFactorStep = 5;

    private static readonly IReadOnlyList<Terrain> KnownTerrains =
    [
        new ClearTerrain(),
        new LightWoodsTerrain(),
        new HeavyWoodsTerrain(),
        new RoughTerrain(),
        new WaterTerrain(),
        new RoadTerrain()
    ];

    private const string AssetBaseUri = "avares://Sanet.MakaMek.MapEditor/Assets";

    private void LoadTerrains()
    {
        AvailableTerrains.Clear();
        AvailableTools.Clear();

        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_Cursor"), ToolType.Cursor,
            imagePath: $"{AssetBaseUri}/tools/cursor.png"));

        foreach (var terrain in KnownTerrains)
        {
            AvailableTerrains.Add(terrain);
            AvailableTools.Add(new ToolItem(terrain.Id.ToString(), ToolType.Terrain, terrain,
                imagePath: $"{AssetBaseUri}/terrain/{terrain.Id.ToString().ToLowerInvariant()}.png"));
        }

        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_RaiseLevel"), ToolType.RaiseLevel));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_LowerLevel"), ToolType.LowerLevel));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_IncreaseWaterDepth"), ToolType.IncreaseWaterDepth));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_DecreaseWaterDepth"), ToolType.DecreaseWaterDepth));
        AvailableTools.Add(new ToolItem(LocalizationService.GetString("EditMap_RoadBridge"), ToolType.RoadBridge,
            imagePath: $"{AssetBaseUri}/terrain/road.png"));

        SelectedTerrain = AvailableTerrains.FirstOrDefault();
        SelectedTool = AvailableTools.FirstOrDefault(t => t.Type == ToolType.Terrain && t.Terrain == SelectedTerrain);
    }

    /// <summary>
    /// Handles hex selection based on the current edit mode.
    /// In Terrain mode, replaces the hex's terrains and returns null.
    /// In Cursor mode, populates the hex info popup and tracks the current hex.
    /// </summary>
    /// <summary>
    /// Handles hex selection based on the current edit mode.
    /// In Terrain mode, applies the selected terrain with correct layering rules.
    /// In Cursor mode, populates the hex info popup and tracks the current hex.
    /// </summary>
    public Hex? HandleHexSelection(Hex hex)
    {
        switch (ActiveEditMode)
        {
            case ToolType.Terrain:
                if (SelectedTerrain == null) return null;
                return ApplyTerrainToHex(hex, SelectedTerrain);

            case ToolType.Cursor:
                if (HexViewModel == null)
                    HexViewModel = new HexViewModel(hex);
                else
                    HexViewModel.UpdateFromHex(hex);
                _currentHex = hex;
                IsHexInfoVisible = true;
                return null;

            case ToolType.RaiseLevel:
                return ReplaceHexWithNewLevel(hex, hex.Level + 1);

            case ToolType.LowerLevel:
                return ReplaceHexWithNewLevel(hex, hex.Level - 1);

            case ToolType.IncreaseWaterDepth:
                return UpdateHexWithNewWaterDepth(hex, -1);

            case ToolType.DecreaseWaterDepth:
                return UpdateHexWithNewWaterDepth(hex, 1);

            case ToolType.RoadBridge:
                if (Map == null) return null;
                if (hex.HasTerrain(MakaMekTerrains.Bridge)) return hex;
                if (hex.HasTerrain(MakaMekTerrains.Road)) return hex;
                return ApplyTerrainToHex(hex,
                    hex.HasTerrain(MakaMekTerrains.Water)
                        ? new BridgeTerrain(DefaultBridgeHeight, DefaultConstructionFactor)
                        : new RoadTerrain());

            default:
                return null;
        }
    }

    private static bool IsGroundTerrain(MakaMekTerrains id) =>
        id is MakaMekTerrains.LightWoods
            or MakaMekTerrains.HeavyWoods or MakaMekTerrains.Rough
            or MakaMekTerrains.Pavement or MakaMekTerrains.Rubble;

    /// <summary>
    /// Applies a terrain to a hex with correct layering:
    /// - Ground terrains (Clear/Woods/Rough/Pavement/Rubble) are mutually exclusive with each other
    /// - Water coexists with ground and road layers; adding Water over Road converts Road to Bridge
    /// - Road/Bridge sits on top and coexists with ground + water layers
    /// </summary>
    private static Hex ApplyTerrainToHex(Hex hex, Terrain terrain)
    {
        var newId = terrain.Id;

        if (newId == MakaMekTerrains.Clear)
        {
            foreach (var t in hex.GetTerrains().ToList())
                hex.RemoveTerrain(t.Id);
            hex.AddTerrain(terrain);
            return hex;
        }

        if (IsGroundTerrain(newId))
        {
            
            var existing = hex.GetTerrains().FirstOrDefault(t => IsGroundTerrain(t.Id));
            if (existing != null)
                hex.RemoveTerrain(existing.Id);
            hex.AddTerrain(terrain);
            return hex;
        }

        if (newId == MakaMekTerrains.Water)
        {
            hex.RemoveTerrain(MakaMekTerrains.Water);
            if (hex.HasTerrain(MakaMekTerrains.Road))
            {
                hex.RemoveTerrain(MakaMekTerrains.Road);
                hex.AddTerrain(new BridgeTerrain(DefaultBridgeHeight, DefaultConstructionFactor));
            }
            hex.AddTerrain(terrain);
            return hex;
        }

        if (newId == MakaMekTerrains.Road)
        {
            if (hex.HasTerrain(MakaMekTerrains.Bridge))
                return hex;
            hex.RemoveTerrain(MakaMekTerrains.Road);
            if (hex.HasTerrain(MakaMekTerrains.Water))
                hex.AddTerrain(new BridgeTerrain(DefaultBridgeHeight, DefaultConstructionFactor));
            else
                hex.AddTerrain(terrain);
            return hex;
        }

        if (newId == MakaMekTerrains.Bridge)
        {
            hex.RemoveTerrain(MakaMekTerrains.Road);
            hex.RemoveTerrain(MakaMekTerrains.Bridge);
            if (hex.HasTerrain(MakaMekTerrains.Water) && terrain is BridgeTerrain bt)
                hex.AddTerrain(new BridgeTerrain(bt.Height, bt.ConstructionFactor));
            else
                hex.AddTerrain(new RoadTerrain());
            return hex;
        }

        var oldGround = hex.GetTerrains().FirstOrDefault(t => IsGroundTerrain(t.Id));
        if (oldGround != null)
            hex.RemoveTerrain(oldGround.Id);
        hex.AddTerrain(terrain);
        return hex;
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

    public Func<Task<(byte[] PngBytes, int WidthPixels, int HeightPixels)>>? CaptureMap { get; set; }

    public async Task ExportMapAsPdf()
    {
        if (CaptureMap == null) return;
        try
        {
            var (pngBytes, width, height) = await CaptureMap();
            await ExportMapAsPdf(pngBytes, width, height);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to capture and export map as PDF");
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

    private bool _isMenuVisible;
    public bool IsMenuVisible
    {
        get => _isMenuVisible;
        set => SetProperty(ref _isMenuVisible, value);
    }

    public IAsyncCommand ToggleMenuCommand => field ??= new AsyncCommand(() =>
    {
        IsMenuVisible = !IsMenuVisible;
        return Task.CompletedTask;
    });

    public IAsyncCommand CloseMenuCommand => field ??= new AsyncCommand(() =>
    {
        IsMenuVisible = false;
        return Task.CompletedTask;
    });

    public IAsyncCommand CloseEditMapCommand => field ??= new AsyncCommand(async () =>
    {
        var yesAction = new UiAction { Title = LocalizationService.GetString("Dialog_Yes") };
        var noAction = new UiAction { Title = LocalizationService.GetString("Dialog_No") };

        var selectedAction = await NavigationService.AskForActionAsync(
            LocalizationService.GetString("EditMap_CloseConfirmTitle"),
            LocalizationService.GetString("EditMap_CloseConfirmMessage"),
            yesAction,
            noAction);

        if (selectedAction != yesAction)
            return;

        IsMenuVisible = false;
        await NavigationService.NavigateBackAsync();
    }, onException: ex => Logger.LogError(ex, "Failed to close map"));

    public IAsyncCommand CloseHexInfoCommand => field ??= new AsyncCommand(() =>
    {
        IsHexInfoVisible = false;
        HexViewModel = null;
        _currentHex = null;
        return Task.CompletedTask;
    });

    public IAsyncCommand OpenAboutCommand => field ??= new AsyncCommand(async () =>
    {
        await NavigationService.NavigateToViewModelAsync<AboutViewModel>();
    });
}
