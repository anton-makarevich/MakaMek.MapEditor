namespace Sanet.MakaMek.MapEditor.Services;

public interface IImageService
{
    Task<object?> GetImage(string assetType, string assetName);
}

public interface IImageService<T>:IImageService
{
    new Task<T?> GetImage(string assetType, string assetName);
}
