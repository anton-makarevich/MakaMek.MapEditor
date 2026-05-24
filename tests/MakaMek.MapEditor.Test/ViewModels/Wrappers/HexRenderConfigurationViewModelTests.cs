using Sanet.MakaMek.MapEditor.ViewModels.Wrappers;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels.Wrappers;

public class HexRenderConfigurationViewModelTests
{
    private readonly HexRenderConfigurationViewModel _sut = new();

    [Fact]
    public void ShowLabels_DefaultValue_ShouldBeTrue()
    {
        _sut.ShowLabels.ShouldBeTrue();
    }

    [Fact]
    public void ShowOutline_DefaultValue_ShouldBeTrue()
    {
        _sut.ShowOutline.ShouldBeTrue();
    }

    [Fact]
    public void ToConfiguration_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        _sut.ShowLabels = false;
        _sut.ShowOutline = false;

        // Act
        var config = _sut.ToConfiguration();

        // Assert
        config.ShowLabels.ShouldBeFalse();
        config.ShowOutline.ShouldBeFalse();
        config.ShowHighlightLabels.ShouldBeFalse();
    }

    [Fact]
    public void ToConfiguration_WithTrueValues_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        _sut.ShowLabels = true;
        _sut.ShowOutline = true;

        // Act
        var config = _sut.ToConfiguration();

        // Assert
        config.ShowLabels.ShouldBeTrue();
        config.ShowOutline.ShouldBeTrue();
        config.ShowHighlightLabels.ShouldBeFalse();
    }

    [Fact]
    public void ShowLabels_WhenChanged_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(HexRenderConfigurationViewModel.ShowLabels))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        _sut.ShowLabels = false;

        // Assert
        propertyChangedRaised.ShouldBeTrue();
    }

    [Fact]
    public void ShowOutline_WhenChanged_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(HexRenderConfigurationViewModel.ShowOutline))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        _sut.ShowOutline = false;

        // Assert
        propertyChangedRaised.ShouldBeTrue();
    }
}
