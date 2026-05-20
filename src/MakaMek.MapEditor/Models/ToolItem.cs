using Sanet.MakaMek.Map.Models.Terrains;

namespace Sanet.MakaMek.MapEditor.Models;

public class ToolItem(string displayName, ToolType type, Terrain? terrain = null)
{
    public string DisplayName { get; } = displayName;
    public ToolType Type { get; } = type;
    public Terrain? Terrain { get; } = terrain;
}
