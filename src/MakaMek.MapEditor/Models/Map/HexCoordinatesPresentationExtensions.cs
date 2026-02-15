using Sanet.MakaMek.Map.Models;

namespace Sanet.MakaMek.MapEditor.Models.Map;

public static class HexCoordinatesPresentationExtensions
{
    public const double HexWidth = 100;
    public const double HexHeight = 86.6;
    private const double HexHorizontalSpacing = HexWidth * 0.75;

    public static double GetH(this HexCoordinates coordinates)
    {
        return coordinates.Q * HexHorizontalSpacing;
    }

    public static double GetV(this HexCoordinates coordinates)
    {
        return coordinates.R * HexHeight - (coordinates.Q % 2 == 0 ? 0 : HexHeight * 0.5);
    }
}
