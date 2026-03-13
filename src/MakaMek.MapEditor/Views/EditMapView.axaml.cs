using System.ComponentModel;
using AsyncAwaitBestPractices;
using Avalonia;
using Microsoft.Extensions.Logging;
using Sanet.MakaMek.Avalonia.Controls;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MakaMek.MapEditor.Views;

public partial class EditMapView : BaseView<EditMapViewModel>
{
    
    public EditMapView()
    {
        InitializeComponent();
    }

    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            RenderMap();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditMapViewModel.Map))
        {
            RenderMap();
        }
    }

    private void RenderMap()
    {
        MapCanvas.Children.Clear();
        if (ViewModel?.Map == null) return;

        double maxX = 0;
        double maxY = 0;

        foreach (var hex in ViewModel.Map.GetHexes())
        {
            var hexControl = new HexControl(hex, ViewModel.Logger, ViewModel.AssetService);
            MapCanvas.Children.Add(hexControl);
            if (hex.Coordinates.H > maxX) maxX = hex.Coordinates.H;
            if (hex.Coordinates.V > maxY) maxY = hex.Coordinates.V;
        }
        
        MapCanvas.Width = maxX+ HexCoordinatesPixelExtensions.HexWidth*0.5;
        MapCanvas.Height = maxY + HexCoordinatesPixelExtensions.HexHeight*1.5;
    }

    private void MapCanvas_OnContentClicked(object? sender, Point clickedPosition)
    {
        var selectedHex = MapCanvas.Children
            .OfType<HexControl>()
            .FirstOrDefault(h => h.IsPointInside(clickedPosition));

        if (selectedHex == null || ViewModel == null) return;
        ViewModel.HandleHexSelection(selectedHex.Hex);
        selectedHex.Render().SafeFireAndForget(
            ex => ViewModel.Logger.LogError(ex, "Failed to render hex"));
    }
}
