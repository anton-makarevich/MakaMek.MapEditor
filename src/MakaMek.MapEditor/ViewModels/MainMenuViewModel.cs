using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Map.Data;
using Sanet.MVVM.Core.ViewModels;
using Sanet.MVVM.Core.Services;

namespace MakaMek.MapEditor.ViewModels;

public class MainMenuViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly Services.IFileService _fileService;
    private readonly Sanet.MakaMek.Map.Factories.IBattleMapFactory _mapFactory;

    public MainMenuViewModel(INavigationService navigationService, Services.IFileService fileService, Sanet.MakaMek.Map.Factories.IBattleMapFactory mapFactory)
    {
        _navigationService = navigationService;
        _fileService = fileService;
        _mapFactory = mapFactory;
    }

    public ICommand CreateNewMapCommand => field ??= new AsyncCommand(() => 
        _navigationService.NavigateToViewModelAsync<NewMapViewModel>());

    public ICommand LoadMapCommand => field ??= new AsyncCommand(async () =>
    {
        var content = await _fileService.OpenFileAsync("Load Map");
        if (string.IsNullOrEmpty(content)) return;


        var data = System.Text.Json.JsonSerializer.Deserialize<List<HexData>>(content);
        if (data != null)
        {
            var map = _mapFactory.CreateFromData(data);
            var editViewModel = _navigationService.GetViewModel<EditMapViewModel>();
            if (editViewModel != null)
            {
                editViewModel.Initialize(map);
                await _navigationService.NavigateToViewModelAsync(editViewModel);
            }
        }
    });
}
