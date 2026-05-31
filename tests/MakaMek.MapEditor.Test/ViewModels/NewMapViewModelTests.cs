using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class NewMapViewModelTests
{
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly NewMapViewModel _sut;
    private readonly IBattleMapFactory _mapFactory = Substitute.For<IBattleMapFactory>();
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IMapPreviewRenderer _previewRenderer = Substitute.For<IMapPreviewRenderer>();
    private readonly IMapResourceProvider _mapResourceProvider = Substitute.For<IMapResourceProvider>();
    private readonly IFileService _fileService = Substitute.For<IFileService>();
    private readonly ILogger<NewMapViewModel> _logger = Substitute.For<ILogger<NewMapViewModel>>();
    private readonly IDispatcherService _dispatcherService = Substitute.For<IDispatcherService>();

    public NewMapViewModelTests()
    {
        _sut = new NewMapViewModel(
            _previewRenderer,
            _mapFactory,
            _mapResourceProvider,
            _fileService,
            _logger,
            _dispatcherService,
            _localizationService);
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public void Constructor_ShouldCreateMapConfig()
    {
        _sut.MapConfig.ShouldNotBeNull();
    }

    [Fact]
    public void EditMapCommand_ShouldNotBeNull()
    {
        _sut.EditMapCommand.ShouldNotBeNull();
    }

    [Fact]
    public async Task EditMapCommand_WhenMapIsNull_ShouldNotNavigate()
    {
        await _sut.EditMapCommand.ExecuteAsync();

        await _navigationService.DidNotReceiveWithAnyArgs().NavigateToViewModelAsync<EditMapViewModel>();
    }

    [Fact]
    public async Task EditMapCommand_WhenMapIsNull_ShouldNotGetEditViewModel()
    {
        await _sut.EditMapCommand.ExecuteAsync();

        _navigationService.DidNotReceive().GetViewModel<EditMapViewModel>();
    }

    [Fact]
    public void DetachHandlers_ShouldNotThrow()
    {
        Should.NotThrow(() => _sut.DetachHandlers());
    }
}
