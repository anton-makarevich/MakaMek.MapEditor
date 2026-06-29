using Sanet.MakaMek.Map.Models;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels.Wrappers;

public class HexViewModel : BindableBase
{
    public HexViewModel(Hex hex)
    {
        UpdateFromHex(hex);
    }

    public int Level { get; private set; }
    public string Coordinates { get; private set; } = string.Empty;
    public IReadOnlyList<string> TerrainTypes { get; private set; } = [];
    public int? WaterDepth { get; private set; }
    public bool IsWater => WaterDepth.HasValue;

    public void UpdateFromHex(Hex hex)
    {
        Level = hex.Level;
        Coordinates = hex.Coordinates.ToString();
        TerrainTypes = hex.GetTerrains().Select(t => t.Id.ToString()).ToList();
        WaterDepth = hex.GetWaterDepth();
        NotifyPropertyChanged(nameof(Level));
        NotifyPropertyChanged(nameof(Coordinates));
        NotifyPropertyChanged(nameof(TerrainTypes));
        NotifyPropertyChanged(nameof(WaterDepth));
        NotifyPropertyChanged(nameof(IsWater));
    }
}