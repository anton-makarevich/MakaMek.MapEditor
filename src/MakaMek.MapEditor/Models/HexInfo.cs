namespace Sanet.MakaMek.MapEditor.Models;

public class HexInfo
{
    public int Level { get; init; }
    public IReadOnlyList<string> TerrainTypes { get; init; } = [];
    public int? WaterDepth { get; init; }
}