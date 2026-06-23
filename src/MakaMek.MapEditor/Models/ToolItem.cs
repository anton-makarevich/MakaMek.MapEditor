using Sanet.MakaMek.Map.Models.Terrains;

namespace Sanet.MakaMek.MapEditor.Models;

public class ToolItem(string displayName, ToolType type, Terrain? terrain = null, string? imagePath = null)
{
    public string DisplayName { get; } = displayName;
    public ToolType Type { get; } = type;
    public Terrain? Terrain { get; } = terrain;
    public string? ImagePath { get; } = imagePath;
    public bool HasImagePath => !string.IsNullOrWhiteSpace(ImagePath);
}
