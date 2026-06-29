using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.MapEditor.ViewModels.Wrappers;

public class HexViewModel : BindableBase
{
    public HexViewModel(Hex hex)
    {
        UpdateFromHex(hex);
    }

    public int Level { get; private set; }
    public IReadOnlyList<string> TerrainTypes { get; private set; } = [];
    public int? WaterDepth { get; private set; }

    public void UpdateFromHex(Hex hex)
    {
        Level = hex.Level;
        TerrainTypes = hex.GetTerrains().Select(t => t.Id.ToString()).ToList();
        WaterDepth = (hex.GetTerrain(MakaMekTerrains.Water) as WaterTerrain)?.Height;
        NotifyPropertyChanged(nameof(Level));
        NotifyPropertyChanged(nameof(TerrainTypes));
        NotifyPropertyChanged(nameof(WaterDepth));
    }
}