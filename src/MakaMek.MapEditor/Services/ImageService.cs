using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MakaMek.MapEditor.Services;

public class ImageService : IImageService<Bitmap>
{
    public Task<object?> GetImage(string assetType, string assetName)
    {
        return Task.FromResult<object?>(GetImageInternal(assetType, assetName));
    }

    Task<Bitmap?> IImageService<Bitmap>.GetImage(string assetType, string assetName)
    {
        return Task.FromResult(GetImageInternal(assetType, assetName));
    }

    private Bitmap? GetImageInternal(string assetType, string assetName)
    {
        // Placeholder: Generate a bitmap with a color based on name
        // Ideally load from assets
         try
        {
            // Try load from assets if available, else generate color
            // For now, simple color generation
            var color = GetColorForTerrain(assetName);
            var pixelSize = new PixelSize(100, 100);
            var bitmap = new WriteableBitmap(pixelSize, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
            
            using (var frame = bitmap.Lock())
            {
                var pixels = new uint[100 * 100];
                var colorUint = (uint)(color.B | (color.G << 8) | (color.R << 16) | (color.A << 24));
                for(int i=0; i<pixels.Length; i++) pixels[i] = colorUint;
                
                System.Runtime.InteropServices.Marshal.Copy((int[])(object)pixels, 0, frame.Address, pixels.Length);
            }
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Color GetColorForTerrain(string name)
    {
        if (name.Contains("clear")) return Colors.LightGreen;
        if (name.Contains("heavy")) return Colors.DarkGreen; // heavy woods
        if (name.Contains("light")) return Colors.ForestGreen; // light woods
        if (name.Contains("water")) return Colors.Blue;
        if (name.Contains("rough")) return Colors.Brown;
        if (name.Contains("paved")) return Colors.Gray;
        if (name.Contains("road")) return Colors.DarkGray;
        
        return Colors.White;
    }
}
