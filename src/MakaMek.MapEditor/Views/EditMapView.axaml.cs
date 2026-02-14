using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Sanet.MVVM.Views.Avalonia;
using MakaMek.MapEditor.Controls;
using MakaMek.MapEditor.Services;
using MakaMek.MapEditor.ViewModels;
// Check namespace if needed
using MakaMek.MapEditor.Models.Map; // For Extensions

namespace MakaMek.MapEditor.Views;

public partial class EditMapView : BaseView<EditMapViewModel>
{
    
    public EditMapView()
    {
        InitializeComponent();
        
        // Resolve ImageService from App resources or DI
        // Ideally we should inject it but in code behind passing it via constructor is hard with XAML.
        // We can resolve it from App.Services if available or use locator.
        // Sanet.MVVM might provide way.
        // For now, let's try to get it from ViewModel or ServiceProvider (if accessible).
        // Since we are creating HexControls, we need it.
        
        // We can cast Application.Current to App?
        
        // Let's defer loading map until ViewModel is set.
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

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        // Get ImageService
        // Assuming we can get it from ServiceProvider stored in App or similar.
        // But App.ServiceProvider is static? No.
        // We can resolve via ServiceCollection resource key if initialized?
        // Or inject via property injection?
        
        // HACK: Helper to get service
        var imageService = GetImageService();
        if (imageService == null) return; // Should not happen

        double maxX = 0;
        double maxY = 0;

        foreach (var hex in ViewModel.Map.GetHexes())
        {
            var hexControl = new HexControl(hex, imageService);
            // Click handler
             hexControl.PointerPressed += HexControl_PointerPressed;
            
            MapCanvas.Children.Add(hexControl);
            
            var x = hex.Coordinates.GetH() + HexCoordinatesPresentationExtensions.HexWidth;
            var y = hex.Coordinates.GetV() + HexCoordinatesPresentationExtensions.HexHeight;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }
        
        MapCanvas.Width = maxX;
        MapCanvas.Height = maxY;
    }

    private void HexControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is HexControl hexControl && ViewModel != null)
        {
            ViewModel.HandleHexSelection(hexControl.Hex);
            // Re-render this hex or update it?
            // HexControl handles its own update if it observes model?
            // "Create an observable that polls" code in HexControl was commented out.
            // We should force update of image.
            // But UpdateTerrainImage is private in HexControl.
            // We might need to recreate the HexControl or expose Update method.
            
            // For map editor, recreating one control is fine or simple update.
            // Let's assume we refresh map or just that control.
            // But changing terrain in Model might not trigger View update automatically without Observable.
            
            RenderMap(); // Brute force refresh for now (Optimization later)
        }
    }

    private IImageService<Bitmap>? GetImageService()
    {
        if (Application.Current is App app)
        {
             // Oops App.ServiceProvider is internal/private?
             // Step 96: `public IServiceProvider? ServiceProvider { get; private set; }`
             // Yes it is public!
             return app.ServiceProvider?.GetService<IImageService<Bitmap>>();
        }
        return null; // Fallback
    }
}
