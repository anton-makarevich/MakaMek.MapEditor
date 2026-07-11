using System.ComponentModel;
using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Interactivity;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.Models;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MakaMek.MapEditor.Views;

public partial class EditMapView : BaseView<EditMapViewModel>
{
    private readonly Dictionary<HexCoordinates, HexRenderData> _hexRenderData = new();

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
            SettingsPanelControl.ExportPdfClicked += OnExportPdfClicked;
            RenderMap();
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
                MapCanvas.UpdateHexConfiguration(config);
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

    private HexRenderData BuildHexRenderData(Hex hex)
    {
        var edges = ViewModel?.Map?.GetHexEdges(hex.Coordinates) ?? [];
        var waterBitmask = ComputeWaterBitmask(hex);
        return new HexRenderData(hex, edges, waterBitmask, null);
    }

    private void PushHexData()
    {
        if (ViewModel?.Map == null 
            || ViewModel.Scheduler == null) return;

        MapCanvas.Children.Clear();
        var config = ViewModel.HexConfiguration.ToConfiguration();
        MapCanvas.SetHexData(
            _hexRenderData.Values,
            config,
            ViewModel.Logger,
            ViewModel.AssetService,
            ViewModel.LocalizationService,
            ViewModel.Scheduler);
    }

    private void RenderMap()
    {
        MapCanvas.Children.Clear();
        _hexRenderData.Clear();
        if (ViewModel?.Map == null) return;

        double maxX = 0;
        double maxY = 0;

        foreach (var hex in ViewModel.Map.GetHexes())
        {
            _hexRenderData[hex.Coordinates] = BuildHexRenderData(hex);
            if (hex.Coordinates.H > maxX) maxX = hex.Coordinates.H;
            if (hex.Coordinates.V > maxY) maxY = hex.Coordinates.V;
        }

        MapCanvas.Width = maxX + HexCoordinatesPixelExtensions.HexWidth * 1.5;
        MapCanvas.Height = maxY + HexCoordinatesPixelExtensions.HexHeight * 1.5;

        PushHexData();
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

        _hexRenderData[coords] = BuildHexRenderData(hex);

        foreach (var neighborCoords in coords.GetAllNeighbours())
        {
            if (!ViewModel.Map.IsOnMap(neighborCoords)) continue;
            var neighborHex = ViewModel.Map.GetHex(neighborCoords);
            if (neighborHex == null) continue;

            var newBitmask = ComputeWaterBitmask(neighborHex);
            var existing = _hexRenderData[neighborCoords];
            if (EqualityComparer<CanonicalBitmaskResult?>.Default.Equals(existing.WaterBitmask, newBitmask))
                continue;

            _hexRenderData[neighborCoords] = BuildHexRenderData(neighborHex);
        }

        foreach (var (neighborCoords, neighborEdges) in ViewModel.GetEdgeUpdatesForNeighbors(coords))
        {
            if (!_hexRenderData.TryGetValue(neighborCoords, out var neighborData)) continue;
            _hexRenderData[neighborCoords] = neighborData with { Edges = neighborEdges };
        }

        PushHexData();
    }

    private void MapCanvas_OnContentClicked(object? sender, Point clickedPosition)
    {
        if (ViewModel?.Map == null) return;

        var coords = HexCoordinatesPixelExtensions.FromPixel(clickedPosition.X, clickedPosition.Y);
        var hex = ViewModel.Map.GetHex(coords);

        if (hex == null)
        {
            if (ViewModel.ActiveEditMode == ToolType.Cursor)
                ViewModel.IsHexInfoVisible = false;
            return;
        }

        if (ViewModel.ActiveEditMode == ToolType.Cursor)
        {
            ViewModel.HandleHexSelection(hex);
            var pointInView = MapCanvas.TranslatePoint(clickedPosition, this);
            if (pointInView.HasValue)
                HexInfoOverlay.Margin = new Thickness(pointInView.Value.X + 10, pointInView.Value.Y + 10, 0, 0);
            return;
        }

        var result = ViewModel.HandleHexSelection(hex) ?? hex;
        RefreshHex(result);
    }
}
