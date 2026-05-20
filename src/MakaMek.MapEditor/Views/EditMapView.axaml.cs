using System.ComponentModel;
using Avalonia;
using Sanet.MakaMek.Avalonia.Controls;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
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

        var bitmaskService = ViewModel.TerrainBitmaskService;

        foreach (var hex in ViewModel.Map.GetHexes())
        {
            var edges = ViewModel.Map.GetHexEdges(hex.Coordinates);

            CanonicalBitmaskResult? waterBitmask = null;
            if (bitmaskService != null && hex.HasTerrain(MakaMekTerrains.Water))
            {
                waterBitmask = bitmaskService.ComputeCanonicalBitmask(
                    ViewModel.Map, hex.Coordinates, MakaMekTerrains.Water);
            }

            var hexControl = new HexControl(hex,
                ViewModel.Logger,
                ViewModel.AssetService,
                ViewModel.LocalizationService,
                edges, null, waterBitmask);
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
        var hex = newHex ?? selectedHexControl.Hex;

        // Always replace the HexControl so the water bitmask is computed fresh
        CanonicalBitmaskResult? waterBitmask = null;
        var bitmaskService = ViewModel.TerrainBitmaskService;
        if (ViewModel.Map != null && hex.HasTerrain(MakaMekTerrains.Water))
        {
            waterBitmask = bitmaskService.ComputeCanonicalBitmask(
                ViewModel.Map, hex.Coordinates, MakaMekTerrains.Water);
        }

        var edges = ViewModel.Map?.GetHexEdges(hex.Coordinates);
        var newHexControl = new HexControl(hex,
            ViewModel.Logger,
            ViewModel.AssetService,
            ViewModel.LocalizationService,
            edges, null, waterBitmask);
        MapCanvas.Children.Remove(selectedHexControl);
        MapCanvas.Children.Add(newHexControl);
        _hexControlsByCoords[hex.Coordinates] = newHexControl;

        // Update edges on all on-map neighbors via the ViewModel
        foreach (var (coords, neighborEdges) in ViewModel.GetEdgeUpdatesForNeighbors(hex.Coordinates))
        {
            if (_hexControlsByCoords.TryGetValue(coords, out var neighborControl))
            {
                neighborControl.UpdateEdges(neighborEdges);
            }
        }
    }
}
