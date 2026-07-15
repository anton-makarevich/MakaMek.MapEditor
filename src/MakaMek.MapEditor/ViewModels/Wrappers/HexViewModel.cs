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
    public string Coordinates { get; private set; } = string.Empty;
    public IReadOnlyList<string> TerrainTypes { get; private set; } = [];
    public int? WaterDepth { get; private set; }
    public bool IsWater => WaterDepth.HasValue;
    public bool HasRoadBridge { get; private set; }
    public bool IsBridge { get; private set; }
    public int? BridgeHeight { get; private set; }
    public int? ConstructionFactor { get; private set; }

    public void UpdateFromHex(Hex hex)
    {
        Level = hex.Level;
        Coordinates = hex.Coordinates.ToString();
        TerrainTypes = hex.GetTerrains().Select(t => t.Id.ToString()).ToList();
        WaterDepth = hex.GetWaterDepth();

        var roadTerrain = hex.GetTerrain(MakaMekTerrains.Road);
        var bridgeTerrain = hex.GetTerrain(MakaMekTerrains.Bridge);
        HasRoadBridge = roadTerrain != null || bridgeTerrain != null;
        IsBridge = bridgeTerrain != null;
        BridgeHeight = (bridgeTerrain as BridgeTerrain)?.Height;
        ConstructionFactor = (bridgeTerrain as BridgeTerrain)?.ConstructionFactor;

        NotifyPropertyChanged(nameof(Level));
        NotifyPropertyChanged(nameof(Coordinates));
        NotifyPropertyChanged(nameof(TerrainTypes));
        NotifyPropertyChanged(nameof(WaterDepth));
        NotifyPropertyChanged(nameof(IsWater));
        NotifyPropertyChanged(nameof(HasRoadBridge));
        NotifyPropertyChanged(nameof(IsBridge));
        NotifyPropertyChanged(nameof(BridgeHeight));
        NotifyPropertyChanged(nameof(ConstructionFactor));
    }
}