using Avalonia.Markup.Xaml;
using Sanet.MakaMek.Localization;

namespace Sanet.MakaMek.MapEditor.Extensions;

public class LocalizeExtension : MarkupExtension
{
    private static ILocalizationService? _localizationService;

    public static void Initialize(ILocalizationService localization)
    {
        _localizationService = localization;
    }

    public string Key { get; set; } = string.Empty;

    public LocalizeExtension() { }

    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _localizationService?.GetString(Key) ?? Key;
    }
}
