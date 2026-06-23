using Avalonia.Data.Converters;
using Sanet.MakaMek.MapEditor.Converters;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.Converters;

public class StringToImageConverterTests
{
    private readonly StringToImageConverter _sut = new();

    [Fact]
    public void Convert_NullValue_ReturnsNull()
    {
        var result = _sut.Convert(null, typeof(object), null, null);

        result.ShouldBeNull();
    }

    [Fact]
    public void Convert_EmptyString_ReturnsNull()
    {
        var result = _sut.Convert(string.Empty, typeof(object), null, null);

        result.ShouldBeNull();
    }

    [Fact]
    public void Convert_NonStringValue_ReturnsNull()
    {
        var result = _sut.Convert(123, typeof(object), null, null);

        result.ShouldBeNull();
    }

    [Fact]
    public void Convert_InvalidUri_ReturnsNull()
    {
        var result = _sut.Convert("not-a-valid-uri", typeof(object), null, null);

        result.ShouldBeNull();
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotSupportedException()
    {
        Should.Throw<NotSupportedException>(() =>
            _sut.ConvertBack(null, typeof(object), null, null));
    }
}
