using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class EditMapViewModelTests
{
    private readonly IFileService _fileService = Substitute.For<IFileService>();
    private readonly ITerrainAssetService _assetService = Substitute.For<ITerrainAssetService>();
    private readonly EditMapViewModel _sut;
    private readonly ILogger<EditMapViewModel> _logger = Substitute.For<ILogger<EditMapViewModel>>();

    public EditMapViewModelTests()
    {
        _sut = new EditMapViewModel(_fileService, _assetService, _logger);
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
                    TerrainTypes =
                    [
                        MakaMekTerrains.Clear
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
            "Export Map",
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
        _sut.ActiveEditMode.ShouldBe(EditMode.Terrain);
    }

    [Fact]
    public async Task RaiseLevelCommand_ShouldSetActiveEditModeToRaiseLevel()
    {
        // Act
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(EditMode.RaiseLevel);
    }

    [Fact]
    public async Task RaiseLevelCommand_WhenAlreadyActive_ShouldRevertToTerrain()
    {
        // Arrange
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Act
        await _sut.RaiseLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(EditMode.Terrain);
    }

    [Fact]
    public async Task LowerLevelCommand_ShouldSetActiveEditModeToLowerLevel()
    {
        // Act
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(EditMode.LowerLevel);
    }

    [Fact]
    public async Task LowerLevelCommand_WhenAlreadyActive_ShouldRevertToTerrain()
    {
        // Arrange
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Act
        await _sut.LowerLevelCommand.ExecuteAsync();

        // Assert
        _sut.ActiveEditMode.ShouldBe(EditMode.Terrain);
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
}
