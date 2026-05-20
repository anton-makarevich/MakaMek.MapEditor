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
    private readonly Dictionary<HexCoordinates, CanonicalBitmaskResult?> _waterBitmasksByCoords = new();
    
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

    private CanonicalBitmaskResult? ComputeWaterBitmask(Hex hex)
    {
        var bitmaskService = ViewModel?.TerrainBitmaskService;
        if (bitmaskService == null || ViewModel?.Map == null || !hex.HasTerrain(MakaMekTerrains.Water))
            return null;
        return bitmaskService.ComputeCanonicalBitmask(ViewModel.Map, hex.Coordinates, MakaMekTerrains.Water);
    }

    private void RenderMap()
    {
        MapCanvas.Children.Clear();
        _hexControlsByCoords.Clear();
        _waterBitmasksByCoords.Clear();
        if (ViewModel?.Map == null) return;

        double maxX = 0;
        double maxY = 0;

        foreach (var hex in ViewModel.Map.GetHexes())
        {
            var edges = ViewModel.Map.GetHexEdges(hex.Coordinates);
            var waterBitmask = ComputeWaterBitmask(hex);

            var hexControl = new HexControl(hex,
                ViewModel.Logger,
                ViewModel.AssetService,
                ViewModel.LocalizationService,
                edges, null, waterBitmask);
            MapCanvas.Children.Add(hexControl);
            _hexControlsByCoords[hex.Coordinates] = hexControl;
            _waterBitmasksByCoords[hex.Coordinates] = waterBitmask;
            if (hex.Coordinates.H > maxX) maxX = hex.Coordinates.H;
            if (hex.Coordinates.V > maxY) maxY = hex.Coordinates.V;
        }
        
        MapCanvas.Width = maxX + HexCoordinatesPixelExtensions.HexWidth * 0.5;
        MapCanvas.Height = maxY + HexCoordinatesPixelExtensions.HexHeight * 1.5;
    }

    private void RefreshWaterBitmask(HexCoordinates coords)
    {
        if (ViewModel?.Map == null) return;
        var hex = ViewModel.Map.GetHex(coords);
        if (hex == null || !_hexControlsByCoords.ContainsKey(coords)) return;

        var newBitmask = ComputeWaterBitmask(hex);
        
        if (_waterBitmasksByCoords.TryGetValue(coords, out var oldBitmask) 
            && EqualityComparer<CanonicalBitmaskResult?>.Default.Equals(oldBitmask, newBitmask))
        {
            return;
        }

        var edges = ViewModel.Map.GetHexEdges(coords);
        var newControl = new HexControl(hex,
            ViewModel.Logger,
            ViewModel.AssetService,
            ViewModel.LocalizationService,
            edges, null, newBitmask);

        if (_hexControlsByCoords.TryGetValue(coords, out var oldControl))
            MapCanvas.Children.Remove(oldControl);

        MapCanvas.Children.Add(newControl);
        _hexControlsByCoords[coords] = newControl;
        _waterBitmasksByCoords[coords] = newBitmask;
    }

    private void MapCanvas_OnContentClicked(object? sender, Point clickedPosition)
    {
        var selectedHexControl = MapCanvas.Children
            .OfType<HexControl>()
            .FirstOrDefault(h => h.IsPointInside(clickedPosition));

        if (selectedHexControl == null || ViewModel == null) return;

        var newHex = ViewModel.HandleHexSelection(selectedHexControl.Hex);
        var hex = newHex ?? selectedHexControl.Hex;
        var coords = hex.Coordinates;

        // Refresh water bitmask for the changed hex and all on-map neighbors
        // (adding/removing water on one hex affects neighbors' water bitmasks)
        RefreshWaterBitmask(coords);
        foreach (var neighborCoords in coords.GetAllNeighbours())
        {
            if (ViewModel.Map != null && ViewModel.Map.IsOnMap(neighborCoords))
                RefreshWaterBitmask(neighborCoords);
        }

        // Update edges on all on-map neighbors
        foreach (var (neighborCoords, neighborEdges) in ViewModel.GetEdgeUpdatesForNeighbors(coords))
        {
            if (_hexControlsByCoords.TryGetValue(neighborCoords, out var neighborControl))
                neighborControl.UpdateEdges(neighborEdges);
        }
    }
}
