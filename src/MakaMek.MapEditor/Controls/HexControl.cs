using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Sanet.MakaMek.Map.Models;
using MakaMek.MapEditor.Models.Map;
using MakaMek.MapEditor.Services;

namespace MakaMek.MapEditor.Controls;

public class HexControl : Panel
{
    private readonly Polygon _hexPolygon;
    private readonly Image _terrainImage;
    private readonly IImageService<Bitmap> _imageService;
    private readonly Hex _hex;

    private static readonly IBrush DefaultStroke = Brushes.Gray;
    private static readonly IBrush HighlightStroke = new SolidColorBrush(Color.Parse("#00BFFF")); // Light blue
    private static readonly IBrush HighlightFill = new SolidColorBrush(Color.Parse("#3300BFFF")); // Semi-transparent light blue
    private static readonly IBrush TransparentFill = Brushes.Transparent;

    private const double DefaultStrokeThickness = 1;
    private const double HighlightStrokeThickness = 3;

    private static Points GetHexPoints()
    {
        const double width = HexCoordinatesPresentationExtensions.HexWidth;
        const double height = HexCoordinatesPresentationExtensions.HexHeight;

        return new Points([
            new Point(0, height * 0.5),           // Left
            new Point(width * 0.25, height),      // Bottom Left
            new Point(width * 0.75, height),      // Bottom Right
            new Point(width, height * 0.5),       // Right
            new Point(width * 0.75, 0),           // Top Right
            new Point(width * 0.25, 0)            // Top Left
        ]);
    }

    public HexControl(Hex hex, IImageService<Bitmap> imageService)
    {
        _hex = hex;
        _imageService = imageService;
        Width = HexCoordinatesPresentationExtensions.HexWidth;
        Height = HexCoordinatesPresentationExtensions.HexHeight;
        
        // Terrain image (bottom layer)
        _terrainImage = new Image
        {
            Width = Width,
            Height = Height,
            Stretch = Stretch.Fill
        };

        // Hex polygon (top layer)
        _hexPolygon = new Polygon
        {
            Points = GetHexPoints(),
            Fill = TransparentFill,
            Stroke = DefaultStroke,
            StrokeThickness = DefaultStrokeThickness
        };
        
        var label = new Label
        {
            Content = hex.Coordinates.ToString(),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Black,
            FontSize = 10
        };
        
        Children.Add(_terrainImage);
        Children.Add(_hexPolygon);
        Children.Add(label);
        
        // Create an observable that polls the unit's position
        // We removed Observable interval for now to simplify, or keep it if System.Reactive is available
        // Assuming System.Reactive is available
        /*
        Observable
            .Interval(TimeSpan.FromMilliseconds(16)) // ~60fps
            .Select(_ => _hex.IsHighlighted) // Hex.IsHighlighted might not exist in Sanet.MakaMek.Map Hex class? Check.
            // If Hex class doesn't have IsHighlighted, we might need a wrapper or ignore highlighting for now.
            // .DistinctUntilChanged()
            // .ObserveOn(SynchronizationContext.Current)
            // .Subscribe(_ => Highlight(_hex.IsHighlighted ? HexHighlightType.Selected : HexHighlightType.None));
        */
        
        // Set position
        SetValue(Canvas.LeftProperty, hex.Coordinates.GetH());
        SetValue(Canvas.TopProperty, hex.Coordinates.GetV());

        UpdateTerrainImage().SafeFireAndForget();
    }
    public Hex Hex => _hex;
    
    public void Highlight(bool isSelected)
    {
        if (isSelected)
        {
            _hexPolygon.Stroke = HighlightStroke;
            _hexPolygon.StrokeThickness = HighlightStrokeThickness;
            _hexPolygon.Fill = HighlightFill;
        }
        else
        {
            _hexPolygon.Stroke = DefaultStroke;
            _hexPolygon.StrokeThickness = DefaultStrokeThickness;
            _hexPolygon.Fill = TransparentFill;
        }
    }

    private async Task UpdateTerrainImage()
    {
        var terrain = _hex.GetTerrains().FirstOrDefault();
        if (terrain == null) return;

        var image = await _imageService.GetImage("terrain", terrain.Id.ToString().ToLower()); 
        if (image != null)
        {
            _terrainImage.Source = image;
        }
    }
    
    public bool IsPointInside(Point point)
    {
        // Simple bounding box check might be enough for clicks if precise hit test isn't critical,
        // but polygon hit test is better. Bounds check is easiest first step.
        return Bounds.Contains(point);
    }
}
