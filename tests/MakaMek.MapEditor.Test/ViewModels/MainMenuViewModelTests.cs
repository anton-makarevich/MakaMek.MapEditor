using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.Services;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class MainMenuViewModelTests
{
    private readonly IBattleMapFactory _mapFactory = Substitute.For<IBattleMapFactory>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly MainMenuViewModel _sut;
    private readonly ILogger<EditMapViewModel> _logger = Substitute.For<ILogger<EditMapViewModel>>();
    private readonly IImageService _imageService = Substitute.For<IImageService>();
    private readonly IFileService _fileService = Substitute.For<IFileService>();

    public MainMenuViewModelTests()
    {
        _sut = new MainMenuViewModel(_fileService, _mapFactory);
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public async Task CreateNewMapCommand_ShouldNavigateToNewMapViewModel()
    {
        // Act
        await _sut.CreateNewMapCommand.ExecuteAsync();

        // Assert
        await _navigationService.Received(1).NavigateToViewModelAsync<NewMapViewModel>();
    }

    [Fact]
    public async Task LoadMapCommand_WhenFileContentIsEmpty_ShouldNotProcessFile()
    {
        // Arrange
        _fileService.OpenFile("Load Map").Returns(("",""));

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenFileContentIsNull_ShouldNotProcessFile()
    {
        // Arrange
        _fileService.OpenFile("Load Map").Returns(("",(string?)null));

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldDeserializeAndCreateMap()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] },
            new() { Coordinates = new HexCoordinateData(1, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.Received(1).CreateFromData(Arg.Is<List<HexData>>(data => data.Count == 2));
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldInitializeEditViewModel()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _imageService, _logger);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        editViewModel.Received(1).Initialize(map);
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldNavigateToEditViewModel()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _imageService, _logger);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        await _navigationService.Received(1).NavigateToViewModelAsync(editViewModel);
    }

    [Fact]
    public async Task LoadMapCommand_WhenEditViewModelIsNull_ShouldNotNavigate()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ] }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns((EditMapViewModel?)null);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        await _navigationService.DidNotReceive().NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenDeserializationReturnsNull_ShouldNotCreateMap()
    {
        // Arrange
        var json = "null";
        _fileService.OpenFile("Load Map").Returns(("",json));

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenInvalidJson_ShouldHandleExceptionGracefully()
    {
        // Arrange
        _fileService.OpenFile("Load Map").Returns(("","invalid json {{{"));

        // Act & Assert - Should not throw
        await _sut.LoadMapCommand.ExecuteAsync();

        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenMapFactoryThrows_ShouldHandleExceptionGracefully()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new()
            {
                Coordinates = new HexCoordinateData(0, 0),
                TerrainTypes =
                [
                    MakaMekTerrains.Clear
                ]
            }
        };
        var json = JsonSerializer.Serialize(hexData);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>())
            .Returns(_ => throw new InvalidOperationException("Test exception"));

        // Act & Assert - Should not throw
        await _sut.LoadMapCommand.ExecuteAsync();

        await _navigationService.DidNotReceive().NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
    }
}
