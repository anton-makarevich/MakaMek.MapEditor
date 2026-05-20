using System.Collections.ObjectModel;
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
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class EditMapViewModelTests
{
    private readonly IFileService _fileService = Substitute.For<IFileService>();
    private readonly ITerrainAssetService _assetService = Substitute.For<ITerrainAssetService>();
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly EditMapViewModel _sut;
    private readonly ILogger<EditMapViewModel> _logger = Substitute.For<ILogger<EditMapViewModel>>();
    private readonly ITerrainBitmaskService _bitmaskService = Substitute.For<ITerrainBitmaskService>();

    public EditMapViewModelTests()
    {
        _localizationService.GetString(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());
        _sut = new EditMapViewModel(_fileService,
            _assetService,
            _localizationService,
            _logger,
            _bitmaskService);
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
    public void LocalizationService_ShouldBeInitializedViaConstructor()
    {
        // Assert
        _sut.LocalizationService.ShouldNotBeNull();
        _sut.LocalizationService.ShouldBe(_localizationService);
    }

    [Fact]
    public void Initialize_ShouldSetMap()
    {
        // Arrange
        var map = new BattleMap(1,1);

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.Map.ShouldBe(map);
    }

    [Fact]
    public void Initialize_ShouldLoadTerrains()
    {
        // Arrange
        var map = new BattleMap(1,1);

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.AvailableTerrains.ShouldNotBeEmpty();
    }

    [Fact]
    public void Initialize_ShouldSetFirstTerrainAsSelected()
    {
        // Arrange
        var map = new BattleMap(1,1);

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.SelectedTerrain.ShouldNotBeNull();
        _sut.SelectedTerrain.ShouldBe(_sut.AvailableTerrains.First());
    }

    [Fact]
    public void Initialize_ShouldLoadAllConcreteTerrainTypes()
    {
        // Arrange
        var map = new BattleMap(1,1);

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.AvailableTerrains.Count.ShouldBeGreaterThan(0);
        _sut.AvailableTerrains.All(t => t.GetType().IsAbstract == false).ShouldBeTrue();
    }

    [Fact]
    public void SelectedTerrain_WhenSet_ShouldUpdateValue()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        var newTerrain = new ClearTerrain();

        // Act
        _sut.SelectedTerrain = newTerrain;

        // Assert
        _sut.SelectedTerrain.ShouldBe(newTerrain);
    }

    [Fact]
    public void HandleHexSelection_WhenSelectedTerrainIsNull_ShouldNotModifyHex()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        var initialTerrain = new ClearTerrain();
        hex.AddTerrain(initialTerrain);
        _sut.SelectedTerrain = null;

        // Act
        _sut.HandleHexSelection(hex);

        // Assert
        hex.GetTerrains().First().ShouldBe(initialTerrain);
    }

    [Fact]
    public void HandleHexSelection_WhenSelectedTerrainIsSet_ShouldReplaceHexTerrains()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        var terrain = new ClearTerrain();
        var initialTerrain = new LightWoodsTerrain();
        hex.AddTerrain(initialTerrain);
        _sut.SelectedTerrain = terrain;

        // Act
        _sut.HandleHexSelection(hex);
 
        // Assert
        hex.GetTerrains().First().ShouldBe(terrain);
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsNull_ShouldNotSaveFile()
    {
        // Act
        await _sut.ExportMapCommand.ExecuteAsync();

        // Assert
        await _fileService.DidNotReceive().SaveFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsSet_ShouldConvertMapToData()
    {
        // Arrange
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData();
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        // Act
        await _sut.ExportMapCommand.ExecuteAsync();

        // Assert
        map.Received(1).ToData();
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsSet_ShouldSerializeToJson()
    {
        // Arrange
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData(2, 3);
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        string? savedContent = null;
        await _fileService.SaveFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(content => savedContent = content));

        // Act
        await _sut.ExportMapCommand.ExecuteAsync();

        // Assert
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
        // Arrange
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData();
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        // Act
        await _sut.ExportMapCommand.ExecuteAsync();

        // Assert
        await _fileService.Received(1).SaveFile(
            _localizationService.GetString("EditMap_ExportMapDialogTitle"),
            "map.json",
            Arg.Any<string>());
    }

    [Fact]
    public async Task ExportMapCommand_ShouldSerializeWithIndentation()
    {
        // Arrange
        var map = Substitute.For<IBattleMap>();
        var hexData = CreateTestBattleMapData();
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        string? savedContent = null;
        await _fileService.SaveFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(content => savedContent = content));

        // Act
        await _sut.ExportMapCommand.ExecuteAsync();

        // Assert
        savedContent.ShouldNotBeNull();
        savedContent.ShouldContain("\n"); // Should contain newlines due to WriteIndented = true
    }

    [Fact]
    public void AvailableTerrains_ShouldBeObservableCollection()
    {
        _sut.AvailableTerrains.ShouldBeOfType<ObservableCollection<Terrain>>();
    }

    [Fact]
    public void HandleHexSelection_WithMultipleCalls_ShouldUpdateHexEachTime()
    {
        // Arrange
        var hex1 = new Hex(new HexCoordinates(0, 0));
        var hex2 = new Hex(new HexCoordinates(1, 1));
        var initialTerrain = new LightWoodsTerrain();
        hex1.AddTerrain(initialTerrain);
        hex2.AddTerrain(initialTerrain);
        var terrain = new ClearTerrain();
        _sut.SelectedTerrain = terrain;

        // Act
        _sut.HandleHexSelection(hex1);
        _sut.HandleHexSelection(hex2);

        // Assert
        hex1.GetTerrains().First().ShouldBe(terrain);
        hex2.GetTerrains().First().ShouldBe(terrain);
    }

    // --- EditMode / Level tests ---
    [Fact]
    public void ActiveEditMode_DefaultValue_ShouldBeTerrain()
    {
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public async Task RaiseLevelCommand_ShouldSetActiveEditModeToRaiseLevel()
    {
        // Act
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.RaiseLevel);
    }

    [Fact]
    public async Task RaiseLevelCommand_WhenAlreadyActive_ShouldRevertToTerrain()
    {
        // Arrange
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Act
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public async Task LowerLevelCommand_ShouldSetActiveEditModeToLowerLevel()
    {
        // Act
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.LowerLevel);
    }

    [Fact]
    public async Task LowerLevelCommand_WhenAlreadyActive_ShouldRevertToTerrain()
    {
        // Arrange
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Act
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public async Task IsRaiseLevelActive_ShouldReflectActiveEditMode()
    {
        // Initially false
        _sut.IsRaiseLevelActive.ShouldBeFalse();

        // After activating raise
        await _sut.RaiseLevelCommand.ExecuteAsync();
        _sut.IsRaiseLevelActive.ShouldBeTrue();

        // After deactivating
        await _sut.RaiseLevelCommand.ExecuteAsync();
        _sut.IsRaiseLevelActive.ShouldBeFalse();
    }

    [Fact]
    public async Task IsLowerLevelActive_ShouldReflectActiveEditMode()
    {
        // Initially false
        _sut.IsLowerLevelActive.ShouldBeFalse();

        // After activating lower
        await _sut.LowerLevelCommand.ExecuteAsync();
        _sut.IsLowerLevelActive.ShouldBeTrue();

        // After deactivating
        await _sut.LowerLevelCommand.ExecuteAsync();
        _sut.IsLowerLevelActive.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleHexSelection_InRaiseLevelMode_ShouldCreateNewHexWithIncreasedLevel()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        newHex.Level.ShouldBe(3);
    }

    [Fact]
    public async Task HandleHexSelection_InRaiseLevelMode_ShouldPreserveTerrains()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 0);
        hex.AddTerrain(new LightWoodsTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        newHex.GetTerrains().ShouldContain(t => t is LightWoodsTerrain);
    }

    [Fact]
    public async Task HandleHexSelection_InLowerLevelMode_ShouldCreateNewHexWithDecreasedLevel()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 2);
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        newHex.Level.ShouldBe(1);
    }

    [Fact]
    public async Task HandleHexSelection_InRaiseLevelMode_ShouldReplaceHexInMap()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 0);
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Act
        _sut.HandleHexSelection(hex);

        // Assert
        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        mapHex.Level.ShouldBe(1);
    }

    [Fact]
    public void HandleHexSelection_InTerrainMode_ShouldReturnNull()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        _sut.SelectedTerrain = new ClearTerrain();

        // Act
        var result = _sut.HandleHexSelection(hex);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetEdgeUpdatesForNeighbors_ShouldReturnEdgesForValidOnMapNeighbors()
    {
        // Arrange - 3x3 map, center hex at (2,2)
        var map = new BattleMap(3, 3);
        for (var q = 1; q <= 3; q++)
        for (var r = 1; r <= 3; r++)
            map.AddHex(new Hex(new HexCoordinates(q, r)));

        _sut.Initialize(map);
        var centerCoords = new HexCoordinates(2, 2);

        // Act
        var updates = _sut.GetEdgeUpdatesForNeighbors(centerCoords).ToList();

        // Assert
        updates.ShouldNotBeEmpty();
        // All returned coordinates must be on the map
        updates.All(u => map.IsOnMap(u.Coordinates)).ShouldBeTrue();
        // Each has 6 edges
        updates.All(u => u.Edges.Count == 6).ShouldBeTrue();
    }
    
    [Fact]
    public async Task HandleHexSelection_InLowerLevelMode_WhenLevelIsZero_ShouldCreateNegativeLevel()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1), level: 0);
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        newHex.Level.ShouldBe(-1); 
    }

    [Fact]
    public async Task SwitchingFromRaiseLevelToLowerLevel_ShouldChangeActiveMode()
    {
        // Arrange
        await _sut.RaiseLevelCommand.ExecuteAsync();
        _sut.ActiveEditMode.ShouldBe(ToolType.RaiseLevel);

        // Act
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.LowerLevel);
        _sut.IsLowerLevelActive.ShouldBeTrue();
        _sut.IsRaiseLevelActive.ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_WithInvalidEditMode_ShouldReturnNull()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        // Use reflection to set an invalid EditMode value
        var propertyInfo = typeof(EditMapViewModel).GetProperty(nameof(EditMapViewModel.ActiveEditMode));
        propertyInfo?.SetValue(_sut, (ToolType)999);

        // Act
        var result = _sut.HandleHexSelection(hex);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SelectedTerrain_WhenSetInRaiseLevelMode_ShouldSwitchToTerrainMode()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        await _sut.RaiseLevelCommand.ExecuteAsync();
        _sut.ActiveEditMode.ShouldBe(ToolType.RaiseLevel);

        // Act
        _sut.SelectedTerrain = new ClearTerrain();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public async Task IsRaiseLevelActive_ShouldBeFalse_WhenTerrainSelectedInRaiseLevelMode()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        await _sut.RaiseLevelCommand.ExecuteAsync();
        _sut.IsRaiseLevelActive.ShouldBeTrue();

        // Act
        _sut.SelectedTerrain = new ClearTerrain();

        // Assert
        _sut.IsRaiseLevelActive.ShouldBeFalse();
    }

    [Fact]
    public async Task IsLowerLevelActive_ShouldBeFalse_WhenTerrainSelectedInLowerLevelMode()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        await _sut.LowerLevelCommand.ExecuteAsync();
        _sut.IsLowerLevelActive.ShouldBeTrue();

        // Act
        _sut.SelectedTerrain = new ClearTerrain();

        // Assert
        _sut.IsLowerLevelActive.ShouldBeFalse();
    }

    // --- AvailableTools / SelectedTool tests ---
    [Fact]
    public void AvailableTools_DefaultValue_ShouldBeEmpty()
    {
        _sut.AvailableTools.ShouldBeEmpty();
    }

    [Fact]
    public void Initialize_ShouldPopulateAvailableToolsWithElevationAndTerrains()
    {
        // Arrange
        var map = new BattleMap(1,1);

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.AvailableTools.ShouldNotBeEmpty();
        _sut.AvailableTools.Count.ShouldBe(_sut.AvailableTerrains.Count + 4);
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
        // Arrange
        var map = new BattleMap(1,1);

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.SelectedTool.ShouldNotBeNull();
        _sut.SelectedTool.Type.ShouldBe(ToolType.Terrain);
        _sut.SelectedTool.Terrain.ShouldBe(_sut.SelectedTerrain);
    }

    [Fact]
    public void SelectedTool_WhenSetToRaiseLevel_ShouldSetActiveEditModeToRaiseLevel()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        var raiseTool = _sut.AvailableTools.First(t => t.Type == ToolType.RaiseLevel);

        // Act
        _sut.SelectedTool = raiseTool;

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.RaiseLevel);
    }

    [Fact]
    public void SelectedTool_WhenSetToLowerLevel_ShouldSetActiveEditModeToLowerLevel()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        var lowerTool = _sut.AvailableTools.First(t => t.Type == ToolType.LowerLevel);

        // Act
        _sut.SelectedTool = lowerTool;

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.LowerLevel);
    }

    [Fact]
    public void SelectedTool_WhenSetToTerrain_ShouldSetSelectedTerrainAndTerrainMode()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _sut.Initialize(map);
        var terrainTool = _sut.AvailableTools.First(t => t.Type == ToolType.Terrain);

        // Act
        _sut.SelectedTool = terrainTool;

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
        _sut.SelectedTerrain.ShouldBe(terrainTool.Terrain);
    }
    
    [Fact]
    public async Task RaiseLevelCommand_WhenAlreadyActiveAndToolsPopulated_ShouldSelectTerrainTool()
    {
        // Arrange
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var raiseTool = _sut.AvailableTools.First(t => t.Type == ToolType.RaiseLevel);
        _sut.SelectedTool = raiseTool;
        _sut.ActiveEditMode.ShouldBe(ToolType.RaiseLevel);
        var terrainTool = _sut.AvailableTools.First(t => t.Type == ToolType.Terrain);

        // Act
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Assert
        _sut.SelectedTool.ShouldBe(terrainTool);
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public async Task LowerLevelCommand_WhenAlreadyActiveAndToolsPopulated_ShouldSelectTerrainTool()
    {
        // Arrange
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var lowerTool = _sut.AvailableTools.First(t => t.Type == ToolType.LowerLevel);
        _sut.SelectedTool = lowerTool;
        _sut.ActiveEditMode.ShouldBe(ToolType.LowerLevel);
        var terrainTool = _sut.AvailableTools.First(t => t.Type == ToolType.Terrain);

        // Act
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Assert
        _sut.SelectedTool.ShouldBe(terrainTool);
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    // --- Water Depth Command Tests ---
    [Fact]
    public async Task IncreaseWaterDepthCommand_ShouldSetActiveEditModeToIncreaseWaterDepth()
    {
        // Act
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.IncreaseWaterDepth);
    }

    [Fact]
    public async Task IncreaseWaterDepthCommand_WhenAlreadyActive_ShouldRevertToTerrain()
    {
        // Arrange
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Act
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public async Task IsIncreaseWaterDepthActive_ShouldReflectActiveEditMode()
    {
        // Initially false
        _sut.IsIncreaseWaterDepthActive.ShouldBeFalse();

        // After activating
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();
        _sut.IsIncreaseWaterDepthActive.ShouldBeTrue();

        // After deactivating
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();
        _sut.IsIncreaseWaterDepthActive.ShouldBeFalse();
    }

    [Fact]
    public async Task DecreaseWaterDepthCommand_ShouldSetActiveEditModeToDecreaseWaterDepth()
    {
        // Act
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.DecreaseWaterDepth);
    }

    [Fact]
    public async Task DecreaseWaterDepthCommand_WhenAlreadyActive_ShouldRevertToTerrain()
    {
        // Arrange
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        // Act
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
    }

    [Fact]
    public async Task IsDecreaseWaterDepthActive_ShouldReflectActiveEditMode()
    {
        // Initially false
        _sut.IsDecreaseWaterDepthActive.ShouldBeFalse();

        // After activating
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();
        _sut.IsDecreaseWaterDepthActive.ShouldBeTrue();

        // After deactivating
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();
        _sut.IsDecreaseWaterDepthActive.ShouldBeFalse();
    }

    [Fact]
    public async Task SwitchingFromIncreaseWaterDepthToDecreaseWaterDepth_ShouldChangeActiveMode()
    {
        // Arrange
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();
        _sut.ActiveEditMode.ShouldBe(ToolType.IncreaseWaterDepth);

        // Act
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.DecreaseWaterDepth);
        _sut.IsDecreaseWaterDepthActive.ShouldBeTrue();
        _sut.IsIncreaseWaterDepthActive.ShouldBeFalse();
    }

    [Fact]
    public async Task SwitchingFromDecreaseWaterDepthToIncreaseWaterDepth_ShouldChangeActiveMode()
    {
        // Arrange
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();
        _sut.ActiveEditMode.ShouldBe(ToolType.DecreaseWaterDepth);

        // Act
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.IncreaseWaterDepth);
        _sut.IsIncreaseWaterDepthActive.ShouldBeTrue();
        _sut.IsDecreaseWaterDepthActive.ShouldBeFalse();
    }

    // --- Water Depth Tool Selection Tests ---
    [Fact]
    public void SelectedTool_WhenSetToIncreaseWaterDepth_ShouldSetActiveEditModeToIncreaseWaterDepth()
    {
        // Arrange
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var waterDepthTool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);

        // Act
        _sut.SelectedTool = waterDepthTool;

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.IncreaseWaterDepth);
    }

    [Fact]
    public void SelectedTool_WhenSetToDecreaseWaterDepth_ShouldSetActiveEditModeToDecreaseWaterDepth()
    {
        // Arrange
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var waterDepthTool = _sut.AvailableTools.First(t => t.Type == ToolType.DecreaseWaterDepth);

        // Act
        _sut.SelectedTool = waterDepthTool;

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.DecreaseWaterDepth);
    }

    [Fact]
    public void SelectedTool_WhenSetToTerrainInWaterDepthMode_ShouldRevertToTerrainMode()
    {
        // Arrange
        var map = new BattleMap(1, 1);
        _sut.Initialize(map);
        var increaseDepthTool = _sut.AvailableTools.First(t => t.Type == ToolType.IncreaseWaterDepth);
        _sut.SelectedTool = increaseDepthTool;
        _sut.ActiveEditMode.ShouldBe(ToolType.IncreaseWaterDepth);
        var terrainTool = _sut.AvailableTools.First(t => t.Type == ToolType.Terrain);

        // Act
        _sut.SelectedTool = terrainTool;

        // Assert
        _sut.ActiveEditMode.ShouldBe(ToolType.Terrain);
        _sut.SelectedTerrain.ShouldBe(terrainTool.Terrain);
    }

    // --- Water Depth Hex Selection Tests ---
    [Fact]
    public async Task HandleHexSelection_InIncreaseWaterDepthMode_ShouldReturnNewHexWithIncreasedDepth()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0)); // shallow water
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        var waterTerrain = newHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-1); // depth increased from 0 to -1
    }

    [Fact]
    public async Task HandleHexSelection_InDecreaseWaterDepthMode_ShouldReturnNewHexWithDecreasedDepth()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-1)); // standard depth
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        var waterTerrain = newHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(0); // depth decreased from -1 to 0
    }

    [Fact]
    public async Task HandleHexSelection_InIncreaseWaterDepthMode_ShouldPreserveNonWaterTerrains()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new LightWoodsTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        newHex.GetTerrains().ShouldContain(t => t is LightWoodsTerrain);
        newHex.GetTerrains().ShouldContain(t => t is WaterTerrain);
    }

    [Fact]
    public async Task HandleHexSelection_InDecreaseWaterDepthMode_WhenHexHasNoWater_ShouldNotAddWater()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new ClearTerrain());
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        newHex.HasTerrain(MakaMekTerrains.Water).ShouldBeFalse();
        newHex.GetTerrains().ShouldContain(t => t is ClearTerrain);
    }

    [Fact]
    public async Task HandleHexSelection_InIncreaseWaterDepthMode_ShouldReplaceHexInMap()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Act
        _sut.HandleHexSelection(hex);

        // Assert
        var mapHex = map.GetHex(new HexCoordinates(1, 1));
        mapHex.ShouldNotBeNull();
        var waterTerrain = mapHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-1);
    }

    [Fact]
    public async Task HandleHexSelection_InIncreaseWaterDepthMode_WhenMapIsNull_ShouldReturnNull()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(0));
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Act
        var result = _sut.HandleHexSelection(hex);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleHexSelection_InIncreaseWaterDepthMode_WhenDepthAlreadyNegative_ShouldDecreaseFurther()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(-2)); // already deep
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.IncreaseWaterDepthCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        var waterTerrain = newHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(-3); // deeper still
    }

    [Fact]
    public async Task HandleHexSelection_InDecreaseWaterDepthMode_WhenDepthIsZero_ShouldNotBecomePositive()
    {
        // Arrange
        var map = new BattleMap(3, 3);
        var hex = new Hex(new HexCoordinates(1, 1));
        hex.AddTerrain(new WaterTerrain(0));
        map.AddHex(hex);
        _sut.Initialize(map);
        await _sut.DecreaseWaterDepthCommand.ExecuteAsync();

        // Act
        var newHex = _sut.HandleHexSelection(hex);

        // Assert
        newHex.ShouldNotBeNull();
        var waterTerrain = newHex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain;
        waterTerrain.ShouldNotBeNull();
        waterTerrain.Height.ShouldBe(0); // should not become positive
    }
}
