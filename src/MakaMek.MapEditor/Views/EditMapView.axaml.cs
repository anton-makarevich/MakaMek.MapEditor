using System.ComponentModel;
using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Sanet.MakaMek.Avalonia.Controls;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.Models;
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
            ViewModel.HexUpdated += OnHexUpdated;
            RenderMap();

            // On mobile (SingleView e.g. Android/iOS/WASM), hide settings panel by default
            if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime)
            {
                ViewModel.IsSettingsPanelVisible = false;
            }
        }
    }

    private void OnHexUpdated(Hex hex)
    {
        RefreshHex(hex);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditMapViewModel.Map))
        {
            RenderMap();
        }
        else if (e.PropertyName == nameof(EditMapViewModel.HexConfiguration))
        {
            if (ViewModel?.HexConfiguration != null)
            {
                var config = ViewModel.HexConfiguration.ToConfiguration();
                foreach (var hexControl in MapCanvas.Children.OfType<HexControl>())
                {
                    hexControl.UpdateRenderConfiguration(config);
                }
            }
        }
    }

    private CanonicalBitmaskResult? ComputeWaterBitmask(Hex hex)
    {
        var bitmaskService = ViewModel?.TerrainBitmaskService;
        if (bitmaskService == null || ViewModel?.Map == null || !hex.HasTerrain(MakaMekTerrains.Water))
            return null;
        return bitmaskService.ComputeCanonicalBitmask(ViewModel.Map, hex.Coordinates, MakaMekTerrains.Water);
    }

    private void ReplaceHexControl(HexCoordinates coords, CanonicalBitmaskResult? waterBitmask)
    {
        if (ViewModel?.Map == null) return;
        var hex = ViewModel.Map.GetHex(coords);
        if (hex == null) return;

        var edges = ViewModel.Map.GetHexEdges(coords);
        var newControl = new HexControl(hex,
            ViewModel.Logger,
            ViewModel.AssetService,
            ViewModel.LocalizationService,
            edges, ViewModel.HexConfiguration.ToConfiguration(), waterBitmask, ViewModel.Scheduler);

        if (_hexControlsByCoords.TryGetValue(coords, out var oldControl))
            MapCanvas.Children.Remove(oldControl);

        MapCanvas.Children.Add(newControl);
        _hexControlsByCoords[coords] = newControl;
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
            ReplaceHexControl(hex.Coordinates, ComputeWaterBitmask(hex));
            if (hex.Coordinates.H > maxX) maxX = hex.Coordinates.H;
            if (hex.Coordinates.V > maxY) maxY = hex.Coordinates.V;
        }
        
        MapCanvas.Width = maxX + HexCoordinatesPixelExtensions.HexWidth * 1.5;
        MapCanvas.Height = maxY + HexCoordinatesPixelExtensions.HexHeight * 1.5;
    }

    private void RefreshWaterBitmask(HexCoordinates coords)
    {
        var hex = ViewModel?.Map?.GetHex(coords);
        if (hex == null || !_hexControlsByCoords.TryGetValue(coords, out var oldControl)) return;

        var newBitmask = ComputeWaterBitmask(hex);
        if (EqualityComparer<CanonicalBitmaskResult?>.Default.Equals(oldControl.WaterBitmask, newBitmask))
            return;

        ReplaceHexControl(coords, newBitmask);
    }

    private void OnExportPdfClicked(object? sender, RoutedEventArgs e)
    {
        if (double.IsNaN(MapCanvas.Width) || double.IsNaN(MapCanvas.Height))
            return;
        var width = (int)MapCanvas.Width;
        var height = (int)MapCanvas.Height;
        var pngBytes = MapCanvas.ToPng();
        ViewModel?.ExportMapAsPdf(pngBytes, width, height).SafeFireAndForget();
    }

    private void RefreshHex(Hex hex)
    {
        if (ViewModel?.Map == null) return;
        var coords = hex.Coordinates;

        ReplaceHexControl(coords, ComputeWaterBitmask(hex));

        foreach (var neighborCoords in coords.GetAllNeighbours())
        {
            if (ViewModel.Map.IsOnMap(neighborCoords))
                RefreshWaterBitmask(neighborCoords);
        }

        foreach (var (neighborCoords, neighborEdges) in ViewModel.GetEdgeUpdatesForNeighbors(coords))
        {
            if (_hexControlsByCoords.TryGetValue(neighborCoords, out var neighborControl))
                neighborControl.UpdateEdges(neighborEdges);
        }
    }

    private void MapCanvas_OnContentClicked(object? sender, Point clickedPosition)
    {
        if (ViewModel == null) return;

        var selectedHexControl = MapCanvas.Children
            .OfType<HexControl>()
            .FirstOrDefault(h => h.IsPointInside(clickedPosition));

        if (selectedHexControl == null)
        {
            if (ViewModel.ActiveEditMode == ToolType.Cursor)
                ViewModel.IsHexInfoVisible = false;
            return;
        }

        if (ViewModel.ActiveEditMode == ToolType.Cursor)
        {
            ViewModel.HandleHexSelection(selectedHexControl.Hex);
            var pointInView = MapCanvas.TranslatePoint(clickedPosition, this);
            if (pointInView.HasValue)
                HexInfoOverlay.Margin = new Thickness(pointInView.Value.X + 10, pointInView.Value.Y + 10, 0, 0);
            return;
        }

        var hex = ViewModel.HandleHexSelection(selectedHexControl.Hex) ?? selectedHexControl.Hex;
        RefreshHex(hex);
    }
}
