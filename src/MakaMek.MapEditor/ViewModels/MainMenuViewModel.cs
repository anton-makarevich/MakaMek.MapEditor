using System.Text.Json;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class MainMenuViewModel : BaseViewModel
{
    private readonly IFileService _fileService;
    private readonly IBattleMapFactory _mapFactory;
    private readonly ILogger<MainMenuViewModel> _logger;

    public MainMenuViewModel(IFileService fileService, IBattleMapFactory mapFactory, ILogger<MainMenuViewModel> logger)
    {
        _fileService = fileService;
        _mapFactory = mapFactory;
        _logger = logger;
    }

    public IAsyncCommand CreateNewMapCommand => field ??= new AsyncCommand(() => 
        NavigationService.NavigateToViewModelAsync<NewMapViewModel>());

    public IAsyncCommand LoadMapCommand => field ??= new AsyncCommand(async () =>
    {
        try
        {
            var content = (await _fileService.OpenFile("Load Map")).Content;
            if (string.IsNullOrEmpty(content)) return;
            var data = JsonSerializer.Deserialize<List<HexData>>(content);
            if (data != null)
            {
                var map = _mapFactory.CreateFromData(data);
                var editViewModel = NavigationService.GetViewModel<EditMapViewModel>();
                if (editViewModel != null)
                {
                    editViewModel.Initialize(map);
                    await NavigationService.NavigateToViewModelAsync(editViewModel);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load map");
        }
    });
}
