using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.Map.Services;
using Sanet.MakaMek.MapEditor.Models;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.Models;
using Sanet.MVVM.Core.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class EditMapViewModelTests
{
    private readonly IFileService _fileService = Substitute.For<IFileService>();
    private readonly IPdfExportService _pdfExportService = Substitute.For<IPdfExportService>();
    private readonly ITerrainAssetService _assetService = Substitute.For<ITerrainAssetService>();
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly EditMapViewModel _sut;
    private readonly ILogger<EditMapViewModel> _logger = Substitute.For<ILogger<EditMapViewModel>>();
    private readonly ITerrainBitmaskService _bitmaskService = Substitute.For<ITerrainBitmaskService>();
    private readonly IScheduler  _scheduler = ImmediateScheduler.Instance;
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();

    public EditMapViewModelTests()
    {
        _localizationService.GetString(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());
        _sut = new EditMapViewModel(_fileService,
            _pdfExportService,
            _assetService,
            _localizationService,
            _logger,
            _bitmaskService,
            _scheduler);
        _sut.SetNavigationService(_navigationService);
    }

    private static BattleMapData CreateTestBattleMapData(int q = 0, int r = 0)
    {
        return new BattleMapData
        {
            Biome = "test",
            HexData =
            [
                new HexData
                {
                    Coordinates = new HexCoordinateData(q, r),
                    Terrains =
                    [
                        new TerrainData { Type = MakaMekTerrains.Clear }
                    ]
                }
            ]
        };
    }

    private static void SelectHexViaCursor(EditMapViewModel sut, Hex hex)
    {
        sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);
        sut.HandleHexSelection(hex);
    }

    [Fact]
    public void Map_DefaultValue_ShouldBeNull()
    {
        _sut.Map.ShouldBeNull();
    }

    [Fact]
    public void SelectedTerrain_DefaultValue_ShouldBeNull()
    {
        _sut.SelectedTerrain.ShouldBeNull();
    }

    [Fact]
    public void AvailableTerrains_ShouldBeEmpty_WhenNotInitialized()
    {
        _sut.AvailableTerrains.ShouldBeEmpty();
    }

    [Fact]
    public void Logger_ShouldBeAccessible()
    {
        _sut.Logger.ShouldBe(_logger);
    }

    [Fact]
    public void AssetService_ShouldBeAccessible()
    {
        _sut.AssetService.ShouldBe(_assetService);
    }

    [Fact]
    public void BitmaskService_ShouldBeSet()
    {
        _sut.TerrainBitmaskService.ShouldBe(_bitmaskService);
    }

    [Fact]
    public void LocalizationService_ShouldBeInitializedViaConstructor()
    {
        _sut.LocalizationService.ShouldNotBeNull();
        _sut.LocalizationService.ShouldBe(_localizationService);
    }

    [Fact]
    public void Scheduler_ShouldBeSet()
    {
        _sut.Scheduler.ShouldBe(_scheduler);
    }

    [Fact]
    public void Initialize_ShouldSetMap()
    {
        var map = new BattleMap(1,1);

        _sut.Initialize(map);

        _sut.Map.ShouldBe(map);
    }

    [Fact]
    public void Initialize_ShouldLoadTerrains()
    {
        var map = new BattleMap(1,1);

        _sut.Initialize(map);

        _sut.AvailableTerrains.ShouldNotBeEmpty();
    }

    [Fact]
    public void Initialize_ShouldSetFirstTerrainAsSelected()
    {
        var map = new BattleMap(1,1);

        _sut.Initialize(map);

        _sut.SelectedTerrain.ShouldNotBeNull();
        _sut.SelectedTerrain.ShouldBe(_sut.AvailableTerrains.First());
    }

    [Fact]
    public void Initialize_ShouldLoadAllConcreteTerrainTypes()
    {
        var map = new BattleMap(1,1);

        _sut.Initialize(map);

        _sut.AvailableTerrains.Count.ShouldBeGreaterThan(0);
        _sut.AvailableTerrains.All(t => t.GetType().IsAbstract == false).ShouldBeTrue();
    }

    [Fact]
    public void SelectedTerrain_WhenSet_ShouldUpdateValue()
    {
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        var newTerrain = new ClearTerrain();

        _sut.SelectedTerrain = newTerrain;

        _sut.SelectedTerrain.ShouldBe(newTerrain);
    }

    [Fact]
    public void HandleHexSelection_WhenSelectedTerrainIsNull_ShouldNotModifyHex()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        var initialTerrain = new ClearTerrain();
        hex.AddTerrain(initialTerrain);
        _sut.SelectedTerrain = null;

        _sut.HandleHexSelection(hex);

        hex.GetTerrains().First().ShouldBe(initialTerrain);
    }

    [Fact]
    public void HandleHexSelection_ClearTerrain_ShouldReplaceAllWithClear()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        var terrain = new ClearTerrain();
        hex.AddTerrain(new LightWoodsTerrain());
        hex.AddTerrain(new WaterTerrain(-1));
        _sut.SelectedTerrain = terrain;

        _sut.HandleHexSelection(hex);

        hex.GetTerrains().Count().ShouldBe(1);
        hex.GetTerrains().First().ShouldBeOfType<ClearTerrain>();
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsNull_ShouldNotSaveFile()
    {
        await _sut.ExportMapCommand.ExecuteAsync();

        await _fileService.DidNotReceive().SaveJsonFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsSet_ShouldConvertMapToData()
    {
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData();
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        await _sut.ExportMapCommand.ExecuteAsync();

        map.Received(1).ToData();
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsSet_ShouldSerializeToJson()
    {
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData(2, 3);
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        string? savedContent = null;
        await _fileService.SaveJsonFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(content => savedContent = content));

        await _sut.ExportMapCommand.ExecuteAsync();

        savedContent.ShouldNotBeNull();
        var deserializedData = JsonSerializer.Deserialize<BattleMapData>(savedContent);
        deserializedData.ShouldNotBeNull();
        deserializedData.Biome.ShouldBe("test");
        deserializedData.HexData.Count.ShouldBe(1);
        deserializedData.HexData[0].Coordinates.Q.ShouldBe(2);
        deserializedData.HexData[0].Coordinates.R.ShouldBe(3);
    }

    [Fact]
    public async Task ExportMapCommand_ShouldSaveWithCorrectParameters()
    {
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData();
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        await _sut.ExportMapCommand.ExecuteAsync();

        await _fileService.Received(1).SaveJsonFile(
            _localizationService.GetString("EditMap_ExportMapDialogTitle"),
            "map.json",
            Arg.Any<string>());
    }

    [Fact]
    public async Task ExportMapCommand_ShouldSerializeWithIndentation()
    {
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData();
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        string? savedContent = null;
        await _fileService.SaveJsonFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(content => savedContent = content));

        await _sut.ExportMapCommand.ExecuteAsync();

        savedContent.ShouldNotBeNull();
        savedContent.ShouldContain("\n");
    }

    [Fact]
    public async Task ExportMapAsPdf_ShouldCallGeneratePdfFromPngAsync()
    {
        var pngBytes = new byte[] { 1, 2, 3 };
        var width = 800;
        var height = 600;
        var pdfBytes = new byte[] { 4, 5, 6 };
        _pdfExportService.GeneratePdfFromPngAsync(pngBytes, width, height).Returns(pdfBytes);

        await _sut.ExportMapAsPdf(pngBytes, width, height);

        await _pdfExportService.Received(1).GeneratePdfFromPngAsync(pngBytes, width, height);
    }

    [Fact]
    public async Task ExportMapAsPdf_ShouldSaveWithCorrectParameters()
    {
        var pngBytes = new byte[] { 1, 2, 3 };
        const int width = 800;
        const int height = 600;
        var pdfBytes = new byte[] { 4, 5, 6 };
        _pdfExportService.GeneratePdfFromPngAsync(pngBytes, width, height).Returns(pdfBytes);

        await _sut.ExportMapAsPdf(pngBytes, width, height);

        await _fileService.Received(1).SaveBinaryFile(
            _localizationService.GetString("EditMap_ExportPdfDialogTitle"),
            "map.pdf",
            pdfBytes,
            "pdf",
            _localizationService.GetString("EditMap_PdfFilesFilter"));
    }

    [Fact]
    public async Task ExportMapAsPdf_WhenPdfGenerationFails_ShouldLogError()
    {
        var pngBytes = new byte[] { 1, 2, 3 };
        const int width = 800;
        const int height = 600;
        var exception = new InvalidOperationException("PDF generation failed");
        _pdfExportService.GeneratePdfFromPngAsync(pngBytes, width, height).Returns<Task<byte[]>>(_ => throw exception);

        await _sut.ExportMapAsPdf(pngBytes, width, height);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to export PDF")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    // --- ExportMapAsPdf() parameterless overload tests ---
    [Fact]
    public async Task ExportMapAsPdf_WhenCaptureMapIsNull_ShouldNotCallPdfExportService()
    {
        _sut.CaptureMap = null;

        await _sut.ExportMapAsPdf();

        await _pdfExportService.DidNotReceive().GeneratePdfFromPngAsync(
            Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ExportMapAsPdf_WhenCaptureMapIsNull_ShouldNotCallFileService()
    {
        _sut.CaptureMap = null;

        await _sut.ExportMapAsPdf();

        await _fileService.DidNotReceive().SaveBinaryFile(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>(),
            Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExportMapAsPdf_WithCaptureMap_ShouldCallCaptureMap()
    {
        var pngBytes = new byte[] { 1, 2, 3 };
        var pdfBytes = new byte[] { 4, 5, 6 };
        _pdfExportService.GeneratePdfFromPngAsync(pngBytes, 800, 600).Returns(pdfBytes);
        var captureMapCalled = false;
        _sut.CaptureMap = () =>
        {
            captureMapCalled = true;
            return Task.FromResult((pngBytes, 800, 600));
        };

        await _sut.ExportMapAsPdf();

        captureMapCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ExportMapAsPdf_WithCaptureMap_ShouldPassCorrectParametersToPdfExport()
    {
        var pngBytes = new byte[] { 1, 2, 3 };
        var pdfBytes = new byte[] { 4, 5, 6 };
        _pdfExportService.GeneratePdfFromPngAsync(pngBytes, 1024, 768).Returns(pdfBytes);
        _sut.CaptureMap = () => Task.FromResult((pngBytes, 1024, 768));

        await _sut.ExportMapAsPdf();

        await _pdfExportService.Received(1).GeneratePdfFromPngAsync(pngBytes, 1024, 768);
    }

    [Fact]
    public async Task ExportMapAsPdf_WithCaptureMap_ShouldSaveGeneratedPdf()
    {
        var pngBytes = new byte[] { 1, 2, 3 };
        var pdfBytes = new byte[] { 4, 5, 6 };
        _pdfExportService.GeneratePdfFromPngAsync(pngBytes, 800, 600).Returns(pdfBytes);
        _sut.CaptureMap = () => Task.FromResult((pngBytes, 800, 600));

        await _sut.ExportMapAsPdf();

        await _fileService.Received(1).SaveBinaryFile(
            _localizationService.GetString("EditMap_ExportPdfDialogTitle"),
            "map.pdf",
            pdfBytes,
            "pdf",
            _localizationService.GetString("EditMap_PdfFilesFilter"));
    }

    [Fact]
    public async Task ExportMapAsPdf_WhenCaptureMapThrows_ShouldLogError()
    {
        var exception = new InvalidOperationException("Capture failed");
        _sut.CaptureMap = () => throw exception;

        await _sut.ExportMapAsPdf();

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to capture and export map as PDF")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void AvailableTerrains_ShouldBeObservableCollection()
    {
        _sut.AvailableTerrains.ShouldBeOfType<ObservableCollection<Terrain>>();
    }

    [Fact]
    public void HandleHexSelection_WithMultipleCalls_ShouldUpdateHexEachTime()
    {
        var hex1 = new Hex(new HexCoordinates(0, 0));
        var hex2 = new Hex(new HexCoordinates(1, 1));
        var initialTerrain = new LightWoodsTerrain();
        hex1.AddTerrain(initialTerrain);
        hex2.AddTerrain(initialTerrain);
        var terrain = new ClearTerrain();
        _sut.SelectedTerrain = terrain;

        _sut.HandleHexSelection(hex1);
        _sut.HandleHexSelection(hex2);

        hex1.GetTerrains().First().ShouldBeOfType<ClearTerrain>();
        hex2.GetTerrains().First().ShouldBeOfType<ClearTerrain>();
    }

    [Fact]
    public void ActiveEditMode_DefaultValue_ShouldBeTerrain()
    {
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public void HandleHexSelection_InTerrainMode_ShouldReturnHex()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        _sut.SelectedTerrain = new ClearTerrain();

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.ShouldBe(hex);
    }

    [Fact]
    public void GetEdgeUpdatesForNeighbors_ShouldReturnEdgesForValidOnMapNeighbors()
    {
        var map = new BattleMap(3, 3);
        for (var q = 1; q <= 3; q++)
        for (var r = 1; r <= 3; r++)
            map.AddHex(new Hex(new HexCoordinates(q, r)));

        _sut.Initialize(map);
        var centerCoords = new HexCoordinates(2, 2);

        var updates = _sut.GetEdgeUpdatesForNeighbors(centerCoords).ToList();

        updates.ShouldNotBeEmpty();
        updates.All(u => map.IsOnMap(u.Coordinates)).ShouldBeTrue();
        updates.All(u => u.Edges.Count == 6).ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_WithInvalidEditMode_ShouldReturnNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        var propertyInfo = typeof(EditMapViewModel).GetProperty(nameof(EditMapViewModel.ActiveEditMode));
        propertyInfo?.SetValue(_sut, (ToolType)999);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBeNull();
    }

    // --- AvailableTools / SelectedTool tests ---
    [Fact]
    public void AvailableTools_DefaultValue_ShouldBeEmpty()
    {
        _sut.AvailableTools.ShouldBeEmpty();
    }

    [Fact]
    public void Initialize_ShouldPopulateAvailableToolsWithCursorAndTerrains()
    {
        var map = new BattleMap(1,1);

        _sut.Initialize(map);

        _sut.AvailableTools.ShouldNotBeEmpty();
        _sut.AvailableTools.Count.ShouldBe(_sut.AvailableTerrains.Count + 1 + 5);
        _sut.AvailableTools.Count(t => t.Type == ToolType.Cursor).ShouldBe(1);
        _sut.AvailableTools.Count(t => t.Type == ToolType.RaiseLevel).ShouldBe(1);
        _sut.AvailableTools.Count(t => t.Type == ToolType.LowerLevel).ShouldBe(1);
        _sut.AvailableTools.Count(t => t.Type == ToolType.IncreaseWaterDepth).ShouldBe(1);
        _sut.AvailableTools.Count(t => t.Type == ToolType.DecreaseWaterDepth).ShouldBe(1);
    }

    [Fact]
    public void SelectedTool_DefaultValue_ShouldBeNull()
    {
        _sut.SelectedTool.ShouldBeNull();
    }

    [Fact]
    public void Initialize_ShouldSetSelectedToolToFirstTerrainTool()
    {
        var map = new BattleMap(1,1);

        _sut.Initialize(map);

        _sut.SelectedTool.ShouldNotBeNull();
        _sut.SelectedTool.Type.ShouldBe(ToolType.Terrain);
        _sut.SelectedTool.Terrain.ShouldBe(_sut.SelectedTerrain);
    }

    [Fact]
    public void SelectedTool_WhenSetToTerrain_ShouldSetSelectedTerrainAndTerrainMode()
    {
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        var terrainTool = _sut.AvailableTools.First(t => t.Type == ToolType.Terrain);

        _sut.SelectedTool = terrainTool;

        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
        _sut.SelectedTerrain.ShouldBe(terrainTool.Terrain);
    }

    // --- Level Popup Command Tests ---
    [Fact]
    public async Task RaiseLevelCommand_WhenHexSelected_ShouldIncreaseLevel()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.RaiseLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(3);
        _sut.HexViewModel.ShouldNotBeNull();
        _sut.HexViewModel.Level.ShouldBe(3);
    }

    [Fact]
    public async Task RaiseLevelCommand_ShouldPreserveTerrains()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 0);
        hex.AddTerrain(new LightWoodsTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.RaiseLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.GetTerrains().ShouldContain(t => t is LightWoodsTerrain);
    }

    [Fact]
    public async Task LowerLevelCommand_WhenHexSelected_ShouldDecreaseLevel()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.LowerLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(1);
    }

    [Fact]
    public async Task LowerLevelCommand_WhenLevelIsZero_ShouldCreateNegativeLevel()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 0);
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.LowerLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(-1);
    }

    [Fact]
    public async Task RaiseLevelCommand_WhenNoHexSelected_ShouldNotModifyMap()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        map.AddHex(hex);
        _sut.Initialize(map);

        await _sut.RaiseLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(2);
    }

    [Fact]
    public async Task LowerLevelCommand_WhenNoHexSelected_ShouldNotModifyMap()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        map.AddHex(hex);
        _sut.Initialize(map);

        await _sut.LowerLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(2);
    }

    // --- Water Depth Popup Command Tests ---
    [Fact]
    public async Task IncreaseWaterDepthCommand_WhenHexSelected_ShouldIncreaseDepth()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var waterTerrain = mapHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-1);
    }

    [Fact]
    public async Task DecreaseWaterDepthCommand_WhenHexSelected_ShouldDecreaseDepth()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-1));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var waterTerrain = mapHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(0);
    }

    [Fact]
    public async Task IncreaseWaterDepthCommand_WhenHexSelected_ShouldPreserveNonWaterTerrains()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new LightWoodsTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.GetTerrains().ShouldContain(t => t is LightWoodsTerrain);
        mapHex.GetTerrains().ShouldContain(t => t is WaterTerrain);
    }

    [Fact]
    public async Task IncreaseWaterDepthCommand_WhenHexHasNoWater_ShouldNotModifyHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Water).ShouldBeFalse();
    }

    [Fact]
    public async Task DecreaseWaterDepthCommand_WhenHexHasNoWater_ShouldNotModifyHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Water).ShouldBeFalse();
    }

    [Fact]
    public async Task IncreaseWaterDepthCommand_WhenDepthAlreadyNegative_ShouldDecreaseFurther()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-2));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var waterTerrain = mapHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-3);
    }

    [Fact]
    public async Task DecreaseWaterDepthCommand_WhenDepthIsZero_ShouldNotBecomePositive()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var waterTerrain = mapHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(0);
    }

    [Fact]
    public async Task IncreaseWaterDepthCommand_WhenNoHexSelected_ShouldNotAddWater()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);

        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Water).ShouldBeFalse();
    }

    [Fact]
    public async Task DecreaseWaterDepthCommand_WhenNoHexSelected_ShouldNotModifyHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-1));
        map.AddHex(hex);
        _sut.Initialize(map);

        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var waterTerrain = mapHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-1);
    }

    // --- HexUpdated Event Tests ---
    [Fact]
    public async Task RaiseLevelCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.RaiseLevelCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        updatedHex.Level.ShouldBe(3);
    }

    [Fact]
    public async Task LowerLevelCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.LowerLevelCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        updatedHex.Level.ShouldBe(1);
    }

    [Fact]
    public async Task IncreaseWaterDepthCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        var waterTerrain = updatedHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-1);
    }

    [Fact]
    public async Task DecreaseWaterDepthCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-1));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        var waterTerrain = updatedHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(0);
    }

    // --- HexConfiguration tests ---
    [Fact]
    public void HexConfiguration_ShouldBeInitializedViaConstructor()
    {
        _sut.HexConfiguration.ShouldNotBeNull();
    }

    [Fact]
    public void HexConfiguration_WhenPropertyChanged_ShouldRaiseViewModelPropertyChanged()
    {
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(EditMapViewModel.HexConfiguration))
                propertyChangedRaised = true;
        };

        _sut.HexConfiguration.ShowLabels = !_sut.HexConfiguration.ShowLabels;

        propertyChangedRaised.ShouldBeTrue();
    }

    [Fact]
    public void DetachHandlers_ShouldUnsubscribeFromHexConfigurationPropertyChanged()
    {
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(EditMapViewModel.HexConfiguration))
                propertyChangedRaised = true;
        };
        _sut.DetachHandlers();

        _sut.HexConfiguration.ShowLabels = !_sut.HexConfiguration.ShowLabels;

        propertyChangedRaised.ShouldBeFalse();
    }

    // --- Menu Visibility Tests ---
    [Fact]
    public void IsMenuVisible_DefaultValue_ShouldBeFalse()
    {
        _sut.IsMenuVisible.ShouldBeFalse();
    }

    [Fact]
    public async Task ToggleMenuCommand_ShouldToggleIsMenuVisible()
    {
        await _sut.ToggleMenuCommand.ExecuteAsync();

        _sut.IsMenuVisible.ShouldBeTrue();

        await _sut.ToggleMenuCommand.ExecuteAsync();

        _sut.IsMenuVisible.ShouldBeFalse();
    }

    [Fact]
    public void IsMenuVisible_WhenSet_ShouldNotifyPropertyChanged()
    {
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(EditMapViewModel.IsMenuVisible))
                propertyChangedRaised = true;
        };

        _sut.IsMenuVisible = true;

        propertyChangedRaised.ShouldBeTrue();
    }

    [Fact]
    public async Task CloseMenuCommand_ShouldSetIsMenuVisibleToFalse()
    {
        _sut.IsMenuVisible = true;

        await _sut.CloseMenuCommand.ExecuteAsync();

        _sut.IsMenuVisible.ShouldBeFalse();
    }

    // --- Cursor Mode Tests ---
    [Fact]
    public void AvailableTools_ShouldContainCursorTool()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);

        _sut.AvailableTools.Any(t => t.Type == ToolType.Cursor).ShouldBeTrue();
    }

    [Fact]
    public void SelectedCursorTool_ShouldSetActiveEditModeToCursor()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var cursorTool = _sut.AvailableTools.First(t => t.Type == ToolType.Cursor);

        _sut.SelectedTool = cursorTool;

        _sut.ActiveEditMode.ShouldBe(ToolType.Cursor);
        _sut.IsCursorActive.ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_InCursorMode_ShouldNotMutateHex()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBeNull();
        hex.GetTerrains().ShouldContain(t => t is ClearTerrain);
    }

    [Fact]
    public void HandleHexSelection_InCursorMode_ShouldPopulateHexViewModel()
    {
        var hex = new Hex(new HexCoordinates(0, 0), level: 3);
        hex.AddTerrain(new LightWoodsTerrain());
        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);

        _sut.HandleHexSelection(hex);

        _sut.HexViewModel.ShouldNotBeNull();
        _sut.HexViewModel.Level.ShouldBe(3);
        _sut.HexViewModel.TerrainTypes.ShouldContain("LightWoods");
    }

    [Fact]
    public void HandleHexSelection_InCursorMode_ShouldPopulateHexViewModelWithWaterDepth()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-2));
        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);

        _sut.HandleHexSelection(hex);

        _sut.HexViewModel.ShouldNotBeNull();
        _sut.HexViewModel.WaterDepth.ShouldBe(2);
        _sut.HexViewModel.IsWater.ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_InCursorMode_ShouldSetIsHexInfoVisible()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);

        _sut.HandleHexSelection(hex);

        _sut.IsHexInfoVisible.ShouldBeTrue();
    }

    [Fact]
    public void SelectingNonCursorTool_ShouldClearHexViewModelAndVisibility()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var cursorTool = _sut.AvailableTools.First(t => t.Type == ToolType.Cursor);
        _sut.SelectedTool = cursorTool;
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        _sut.HandleHexSelection(hex);
        _sut.HexViewModel.ShouldNotBeNull();
        _sut.IsHexInfoVisible.ShouldBeTrue();

        var terrainTool = _sut.AvailableTools.First(t => t.Type == ToolType.Terrain);
        _sut.SelectedTool = terrainTool;

        _sut.HexViewModel.ShouldBeNull();
        _sut.IsHexInfoVisible.ShouldBeFalse();
    }

    // --- Level/Depth Tool Mode Tests ---
    [Fact]
    public void SelectingRaiseLevelTool_ShouldSetActiveEditModeToRaiseLevel()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var tool = _sut.AvailableTools.First(t => t.Type == ToolType.RaiseLevel);

        _sut.SelectedTool = tool;

        _sut.ActiveEditMode.ShouldBe(ToolType.RaiseLevel);
    }

    [Fact]
    public void SelectingLowerLevelTool_ShouldSetActiveEditModeToLowerLevel()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var tool = _sut.AvailableTools.First(t => t.Type == ToolType.LowerLevel);

        _sut.SelectedTool = tool;

        _sut.ActiveEditMode.ShouldBe(ToolType.LowerLevel);
    }

    [Fact]
    public void SelectingIncreaseWaterDepthTool_ShouldSetActiveEditModeToIncreaseWaterDepth()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var tool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);

        _sut.SelectedTool = tool;

        _sut.ActiveEditMode.ShouldBe(ToolType.IncreaseWaterDepth);
    }

    [Fact]
    public void SelectingDecreaseWaterDepthTool_ShouldSetActiveEditModeToDecreaseWaterDepth()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var tool = _sut.AvailableTools.First(t => t.Type == ToolType.DecreaseWaterDepth);

        _sut.SelectedTool = tool;

        _sut.ActiveEditMode.ShouldBe(ToolType.DecreaseWaterDepth);
    }

    [Fact]
    public void HandleHexSelection_RaiseLevelTool_ShouldIncreaseLevel()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RaiseLevel);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.Level.ShouldBe(3);
        result.Coordinates.ShouldBe(new HexCoordinates(1, 1));
        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(3);
    }

    [Fact]
    public void HandleHexSelection_RaiseLevelTool_ShouldPreserveTerrains()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 0);
        hex.AddTerrain(new LightWoodsTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RaiseLevel);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.GetTerrains().ShouldContain(t => t is LightWoodsTerrain);
    }

    [Fact]
    public void HandleHexSelection_LowerLevelTool_ShouldDecreaseLevel()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.LowerLevel);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.Level.ShouldBe(1);
        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(1);
    }

    [Fact]
    public void HandleHexSelection_LowerLevelTool_WhenLevelIsZero_ShouldCreateNegativeLevel()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 0);
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.LowerLevel);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.Level.ShouldBe(-1);
    }

    [Fact]
    public void HandleHexSelection_IncreaseWaterDepthTool_ShouldIncreaseDepth()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        var waterTerrain = result.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-1);
    }

    [Fact]
    public void HandleHexSelection_DecreaseWaterDepthTool_ShouldDecreaseDepth()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-1));
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.DecreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        var waterTerrain = result.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(0);
    }

    [Fact]
    public void HandleHexSelection_IncreaseWaterDepthTool_ShouldPreserveNonWaterTerrains()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new LightWoodsTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.GetTerrains().ShouldContain(t => t is LightWoodsTerrain);
        result.GetTerrains().ShouldContain(t => t is WaterTerrain);
    }

    [Fact]
    public void HandleHexSelection_IncreaseWaterDepthTool_WhenHexHasNoWater_ShouldNotModifyHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Water).ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_DecreaseWaterDepthTool_WhenHexHasNoWater_ShouldNotModifyHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.DecreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Water).ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_IncreaseWaterDepthTool_WhenDepthAlreadyNegative_ShouldDecreaseFurther()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-2));
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        var waterTerrain = result.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-3);
    }

    [Fact]
    public void HandleHexSelection_DecreaseWaterDepthTool_WhenDepthIsZero_ShouldNotBecomePositive()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.DecreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        var waterTerrain = result.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(0);
    }

    [Fact]
    public void HandleHexSelection_LevelTool_ShouldNotOpenHexInfo()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RaiseLevel);

        _sut.HandleHexSelection(hex);

        _sut.IsHexInfoVisible.ShouldBeFalse();
        _sut.HexViewModel.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_WaterDepthTool_ShouldNotOpenHexInfo()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);

        _sut.HandleHexSelection(hex);

        _sut.IsHexInfoVisible.ShouldBeFalse();
        _sut.HexViewModel.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_RaiseLevelTool_WhenMapIsNull_ShouldReturnNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        _sut.SelectedTool = new ToolItem("Raise", ToolType.RaiseLevel);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_LowerLevelTool_WhenMapIsNull_ShouldReturnNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        _sut.SelectedTool = new ToolItem("Lower", ToolType.LowerLevel);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_IncreaseWaterDepthTool_WhenMapIsNull_ShouldReturnNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        _sut.SelectedTool = new ToolItem("IncreaseDepth", ToolType.IncreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_DecreaseWaterDepthTool_WhenMapIsNull_ShouldReturnNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        _sut.SelectedTool = new ToolItem("DecreaseDepth", ToolType.DecreaseWaterDepth);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_InCursorMode_ShouldPopulateMultipleTerrainTypes()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new LightWoodsTerrain());
        hex.AddTerrain(new WaterTerrain(-1));
        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);

        _sut.HandleHexSelection(hex);

        _sut.HexViewModel.ShouldNotBeNull();
        _sut.HexViewModel.TerrainTypes.Count.ShouldBe(2);
        _sut.HexViewModel.TerrainTypes.ShouldContain("LightWoods");
        _sut.HexViewModel.TerrainTypes.ShouldContain("Water");
        _sut.HexViewModel.WaterDepth.ShouldBe(1);
    }

    [Fact]
    public void ActiveEditMode_WhenSetToCursor_ShouldNotifyIsCursorActive()
    {
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(EditMapViewModel.IsCursorActive))
                propertyChangedRaised = true;
        };

        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);

        propertyChangedRaised.ShouldBeTrue();
        _sut.IsCursorActive.ShouldBeTrue();
    }

    [Fact]
    public void IsCursorActive_ShouldBeFalseByDefault()
    {
        _sut.IsCursorActive.ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_InCursorMode_WhenHexViewModelExists_ShouldUpdateExistingViewModel()
    {
        var hex1 = new Hex(new HexCoordinates(0, 0), level: 2);
        hex1.AddTerrain(new LightWoodsTerrain());
        var hex2 = new Hex(new HexCoordinates(1, 1), level: 5);
        hex2.AddTerrain(new WaterTerrain(-3));
        hex2.AddTerrain(new HeavyWoodsTerrain());
        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);

        _sut.HandleHexSelection(hex1);
        var initialViewModel = _sut.HexViewModel;
        initialViewModel.ShouldNotBeNull();
        initialViewModel.Level.ShouldBe(2);

        _sut.HandleHexSelection(hex2);

        _sut.HexViewModel.ShouldBe(initialViewModel);
        initialViewModel.Level.ShouldBe(5);
        initialViewModel.TerrainTypes.ShouldContain("Water");
        initialViewModel.TerrainTypes.ShouldContain("HeavyWoods");
        initialViewModel.WaterDepth.ShouldBe(3);
        _sut.IsHexInfoVisible.ShouldBeTrue();
    }

    // --- CloseHexInfoCommand tests ---
    [Fact]
    public async Task CloseHexInfoCommand_ShouldHideHexInfoAndClearViewModel()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        _sut.SelectedTool = new ToolItem("Cursor", ToolType.Cursor);
        _sut.HandleHexSelection(hex);
        _sut.HexViewModel.ShouldNotBeNull();
        _sut.IsHexInfoVisible.ShouldBeTrue();

        await _sut.CloseHexInfoCommand.ExecuteAsync();

        _sut.IsHexInfoVisible.ShouldBeFalse();
        _sut.HexViewModel.ShouldBeNull();

        _sut.HandleHexSelection(hex);
        _sut.HexViewModel.ShouldNotBeNull();
        _sut.IsHexInfoVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task CloseEditMapCommand_WhenConfirmed_ShouldNavigateToNewMap()
    {
        var yesAction = new UiAction { Title = "Dialog_Yes" };
        _navigationService.AskForActionAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UiAction>(),
            Arg.Any<UiAction>()).Returns(yesAction);

        await _sut.CloseEditMapCommand.ExecuteAsync();

        await _navigationService.Received(1).NavigateBackAsync();
        _sut.IsMenuVisible.ShouldBeFalse();
    }

    [Fact]
    public async Task CloseEditMapCommand_WhenDeclined_ShouldNotNavigateBack()
    {
        var noAction = new UiAction { Title = "Dialog_No" };
        _navigationService.AskForActionAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UiAction>(),
            Arg.Any<UiAction>()).Returns(noAction);

        await _sut.CloseEditMapCommand.ExecuteAsync();

        await _navigationService.DidNotReceive().NavigateBackAsync();
    }

    [Fact]
    public async Task OpenAboutCommand_ShouldNavigateToAboutViewModel()
    {
        await _sut.OpenAboutCommand.ExecuteAsync();

        await _navigationService.Received(1).NavigateToViewModelAsync<AboutViewModel>();
    }

    // --- RoadBridge Tool Tests ---
    [Fact]
    public void HandleHexSelection_RoadBridgeTool_OnNonWaterHex_ShouldAddRoad()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RoadBridge);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Road).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Clear).ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_RoadBridgeTool_OnWaterHex_ShouldAddBridge()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RoadBridge);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Water).ShouldBeTrue();
        var bridge = result.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.Height.ShouldBe(EditMapViewModel.DefaultBridgeHeight);
        bridge.ConstructionFactor.ShouldBe(EditMapViewModel.DefaultConstructionFactor);
    }

    [Fact]
    public void HandleHexSelection_RoadBridgeTool_WhenAlreadyRoad_ShouldReturnSameHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new RoadTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RoadBridge);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBe(hex);
    }

    [Fact]
    public void HandleHexSelection_RoadBridgeTool_WhenAlreadyBridge_ShouldReturnSameHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RoadBridge);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBe(hex);
    }

    [Fact]
    public void HandleHexSelection_RoadBridgeTool_OnNonWaterHex_ShouldPreserveUnderlyingTerrain()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new LightWoodsTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RoadBridge);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Road).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.LightWoods).ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_RoadBridgeTool_WhenMapIsNull_ShouldReturnNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        _sut.SelectedTool = new ToolItem("RoadBridge", ToolType.RoadBridge);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldBeNull();
    }

    // --- Terrain Layering Tests ---
    [Fact]
    public void HandleHexSelection_TerrainMode_RoadOnWater_ShouldAddBridgePreserveWater()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-1));
        _sut.SelectedTerrain = new RoadTerrain();

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Water).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Road).ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_TerrainMode_RoadOnLightWoods_ShouldPreserveBoth()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new LightWoodsTerrain());
        _sut.SelectedTerrain = new RoadTerrain();

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Road).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.LightWoods).ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_TerrainMode_WaterOnRoad_ShouldConvertRoadToBridge()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new RoadTerrain());
        _sut.SelectedTerrain = new WaterTerrain(-1);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Water).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Road).ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_TerrainMode_LightWoodsOnRoad_ShouldPreserveRoad()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new RoadTerrain());
        _sut.SelectedTerrain = new LightWoodsTerrain();

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Road).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.LightWoods).ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_TerrainMode_LightWoodsOnHeavyWoods_ShouldReplaceGround()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new HeavyWoodsTerrain());
        _sut.SelectedTerrain = new LightWoodsTerrain();

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.LightWoods).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.HeavyWoods).ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_TerrainMode_RoadOnBridge_ShouldDoNothing()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-1));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        _sut.SelectedTerrain = new RoadTerrain();

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Water).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Road).ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_TerrainMode_WaterOnBridge_ShouldPreserveBridge()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-1));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        _sut.SelectedTerrain = new WaterTerrain(-2);

        var result = _sut.HandleHexSelection(hex);

        result.ShouldNotBeNull();
        result.HasTerrain(MakaMekTerrains.Water).ShouldBeTrue();
        result.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
    }

    // --- Bridge Command Tests ---
    [Fact]
    public async Task RaiseBridgeLevelCommand_ConvertsRoadToBridge()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new RoadTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.RaiseBridgeLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
        mapHex.HasTerrain(MakaMekTerrains.Road).ShouldBeFalse();
        var bridge = mapHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.Height.ShouldBe(1);
        bridge.ConstructionFactor.ShouldBe(EditMapViewModel.DefaultConstructionFactor);
    }

    [Fact]
    public async Task RaiseBridgeLevelCommand_IncreasesExistingBridgeHeight()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.RaiseBridgeLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var bridge = mapHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.Height.ShouldBe(2);
        bridge.ConstructionFactor.ShouldBe(60);
    }

    [Fact]
    public async Task LowerBridgeLevelCommand_ConvertsBridgeToRoadOverNonWater()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.LowerBridgeLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Road).ShouldBeTrue();
        mapHex.HasTerrain(MakaMekTerrains.Bridge).ShouldBeFalse();
    }

    [Fact]
    public async Task LowerBridgeLevelCommand_KeepsBridgeOverWater()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.LowerBridgeLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
        var bridge = mapHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.Height.ShouldBe(0);
    }

    [Fact]
    public async Task LowerBridgeLevelCommand_WhenNoBridge_ShouldNotModifyHex()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.LowerBridgeLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Bridge).ShouldBeFalse();
        mapHex.HasTerrain(MakaMekTerrains.Road).ShouldBeFalse();
    }

    [Fact]
    public async Task IncreaseConstructionFactorCommand_ShouldIncreaseByStep()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.IncreaseConstructionFactorCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var bridge = mapHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.ConstructionFactor.ShouldBe(65);
    }

    [Fact]
    public async Task DecreaseConstructionFactorCommand_ShouldDecreaseByStep()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.DecreaseConstructionFactorCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var bridge = mapHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.ConstructionFactor.ShouldBe(55);
    }

    [Fact]
    public async Task IncreaseConstructionFactorCommand_ShouldNotExceedMax()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 500));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.IncreaseConstructionFactorCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var bridge = mapHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.ConstructionFactor.ShouldBe(500);
    }

    [Fact]
    public async Task DecreaseConstructionFactorCommand_ShouldNotGoBelowMin()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 0));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        await _sut.DecreaseConstructionFactorCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var bridge = mapHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.ConstructionFactor.ShouldBe(0);
    }

    [Fact]
    public async Task RaiseBridgeLevelCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new RoadTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.RaiseBridgeLevelCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        updatedHex.HasTerrain(MakaMekTerrains.Bridge).ShouldBeTrue();
    }

    [Fact]
    public async Task LowerBridgeLevelCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.LowerBridgeLevelCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        updatedHex.HasTerrain(MakaMekTerrains.Road).ShouldBeTrue();
    }

    [Fact]
    public async Task IncreaseConstructionFactorCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.IncreaseConstructionFactorCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        var bridge = updatedHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.ConstructionFactor.ShouldBe(65);
    }

    [Fact]
    public async Task DecreaseConstructionFactorCommand_ShouldFireHexUpdated()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        map.AddHex(hex);
        _sut.Initialize(map);
        SelectHexViaCursor(_sut, hex);

        Hex? updatedHex = null;
        _sut.HexUpdated += h => updatedHex = h;

        await _sut.DecreaseConstructionFactorCommand.ExecuteAsync();

        updatedHex.ShouldNotBeNull();
        var bridge = updatedHex.GetTerrain(MakaMekTerrains.Bridge) as BridgeTerrain;
        bridge.ShouldNotBeNull();
        bridge.ConstructionFactor.ShouldBe(55);
    }

    [Fact]
    public async Task RaiseBridgeLevelCommand_WhenNoHexSelected_ShouldNotModifyMap()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new RoadTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);

        await _sut.RaiseBridgeLevelCommand.ExecuteAsync();

        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.HasTerrain(MakaMekTerrains.Road).ShouldBeTrue();
    }

    [Fact]
    public void HandleHexSelection_RoadBridgeTool_ShouldNotOpenHexInfo()
    {
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        _sut.SelectedTool = _sut.AvailableTools.First(t => t.Type == ToolType.RoadBridge);

        _sut.HandleHexSelection(hex);

        _sut.IsHexInfoVisible.ShouldBeFalse();
        _sut.HexViewModel.ShouldBeNull();
    }

    [Fact]
    public void Initialize_ShouldContainRoadBridgeTool()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);

        _sut.AvailableTools.Any(t => t.Type == ToolType.RoadBridge).ShouldBeTrue();
    }

    [Fact]
    public void SelectingRoadBridgeTool_ShouldSetActiveEditModeToRoadBridge()
    {
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var tool = _sut.AvailableTools.First(t => t.Type == ToolType.RoadBridge);

        _sut.SelectedTool = tool;

        _sut.ActiveEditMode.ShouldBe(ToolType.RoadBridge);
    }
}
