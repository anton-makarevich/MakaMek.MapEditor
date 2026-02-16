using System.Text.Json;
using NSubstitute;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.MapEditor.Services;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MVVM.Core.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class MainMenuViewModelTests
{
    private readonly IFileService _fileService;
    private readonly IBattleMapFactory _mapFactory;
    private readonly INavigationService _navigationService;
    private readonly MainMenuViewModel _sut;

    public MainMenuViewModelTests()
    {
        _fileService = Substitute.For<IFileService>();
        _mapFactory = Substitute.For<IBattleMapFactory>();
        _navigationService = Substitute.For<INavigationService>();
        _sut = new MainMenuViewModel(_fileService, _mapFactory);
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public async Task CreateNewMapCommand_ShouldNavigateToNewMapViewModel()
    {
        // Act
        await _sut.CreateNewMapCommand.ExecuteAsync(null);

        // Assert
        await _navigationService.Received(1).NavigateToViewModelAsync<NewMapViewModel>();
    }

    [Fact]
    public async Task LoadMapCommand_WhenFileContentIsEmpty_ShouldNotProcessFile()
    {
        // Arrange
        _fileService.OpenFileAsync("Load Map").Returns(string.Empty);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync(null);

        // Assert
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenFileContentIsNull_ShouldNotProcessFile()
    {
        // Arrange
        _fileService.OpenFileAsync("Load Map").Returns((string?)null);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync(null);

        // Assert
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldDeserializeAndCreateMap()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } },
            new() { X = 1, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap();

        _fileService.OpenFileAsync("Load Map").Returns(json);
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync(null);

        // Assert
        _mapFactory.Received(1).CreateFromData(Arg.Is<List<HexData>>(data => data.Count == 2));
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldInitializeEditViewModel()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap();
        var editViewModel = Substitute.For<EditMapViewModel>(
            Substitute.For<IFileService>(),
            Substitute.For<IImageService>());

        _fileService.OpenFileAsync("Load Map").Returns(json);
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync(null);

        // Assert
        editViewModel.Received(1).Initialize(map);
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldNavigateToEditViewModel()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap();
        var editViewModel = Substitute.For<EditMapViewModel>(
            Substitute.For<IFileService>(),
            Substitute.For<IImageService>());

        _fileService.OpenFileAsync("Load Map").Returns(json);
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync(null);

        // Assert
        await _navigationService.Received(1).NavigateToViewModelAsync(editViewModel);
    }

    [Fact]
    public async Task LoadMapCommand_WhenEditViewModelIsNull_ShouldNotNavigate()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap();

        _fileService.OpenFileAsync("Load Map").Returns(json);
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>()).Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns((EditMapViewModel?)null);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync(null);

        // Assert
        await _navigationService.DidNotReceive().NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenDeserializationReturnsNull_ShouldNotCreateMap()
    {
        // Arrange
        var json = "null";
        _fileService.OpenFileAsync("Load Map").Returns(json);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync(null);

        // Assert
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenInvalidJson_ShouldHandleExceptionGracefully()
    {
        // Arrange
        _fileService.OpenFileAsync("Load Map").Returns("invalid json {{{");

        // Act & Assert - Should not throw
        await _sut.LoadMapCommand.ExecuteAsync(null);

        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<List<HexData>>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenMapFactoryThrows_ShouldHandleExceptionGracefully()
    {
        // Arrange
        var hexData = new List<HexData>
        {
            new() { X = 0, Y = 0, TerrainTypes = new List<string> { "ClearTerrain" } }
        };
        var json = JsonSerializer.Serialize(hexData);

        _fileService.OpenFileAsync("Load Map").Returns(json);
        _mapFactory.CreateFromData(Arg.Any<List<HexData>>())
            .Returns(x => throw new InvalidOperationException("Test exception"));

        // Act & Assert - Should not throw
        await _sut.LoadMapCommand.ExecuteAsync(null);

        await _navigationService.DidNotReceive().NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
    }
}
