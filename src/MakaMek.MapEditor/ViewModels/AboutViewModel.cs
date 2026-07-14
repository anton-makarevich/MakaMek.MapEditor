using System.Reflection;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Localization;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class AboutViewModel : BaseViewModel
{
    private readonly IExternalNavigationService _externalNavigationService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<AboutViewModel> _logger;

    public AboutViewModel(
        IExternalNavigationService externalNavigationService,
        ILocalizationService localizationService,
        ILogger<AboutViewModel> logger)
    {
        _externalNavigationService = externalNavigationService;
        _localizationService = localizationService;
        _logger = logger;
    }

    public string Version
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(infoVersion))
                return $"v{infoVersion}";
            return assembly.GetName().Version?.ToString() ?? "NA";
        }
    }

    public string Title => _localizationService.GetString("About_Title");
    public string Description => _localizationService.GetString("About_Description");
    public string Attribution => _localizationService.GetString("About_Attribution");
    public string FossStatement => _localizationService.GetString("About_FossStatement");
    public string TrademarkDisclaimer => _localizationService.GetString("About_TrademarkDisclaimer");

    public IAsyncCommand OpenGitHubCommand => field ??= new AsyncCommand(async () =>
    {
        try
        {
            await _externalNavigationService.OpenUrlAsync("https://github.com/anton-makarevich/MakaMek.MapEditor");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open GitHub repository");
        }
    }, onException: ex => _logger.LogError(ex, "Failed to open GitHub repository"));

    public IAsyncCommand OpenContactCommand => field ??= new AsyncCommand(async () =>
    {
        try
        {
            await _externalNavigationService.OpenEmailAsync("makarevich.software@gmail.com", "MakaMek Map Editor");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open contact email");
        }
    }, onException: ex => _logger.LogError(ex, "Failed to open contact email"));

    public IAsyncCommand CloseCommand => field ??= new AsyncCommand(async () =>
    {
        try
        {
            await NavigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate back");
        }
    }, onException: ex => _logger.LogError(ex, "Failed to navigate back"));
}
