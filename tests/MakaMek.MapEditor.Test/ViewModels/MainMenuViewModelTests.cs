using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Assets.Services;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MVVM.Core.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class MainMenuViewModelTests
{
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly MainMenuViewModel _sut;
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly ILogger<MainMenuViewModel> _mainMenuLogger = Substitute.For<ILogger<MainMenuViewModel>>();
    private readonly ITerrainAssetService _assetService = Substitute.For<ITerrainAssetService>();

    public MainMenuViewModelTests()
    {
        _localizationService.GetString(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());
        _sut = new MainMenuViewModel(_mainMenuLogger, _assetService, _localizationService);
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public void Constructor_ShouldNotStartPreloadingImmediately()
    {
        _assetService.DidNotReceive().GetLoadedBiomes();
    }

    [Fact]
    public void AttachHandlers_ShouldStartPreloading()
    {
        _sut.AttachHandlers();

        _assetService.Received(1).GetLoadedBiomes();
    }

    [Fact]
    public async Task AttachHandlers_WhenIsLoading_ShouldNotStartPreloadingAgain()
    {
        var tcs = new TaskCompletionSource<IEnumerable<string>>();
        _assetService.GetLoadedBiomes().Returns(tcs.Task);

        _sut.AttachHandlers();
        _sut.AttachHandlers();

        tcs.SetResult([]);
        await Task.Delay(100);

        await _assetService.Received(1).GetLoadedBiomes();
    }

    [Fact]
    public async Task AttachHandlers_WhenBiomesAlreadyLoaded_ShouldNotStartPreloadingAgain()
    {
        var biomes = new[] { "biome1" };
        _assetService.GetLoadedBiomes().Returns(biomes);

        _sut.AttachHandlers();
        await Task.Delay(100);

        _sut.AttachHandlers();

        await _assetService.Received(1).GetLoadedBiomes();
    }

    [Fact]
    public async Task PreloadBiomes_WhenBiomesExist_ShouldNavigateToNewMapViewModel()
    {
        var biomes = new[] { "biome1", "biome2" };
        _assetService.GetLoadedBiomes().Returns(biomes);
        _localizationService.GetString("Status_BiomesLoaded").Returns("{0} biomes loaded");

        _sut.AttachHandlers();
        await Task.Delay(100);

        await _navigationService.Received(1).NavigateToViewModelAsync<NewMapViewModel>();
        _sut.BiomeLoadingStatus.ShouldContain("2 biomes loaded");
        _sut.HasError.ShouldBeFalse();
        _sut.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task PreloadBiomes_WhenNoBiomesExist_ShouldSetErrorStatus()
    {
        var biomes = Array.Empty<string>();
        _assetService.GetLoadedBiomes().Returns(biomes);
        _localizationService.GetString("Status_NoBiomesFound").Returns("No biomes found");
        _localizationService.GetString("Status_ErrorLoadingBiomes").Returns("Error loading biomes: {0}");

        _sut.AttachHandlers();
        await Task.Delay(100);

        _sut.BiomeLoadingStatus.ShouldContain("No biomes found");
        _sut.HasError.ShouldBeTrue();
        _sut.IsLoading.ShouldBeFalse();
        await _navigationService.DidNotReceive().NavigateToViewModelAsync<NewMapViewModel>();
    }

    [Fact]
    public async Task PreloadBiomes_WhenExceptionOccurs_ShouldSetErrorStatus()
    {
        _assetService.GetLoadedBiomes()
            .Returns(Task.FromException<IEnumerable<string>>(new InvalidOperationException("Service unavailable")));
        _localizationService.GetString("Status_ErrorLoadingBiomes").Returns("Error loading biomes: {0}");

        _sut.AttachHandlers();
        await Task.Delay(100);

        _sut.BiomeLoadingStatus.ShouldContain("Error loading biomes: Service unavailable");
        _sut.HasError.ShouldBeTrue();
        _sut.IsLoading.ShouldBeFalse();
        await _navigationService.DidNotReceive().NavigateToViewModelAsync<NewMapViewModel>();
    }
}
