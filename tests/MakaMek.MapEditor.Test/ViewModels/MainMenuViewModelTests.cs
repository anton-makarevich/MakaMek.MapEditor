using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class MainMenuViewModelTests
{
    private readonly IBattleMapFactory _mapFactory = Substitute.For<IBattleMapFactory>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly MainMenuViewModel _sut;
    private readonly ILogger<EditMapViewModel> _editViewLogger = Substitute.For<ILogger<EditMapViewModel>>();
    private readonly ILogger<MainMenuViewModel> _mainMenuLogger = Substitute.For<ILogger<MainMenuViewModel>>();
    private readonly ITerrainAssetService _assetService = Substitute.For<ITerrainAssetService>();
    private readonly IFileService _fileService = Substitute.For<IFileService>();

    public MainMenuViewModelTests()
    {
        _sut = new MainMenuViewModel(_fileService, _mapFactory, _mainMenuLogger, _assetService);
        _sut.SetNavigationService(_navigationService);
    }

    private static BattleMapData BuildTestBattleMapData()
    {
        return new BattleMapData
        {
            Biome = "test",
            HexData =
            [
                new HexData
                {
                    Coordinates = new HexCoordinateData(0, 0),
                    TerrainTypes =
                    [
                        MakaMekTerrains.Clear
                    ]
                },
                new HexData
                {
                    Coordinates = new HexCoordinateData(1, 0),
                    TerrainTypes =
                    [
                        MakaMekTerrains.Clear
                    ]
                }
            ]
        };
    }

    private static BattleMapData BuildTestBattleMapDataWithSingleHex()
    {
        return new BattleMapData
        {
            Biome = "test",
            HexData =
            [
                new HexData
                {
                    Coordinates = new HexCoordinateData(0, 0),
                    TerrainTypes =
                    [
                        MakaMekTerrains.Clear
                    ]
                }
            ]
        };
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
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<BattleMapData>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenFileContentIsNull_ShouldNotProcessFile()
    {
        // Arrange
        _fileService.OpenFile("Load Map").Returns(("",(string?)null));

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<BattleMapData>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldDeserializeAndCreateMap()
    {
        // Arrange
        var hexData = BuildTestBattleMapData();
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<BattleMapData>()).Returns(map);

        // Act
        await _sut.LoadMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.Received(1).CreateFromData(Arg.Is<BattleMapData>(data => data.HexData.Count == 2));
    }

    [Fact]
    public async Task LoadMapCommand_WhenValidContent_ShouldInitializeEditViewModel()
    {
        // Arrange
        var hexData = BuildTestBattleMapDataWithSingleHex();
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _assetService, _editViewLogger);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<BattleMapData>()).Returns(map);
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
        var hexData = BuildTestBattleMapDataWithSingleHex();
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _assetService, _editViewLogger);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<BattleMapData>()).Returns(map);
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
        var hexData = BuildTestBattleMapDataWithSingleHex();
        var json = JsonSerializer.Serialize(hexData);
        var map = new BattleMap(1,1);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<BattleMapData>()).Returns(map);
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
        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<BattleMapData>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenInvalidJson_ShouldHandleExceptionGracefully()
    {
        // Arrange
        _fileService.OpenFile("Load Map").Returns(("","invalid json {{{"));

        // Act & Assert - Should not throw
        await _sut.LoadMapCommand.ExecuteAsync();

        _mapFactory.DidNotReceive().CreateFromData(Arg.Any<BattleMapData>());
    }

    [Fact]
    public async Task LoadMapCommand_WhenMapFactoryThrows_ShouldHandleExceptionGracefully()
    {
        // Arrange
        var hexData = BuildTestBattleMapDataWithSingleHex();
        var json = JsonSerializer.Serialize(hexData);

        _fileService.OpenFile("Load Map").Returns(("",json));
        _mapFactory.CreateFromData(Arg.Any<BattleMapData>())
            .Returns(_ => throw new InvalidOperationException("Test exception"));

        // Act & Assert - Should not throw
        await _sut.LoadMapCommand.ExecuteAsync();

        await _navigationService.DidNotReceive().NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
    }

    [Fact]
    public void Constructor_ShouldInitializePreloading()
    {
        // Assert
        _assetService.Received(1).GetLoadedBiomes();
    }

    [Fact]
    public async Task PreloadBiomes_WhenBiomesExist_ShouldSetSuccessStatus()
    {
        // Arrange
        var biomes = new[] { "biome1", "biome2" };
        _assetService.GetLoadedBiomes().Returns(biomes);
        
        // Create a new instance to trigger preloading
        var viewModel = new MainMenuViewModel(_fileService, _mapFactory, _mainMenuLogger, _assetService);
        
        // Wait for the async operation to complete
        await Task.Delay(100);
        
        // Assert
        viewModel.BiomeLoadingStatus.ShouldContain("2 biomes loaded");
        viewModel.HasError.ShouldBeFalse();
        viewModel.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task PreloadBiomes_WhenNoBiomesExist_ShouldSetErrorStatus()
    {
        // Arrange
        var biomes = Array.Empty<string>();
        _assetService.GetLoadedBiomes().Returns(biomes);
        
        // Create a new instance to trigger preloading
        var viewModel = new MainMenuViewModel(_fileService, _mapFactory, _mainMenuLogger, _assetService);
        
        // Wait for the async operation to complete
        await Task.Delay(100);
        
        // Assert
        viewModel.BiomeLoadingStatus.ShouldContain("No biomes found");
        viewModel.HasError.ShouldBeTrue();
        viewModel.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task PreloadBiomes_WhenExceptionOccurs_ShouldSetErrorStatus()
    {
        // Arrange
        _assetService.GetLoadedBiomes().Returns(Task.FromException<IEnumerable<string>>(new InvalidOperationException("Service unavailable")));
        
        // Create a new instance to trigger preloading
        var viewModel = new MainMenuViewModel(_fileService, _mapFactory, _mainMenuLogger, _assetService);
        
        // Wait for the async operation to complete
        await Task.Delay(100);
        
        // Assert
        viewModel.BiomeLoadingStatus.ShouldContain("Error loading biomes: Service unavailable");
        viewModel.HasError.ShouldBeTrue();
        viewModel.IsLoading.ShouldBeFalse();
    }
}
