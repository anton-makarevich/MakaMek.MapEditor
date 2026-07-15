using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MVVM.Core.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels;

public class AboutViewModelTests
{
    private readonly IExternalNavigationService _externalNavigationService = Substitute.For<IExternalNavigationService>();
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly ILogger<AboutViewModel> _logger = Substitute.For<ILogger<AboutViewModel>>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly AboutViewModel _sut;

    public AboutViewModelTests()
    {
        _localizationService.GetString(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());
        _sut = new AboutViewModel(_externalNavigationService, _localizationService, _logger);
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public void Version_ShouldNotBeNullOrEmpty()
    {
        _sut.Version.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("About_Title")]
    [InlineData("About_Description")]
    [InlineData("About_Attribution")]
    [InlineData("About_FossStatement")]
    [InlineData("About_TrademarkDisclaimer")]
    public void TextProperties_ShouldResolveViaLocalization(string key)
    {
        _localizationService.GetString(key).Returns($"resolved_{key}");

        var result = key switch
        {
            "About_Title" => _sut.Title,
            "About_Description" => _sut.Description,
            "About_Attribution" => _sut.Attribution,
            "About_FossStatement" => _sut.FossStatement,
            "About_TrademarkDisclaimer" => _sut.TrademarkDisclaimer,
            _ => null
        };

        result.ShouldBe($"resolved_{key}");
    }

    [Fact]
    public void OpenGitHubCommand_ShouldNotBeNull()
    {
        _sut.OpenGitHubCommand.ShouldNotBeNull();
    }

    [Fact]
    public void OpenContactCommand_ShouldNotBeNull()
    {
        _sut.OpenContactCommand.ShouldNotBeNull();
    }

    [Fact]
    public void CloseCommand_ShouldNotBeNull()
    {
        _sut.CloseCommand.ShouldNotBeNull();
    }

    [Fact]
    public async Task OpenGitHubCommand_ShouldOpenUrl()
    {
        await _sut.OpenGitHubCommand.ExecuteAsync();

        await _externalNavigationService.Received(1).OpenUrlAsync(
            "https://github.com/anton-makarevich/MakaMek.MapEditor");
    }

    [Fact]
    public async Task OpenContactCommand_ShouldOpenEmail()
    {
        await _sut.OpenContactCommand.ExecuteAsync();

        await _externalNavigationService.Received(1).OpenEmailAsync(
            "makarevich.software@gmail.com", "MakaMek Map Editor");
    }

    [Fact]
    public async Task CloseCommand_ShouldNavigateBack()
    {
        await _sut.CloseCommand.ExecuteAsync();

        await _navigationService.Received(1).NavigateBackAsync();
    }

    [Fact]
    public async Task OpenGitHubCommand_WhenServiceThrows_ShouldNotThrow()
    {
        _externalNavigationService.When(x => x.OpenUrlAsync(Arg.Any<string>()))
            .Do(_ => throw new Exception("network error"));

        await Should.NotThrowAsync(() => _sut.OpenGitHubCommand.ExecuteAsync());
    }

    [Fact]
    public async Task OpenContactCommand_WhenServiceThrows_ShouldNotThrow()
    {
        _externalNavigationService.When(x => x.OpenEmailAsync(Arg.Any<string>(), Arg.Any<string>()))
            .Do(_ => throw new Exception("network error"));

        await Should.NotThrowAsync(() => _sut.OpenContactCommand.ExecuteAsync());
    }
}
