using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Map.Generators;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class NewMapViewModelTests
{
    private readonly IBattleMapFactory _mapFactory = Substitute.For<IBattleMapFactory>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly NewMapViewModel _sut;
    private readonly ILogger<EditMapViewModel> _logger = Substitute.For<ILogger<EditMapViewModel>>();
    private readonly IImageService _imageService = Substitute.For<IImageService>();
    private readonly IFileService _fileService = Substitute.For<IFileService>();

    public NewMapViewModelTests()
    {
        _sut = new NewMapViewModel(_mapFactory);
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public void MapWidthMin_ShouldReturn5()
    {
        _sut.MapWidthMin.ShouldBe(5);
    }

    [Fact]
    public void MapWidthMax_ShouldReturn30()
    {
        _sut.MapWidthMax.ShouldBe(30);
    }

    [Fact]
    public void MapHeightMin_ShouldReturn6()
    {
        _sut.MapHeightMin.ShouldBe(6);
    }

    [Fact]
    public void MapHeightMax_ShouldReturn34()
    {
        _sut.MapHeightMax.ShouldBe(34);
    }

    [Fact]
    public void MapWidth_DefaultValue_ShouldBe15()
    {
        _sut.MapWidth.ShouldBe(15);
    }

    [Fact]
    public void MapHeight_DefaultValue_ShouldBe17()
    {
        _sut.MapHeight.ShouldBe(17);
    }

    [Fact]
    public void ForestCoverage_DefaultValue_ShouldBe20()
    {
        _sut.ForestCoverage.ShouldBe(20);
    }

    [Fact]
    public void LightWoodsPercentage_DefaultValue_ShouldBe30()
    {
        _sut.LightWoodsPercentage.ShouldBe(30);
    }

    [Fact]
    public void IsPreGenerated_DefaultValue_ShouldBeFalse()
    {
        _sut.IsPreGenerated.ShouldBeFalse();
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(15, 15)]
    [InlineData(25, 25)]
    public void MapWidth_WhenSetWithinRange_ShouldUpdateValue(int value, int expected)
    {
        _sut.MapWidth = value;
        _sut.MapWidth.ShouldBe(expected);
    }

    [Theory]
    [InlineData(3, 5)]  // Below min
    [InlineData(35, 30)] // Above max
    public void MapWidth_WhenSetOutsideRange_ShouldClampToRange(int value, int expected)
    {
        _sut.MapWidth = value;
        _sut.MapWidth.ShouldBe(expected);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(17, 17)]
    [InlineData(30, 30)]
    public void MapHeight_WhenSetWithinRange_ShouldUpdateValue(int value, int expected)
    {
        _sut.MapHeight = value;
        _sut.MapHeight.ShouldBe(expected);
    }

    [Theory]
    [InlineData(3, 6)]   // Below min
    [InlineData(40, 34)] // Above max
    public void MapHeight_WhenSetOutsideRange_ShouldClampToRange(int value, int expected)
    {
        _sut.MapHeight = value;
        _sut.MapHeight.ShouldBe(expected);
    }

    [Fact]
    public void IsPreGenerated_WhenSet_ShouldUpdateValue()
    {
        _sut.IsPreGenerated = true;
        _sut.IsPreGenerated.ShouldBeTrue();
    }

    [Fact]
    public void ForestCoverage_WhenSet_ShouldUpdateValue()
    {
        _sut.ForestCoverage = 50;
        _sut.ForestCoverage.ShouldBe(50);
    }

    [Fact]
    public void LightWoodsPercentage_WhenSet_ShouldUpdateValue()
    {
        _sut.LightWoodsPercentage = 60;
        _sut.LightWoodsPercentage.ShouldBe(60);
    }

    [Fact]
    public async Task CreateMapCommand_WhenIsPreGeneratedIsFalse_ShouldCreateMapWithSingleTerrainGenerator()
    {
        // Arrange
        _sut.IsPreGenerated = false;
        _sut.MapWidth = 10;
        _sut.MapHeight = 12;
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _imageService, _logger);

        _mapFactory.GenerateMap(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ITerrainGenerator>())
            .Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.CreateMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.Received(1).GenerateMap(
            10,
            12,
            Arg.Is<SingleTerrainGenerator>(g => g != null));
    }

    [Fact]
    public async Task CreateMapCommand_WhenIsPreGeneratedIsTrue_ShouldCreateMapWithForestPatchesGenerator()
    {
        // Arrange
        _sut.IsPreGenerated = true;
        _sut.MapWidth = 10;
        _sut.MapHeight = 12;
        _sut.ForestCoverage = 40;
        _sut.LightWoodsPercentage = 50;
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _imageService, _logger);

        _mapFactory.GenerateMap(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ITerrainGenerator>())
            .Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.CreateMapCommand.ExecuteAsync();

        // Assert
        _mapFactory.Received(1).GenerateMap(
            10,
            12,
            Arg.Is<ForestPatchesGenerator>(g => g != null));
    }

    [Fact]
    public async Task CreateMapCommand_ShouldInitializeEditViewModelWithGeneratedMap()
    {
        // Arrange
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _imageService, _logger);

        _mapFactory.GenerateMap(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ITerrainGenerator>())
            .Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.CreateMapCommand.ExecuteAsync();

        // Assert
        editViewModel.Received(1).Initialize(map);
    }

    [Fact]
    public async Task CreateMapCommand_ShouldNavigateToEditViewModel()
    {
        // Arrange
        var map = new BattleMap(1,1);
        var editViewModel = Substitute.For<EditMapViewModel>(
            _fileService,
            _imageService, _logger);

        _mapFactory.GenerateMap(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ITerrainGenerator>())
            .Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns(editViewModel);

        // Act
        await _sut.CreateMapCommand.ExecuteAsync();

        // Assert
        await _navigationService.Received(1).NavigateToViewModelAsync(editViewModel);
    }

    [Fact]
    public async Task CreateMapCommand_WhenEditViewModelIsNull_ShouldNotNavigate()
    {
        // Arrange
        var map = new BattleMap(1,1);
        _mapFactory.GenerateMap(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ITerrainGenerator>())
            .Returns(map);
        _navigationService.GetViewModel<EditMapViewModel>().Returns((EditMapViewModel?)null);

        // Act
        await _sut.CreateMapCommand.ExecuteAsync();

        // Assert
        await _navigationService.DidNotReceive().NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
    }
}
