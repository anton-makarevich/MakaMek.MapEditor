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
    private readonly Dictionary<HexCoordinates, HexControl> _hexControlsByCoords = new();
    
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
        _hexControlsByCoords.Clear();
        if (ViewModel?.Map == null) return;

        double maxX = 0;
        double maxY = 0;

        foreach (var hex in ViewModel.Map.GetHexes())
        {
            var edges = ViewModel.Map.GetHexEdges(hex.Coordinates);
            var hexControl = new HexControl(hex, ViewModel.Logger, ViewModel.AssetService, edges);
            MapCanvas.Children.Add(hexControl);
            _hexControlsByCoords[hex.Coordinates] = hexControl;
            if (hex.Coordinates.H > maxX) maxX = hex.Coordinates.H;
            if (hex.Coordinates.V > maxY) maxY = hex.Coordinates.V;
        }
        
        MapCanvas.Width = maxX + HexCoordinatesPixelExtensions.HexWidth * 0.5;
        MapCanvas.Height = maxY + HexCoordinatesPixelExtensions.HexHeight * 1.5;
    }

    private void MapCanvas_OnContentClicked(object? sender, Point clickedPosition)
    {
        var selectedHexControl = MapCanvas.Children
            .OfType<HexControl>()
            .FirstOrDefault(h => h.IsPointInside(clickedPosition));

        if (selectedHexControl == null || ViewModel == null) return;

        var newHex = ViewModel.HandleHexSelection(selectedHexControl.Hex);

        if (newHex != null)
        {
            // Level mode: replace the HexControl (Hex is immutable, can't swap _hex)
            var edges = ViewModel.Map?.GetHexEdges(newHex.Coordinates);
            var newHexControl = new HexControl(newHex, ViewModel.Logger, ViewModel.AssetService, edges);
            MapCanvas.Children.Remove(selectedHexControl);
            MapCanvas.Children.Add(newHexControl);

            // Update edges on all on-map neighbors via the ViewModel
            foreach (var (coords, neighborEdges) in ViewModel.GetEdgeUpdatesForNeighbors(newHex.Coordinates))
            {
                if (_hexControlsByCoords.TryGetValue(coords, out var neighborControl))
                {
                    neighborControl.UpdateEdges(neighborEdges);
                }
            }
        }
        else
        {
            // Terrain mode: re-render the single hex
            selectedHexControl.Render().SafeFireAndForget(
                ex => ViewModel.Logger.LogError(ex, "Failed to render hex"));
        }
    }
}
