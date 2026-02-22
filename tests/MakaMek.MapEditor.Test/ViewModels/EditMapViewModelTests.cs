using System.Collections.ObjectModel;
using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
    private readonly IImageService _imageService = Substitute.For<IImageService>();
    private readonly EditMapViewModel _sut;
    private readonly ILogger<EditMapViewModel> _logger = Substitute.For<ILogger<EditMapViewModel>>();

    public EditMapViewModelTests()
    {
        _sut = new EditMapViewModel(_fileService, _imageService, _logger);
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
    public void ImageService_ShouldBeAccessible()
    {
        _sut.ImageService.ShouldBe(_imageService);
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
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
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
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(2, 3),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
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
        var deserializedData = JsonSerializer.Deserialize<List<HexData>>(savedContent);
        deserializedData.ShouldNotBeNull();
        deserializedData.Count.ShouldBe(1);
        deserializedData[0].Coordinates.Q.ShouldBe(2);
        deserializedData[0].Coordinates.R.ShouldBe(3);
    }

    [Fact]
    public async Task ExportMapCommand_ShouldSaveWithCorrectParameters()
    {
        // Arrange
        var map = Substitute.For<IBattleMap>();
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
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
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
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
}
