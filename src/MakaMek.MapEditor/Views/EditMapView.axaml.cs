using System.ComponentModel;
using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Media.Imaging;
using Sanet.MakaMek.MapEditor.Controls;
using Sanet.MakaMek.MapEditor.Models.Map;
using Sanet.MakaMek.MapEditor.ViewModels;
using Sanet.MakaMek.Services;
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

        if (ViewModel.ImageService is not IImageService<Bitmap> imageService)
        {
            throw new Exception("ImageService is not initialized");
        }

        double maxX = 0;
        double maxY = 0;

        foreach (var hex in ViewModel.Map.GetHexes())
        {
            var hexControl = new HexControl(hex, imageService);
            MapCanvas.Children.Add(hexControl);
            if (hex.Coordinates.GetH() > maxX) maxX = hex.Coordinates.GetH();
            if (hex.Coordinates.GetV() > maxY) maxY = hex.Coordinates.GetV();
        }
        
        MapCanvas.Width = maxX+ HexCoordinatesPresentationExtensions.HexWidth*0.5;
        MapCanvas.Height = maxY + HexCoordinatesPresentationExtensions.HexHeight*1.5;
    }

    private void MapCanvas_OnContentClicked(object? sender, Point clickedPosition)
    {
        var selectedHex = MapCanvas.Children
            .OfType<HexControl>()
            .FirstOrDefault(h => h.IsPointInside(clickedPosition));

        if (selectedHex != null && ViewModel!=null)
        {
            ViewModel?.HandleHexSelection(selectedHex.Hex);
            selectedHex.Render().SafeFireAndForget();
        }
    }
}
