using System.ComponentModel;
using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Interactivity;
using Sanet.MakaMek.Map.Data;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.MapEditor.Models;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Presentation.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MakaMek.MapEditor.Views;

public partial class EditMapView : BaseView<EditMapViewModel>
{
    private readonly Dictionary<HexCoordinates, HexRenderData> _hexRenderData = new();
    private HexCoordinates? _selectedHexCoords;

    public EditMapView()
    {
        InitializeComponent();
    }

    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();
        if (ViewModel == null) return;
        ViewModel.CaptureMap = CaptureViewMap;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.HexUpdated += OnHexUpdated;
        SettingsPanelControl.ExportPdfClicked += OnExportPdfClicked;
        RenderMap();
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
        else if (e.PropertyName == nameof(EditMapViewModel.ActiveEditMode))
        {
            ClearSelectionOutline();
        }
    }

    

    private HexRenderData BuildHexRenderData(Hex hex)
    {
        if (ViewModel?.TerrainBitmaskService == null || ViewModel.Map == null)
        {
            var edges = ViewModel?.Map?.GetHexEdges(hex.Coordinates) ?? [];
            return new HexRenderData(hex, edges, null, null);
        }
        return ViewModel.TerrainBitmaskService.CreateHexRenderData(ViewModel.Map, hex.Coordinates);
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

    private async Task<(byte[] PngBytes, int WidthPixels, int HeightPixels)> CaptureViewMap()
    {
        var pngBytes = await MapCanvas.ToPng();
        return (pngBytes, (int)MapCanvas.Width, (int)MapCanvas.Height);
    }

    private void OnExportPdfClicked(object? sender, RoutedEventArgs e)
    {
        if (double.IsNaN(MapCanvas.Width) || double.IsNaN(MapCanvas.Height))
            return;
        ViewModel?.ExportMapAsPdf().SafeFireAndForget();
    }

    private void RefreshHex(Hex hex)
    {
        if (ViewModel?.Map == null) return;
        var coords = hex.Coordinates;

        var changedCoords = new HashSet<HexCoordinates>();

        _hexRenderData[coords] = BuildHexRenderData(hex);
        changedCoords.Add(coords);

        foreach (var neighborCoords in coords.GetAllNeighbours())
        {
            if (!ViewModel.Map.IsOnMap(neighborCoords)) continue;
            var neighborHex = ViewModel.Map.GetHex(neighborCoords);
            if (neighborHex == null) continue;

            if (!_hexRenderData.TryGetValue(neighborCoords, out var existing)) continue;
            var newData = BuildHexRenderData(neighborHex);
            if (existing == newData) continue;

            _hexRenderData[neighborCoords] = newData;
            changedCoords.Add(neighborCoords);
        }

        foreach (var (neighborCoords, neighborEdges) in ViewModel.GetEdgeUpdatesForNeighbors(coords))
        {
            if (!_hexRenderData.TryGetValue(neighborCoords, out var neighborData)) continue;
            _hexRenderData[neighborCoords] = neighborData with { Edges = neighborEdges };
            changedCoords.Add(neighborCoords);
        }

        MapCanvas.UpdateHexEntries(changedCoords.Select(c => _hexRenderData[c]));
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
            ClearSelectionOutline();
            return;
        }

        if (ViewModel.ActiveEditMode == ToolType.Cursor)
        {
            ViewModel.HandleHexSelection(hex);
            ShowSelectionOutline(coords);
            var pointInView = MapCanvas.TranslatePoint(clickedPosition, this);
            if (pointInView.HasValue)
                HexInfoOverlay.Margin = new Thickness(pointInView.Value.X + 10, pointInView.Value.Y + 10, 0, 0);
            return;
        }

        var result = ViewModel.HandleHexSelection(hex) ?? hex;
        RefreshHex(result);
    }

    private void ShowSelectionOutline(HexCoordinates coords)
    {
        _selectedHexCoords = coords;
        MapCanvas.SetBoundaryOutlines(new Dictionary<HexCoordinates, HighlightBoundaryOutline>
        {
            [coords] = new HighlightBoundaryOutline(0b00111111, "#FFFFFF", 3.0)
        });
    }

    private void ClearSelectionOutline()
    {
        if (_selectedHexCoords == null) return;
        _selectedHexCoords = null;
        MapCanvas.SetBoundaryOutlines(null);
    }
}
