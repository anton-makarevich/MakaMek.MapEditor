using System.Text.Json;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Factories;
using Sanet.MakaMek.MapEditor.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels;

public class MainMenuViewModel : BaseViewModel
{
    private readonly IFileService _fileService;
    private readonly IBattleMapFactory _mapFactory;

    public MainMenuViewModel(IFileService fileService, IBattleMapFactory mapFactory)
    {
        _fileService = fileService;
        _mapFactory = mapFactory;
    }

    public ICommand CreateNewMapCommand => field ??= new AsyncCommand(() => 
        NavigationService.NavigateToViewModelAsync<NewMapViewModel>());

    public ICommand LoadMapCommand => field ??= new AsyncCommand(async () =>
    {
        var content = await _fileService.OpenFileAsync("Load Map");
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
    });
}
