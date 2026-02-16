using System.Collections.ObjectModel;
using System.Text.Json;
using NSubstitute;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.Services;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class EditMapViewModelTests
{
    private readonly IFileService _fileService;
    private readonly IImageService _imageService;
    private readonly EditMapViewModel _sut;

    public EditMapViewModelTests()
    {
        _fileService = Substitute.For<IFileService>();
        _imageService = Substitute.For<IImageService>();
        _sut = new EditMapViewModel(_fileService, _imageService);
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
    public void ImageService_ShouldBeAccessible()
    {
        _sut.ImageService.ShouldBe(_imageService);
    }

    [Fact]
    public void Initialize_ShouldSetMap()
    {
        // Arrange
        var map = new BattleMap();

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.Map.ShouldBe(map);
    }

    [Fact]
    public void Initialize_ShouldLoadTerrains()
    {
        // Arrange
        var map = new BattleMap();

        // Act
        _sut.Initialize(map);

        // Assert
        _sut.AvailableTerrains.ShouldNotBeEmpty();
    }

    [Fact]
    public void Initialize_ShouldSetFirstTerrainAsSelected()
    {
        // Arrange
        var map = new BattleMap();

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
        var map = new BattleMap();

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
        var map = new BattleMap();
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
        var hex = Substitute.For<Hex>(0, 0);
        _sut.SelectedTerrain = null;

        // Act
        _sut.HandleHexSelection(hex);

        // Assert
        hex.DidNotReceive().ReplaceTerrains(Arg.Any<Terrain[]>());
    }

    [Fact]
    public void HandleHexSelection_WhenSelectedTerrainIsSet_ShouldReplaceHexTerrains()
    {
        // Arrange
        var hex = Substitute.For<Hex>(0, 0);
        var terrain = new ClearTerrain();
        _sut.SelectedTerrain = terrain;

        // Act
        _sut.HandleHexSelection(hex);

        // Assert
        hex.Received(1).ReplaceTerrains(Arg.Is<Terrain[]>(t => t.Length == 1 && t[0] == terrain));
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsNull_ShouldNotSaveFile()
    {
        // Act
        await _sut.ExportMapCommand.ExecuteAsync(null);

        // Assert
        await _fileService.DidNotReceive().SaveFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsSet_ShouldConvertMapToData()
    {
        // Arrange
        var map = Substitute.For<BattleMap>();
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        // Act
        await _sut.ExportMapCommand.ExecuteAsync(null);

        // Assert
        map.Received(1).ToData();
    }

    [Fact]
    public async Task ExportMapCommand_WhenMapIsSet_ShouldSerializeToJson()
    {
        // Arrange
        var map = Substitute.For<BattleMap>();
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        string? savedContent = null;
        await _fileService.SaveFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(content => savedContent = content));

        // Act
        await _sut.ExportMapCommand.ExecuteAsync(null);

        // Assert
        savedContent.ShouldNotBeNull();
        var deserializedData = JsonSerializer.Deserialize<List<HexData>>(savedContent);
        deserializedData.ShouldNotBeNull();
        deserializedData.Count.ShouldBe(1);
        deserializedData[0].X.ShouldBe(0);
        deserializedData[0].Y.ShouldBe(0);
    }

    [Fact]
    public async Task ExportMapCommand_ShouldSaveWithCorrectParameters()
    {
        // Arrange
        var map = Substitute.For<BattleMap>();
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        // Act
        await _sut.ExportMapCommand.ExecuteAsync(null);

        // Assert
        await _fileService.Received(1).SaveFileAsync(
            "Export Map",
            "map.json",
            Arg.Any<string>());
    }

    [Fact]
    public async Task ExportMapCommand_ShouldSerializeWithIndentation()
    {
        // Arrange
        var map = Substitute.For<BattleMap>();
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        map.ToData().Returns(hexData);
        _sut.Initialize(map);

        string? savedContent = null;
        await _fileService.SaveFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(content => savedContent = content));

        // Act
        await _sut.ExportMapCommand.ExecuteAsync(null);

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
        var hex1 = Substitute.For<Hex>(0, 0);
        var hex2 = Substitute.For<Hex>(1, 1);
        var terrain = new ClearTerrain();
        _sut.SelectedTerrain = terrain;

        // Act
        _sut.HandleHexSelection(hex1);
        _sut.HandleHexSelection(hex2);

        // Assert
        hex1.Received(1).ReplaceTerrains(Arg.Any<Terrain[]>());
        hex2.Received(1).ReplaceTerrains(Arg.Any<Terrain[]>());
    }
}
