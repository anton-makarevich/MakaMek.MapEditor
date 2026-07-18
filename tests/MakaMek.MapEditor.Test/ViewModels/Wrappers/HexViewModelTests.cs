using Sanet.MakaMek.Localization;
using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.ViewModels.Wrappers;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels.Wrappers;

public class HexViewModelTests
{
    private readonly ILocalizationService _localizationService = new FakeLocalizationService();
    [Fact]
    public void Constructor_ShouldReadLevel()
    {
        var hex = new Hex(new HexCoordinates(0, 0), level: 5);

        var sut = new HexViewModel(hex, _localizationService);

        sut.Level.ShouldBe(5);
    }

    [Fact]
    public void Constructor_ShouldReadCoordinates()
    {
        var hex = new Hex(new HexCoordinates(3, 7));

        var sut = new HexViewModel(hex, _localizationService);

        sut.Coordinates.ShouldBe(hex.Coordinates.ToString());
    }

    [Fact]
    public void Constructor_ShouldReadTerrainTypes()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new LightWoodsTerrain());

        var sut = new HexViewModel(hex, _localizationService);

        sut.TerrainTypes.ShouldContain("Light Woods");
    }

    [Fact]
    public void Constructor_ShouldReadMultipleTerrainTypes()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new WaterTerrain(-1));

        var sut = new HexViewModel(hex, _localizationService);

        sut.TerrainTypes.Count.ShouldBe(2);
        sut.TerrainTypes.ShouldContain("Clear");
        sut.TerrainTypes.ShouldContain("Water");
    }

    [Fact]
    public void Constructor_WhenHexHasWater_ShouldReadWaterDepth()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-2));

        var sut = new HexViewModel(hex, _localizationService);

        sut.WaterDepth.ShouldBe(2);
        sut.IsWater.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WhenHexHasNoWater_WaterDepthShouldBeNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());

        var sut = new HexViewModel(hex, _localizationService);

        sut.WaterDepth.ShouldBeNull();
        sut.IsWater.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WhenHexHasShallowWater_WaterDepthShouldBeZero()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(0));

        var sut = new HexViewModel(hex, _localizationService);

        sut.WaterDepth.ShouldBe(0);
        sut.IsWater.ShouldBeTrue();
    }

    [Fact]
    public void UpdateFromHex_ShouldUpdateAllProperties()
    {
        var hex = new Hex(new HexCoordinates(0, 0), level: 1);
        hex.AddTerrain(new ClearTerrain());
        var sut = new HexViewModel(hex, _localizationService);

        var updatedHex = new Hex(new HexCoordinates(5, 9), level: 3);
        updatedHex.AddTerrain(new WaterTerrain(-1));

        sut.UpdateFromHex(updatedHex);

        sut.Level.ShouldBe(3);
        sut.Coordinates.ShouldBe(updatedHex.Coordinates.ToString());
        sut.TerrainTypes.ShouldContain("Water");
        sut.WaterDepth.ShouldBe(1);
        sut.IsWater.ShouldBeTrue();
    }

    [Fact]
    public void UpdateFromHex_ShouldClearWaterWhenRemoved()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-1));
        var sut = new HexViewModel(hex, _localizationService);
        sut.IsWater.ShouldBeTrue();

        var dryHex = new Hex(new HexCoordinates(0, 0));
        dryHex.AddTerrain(new ClearTerrain());

        sut.UpdateFromHex(dryHex);

        sut.WaterDepth.ShouldBeNull();
        sut.IsWater.ShouldBeFalse();
    }

    [Fact]
    public void UpdateFromHex_ShouldRaisePropertyChanged()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        var sut = new HexViewModel(hex, _localizationService);

        var levelChanged = false;
        var coordinatesChanged = false;
        var terrainChanged = false;
        var waterDepthChanged = false;
        var isWaterChanged = false;
        sut.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(HexViewModel.Level): levelChanged = true; break;
                case nameof(HexViewModel.Coordinates): coordinatesChanged = true; break;
                case nameof(HexViewModel.TerrainTypes): terrainChanged = true; break;
                case nameof(HexViewModel.WaterDepth): waterDepthChanged = true; break;
                case nameof(HexViewModel.IsWater): isWaterChanged = true; break;
            }
        };

        var updatedHex = new Hex(new HexCoordinates(1, 2), level: 7);
        updatedHex.AddTerrain(new WaterTerrain(-3));
        sut.UpdateFromHex(updatedHex);

        levelChanged.ShouldBeTrue();
        coordinatesChanged.ShouldBeTrue();
        terrainChanged.ShouldBeTrue();
        waterDepthChanged.ShouldBeTrue();
        isWaterChanged.ShouldBeTrue();
    }

    // --- RoadBridge properties ---
    [Fact]
    public void Constructor_WhenHexHasRoad_ShouldReadHasRoadBridge()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new RoadTerrain());

        var sut = new HexViewModel(hex, _localizationService);

        sut.HasRoadBridge.ShouldBeTrue();
        sut.IsBridge.ShouldBeFalse();
        sut.BridgeHeight.ShouldBeNull();
        sut.ConstructionFactor.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WhenHexHasBridge_ShouldReadBridgeProperties()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(2, 60));

        var sut = new HexViewModel(hex, _localizationService);

        sut.HasRoadBridge.ShouldBeTrue();
        sut.IsBridge.ShouldBeTrue();
        sut.BridgeHeight.ShouldBe(2);
        sut.ConstructionFactor.ShouldBe(60);
    }

    [Fact]
    public void Constructor_WhenHexHasNoRoadOrBridge_ShouldHaveDefaultValues()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());

        var sut = new HexViewModel(hex, _localizationService);

        sut.HasRoadBridge.ShouldBeFalse();
        sut.IsBridge.ShouldBeFalse();
        sut.BridgeHeight.ShouldBeNull();
        sut.ConstructionFactor.ShouldBeNull();
    }

    [Fact]
    public void UpdateFromHex_ShouldUpdateRoadBridgeProperties()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        var sut = new HexViewModel(hex, _localizationService);
        sut.HasRoadBridge.ShouldBeFalse();

        var bridgeHex = new Hex(new HexCoordinates(0, 0));
        bridgeHex.AddTerrain(new WaterTerrain(0));
        bridgeHex.AddTerrain(new BridgeTerrain(3, 120));

        sut.UpdateFromHex(bridgeHex);

        sut.HasRoadBridge.ShouldBeTrue();
        sut.IsBridge.ShouldBeTrue();
        sut.BridgeHeight.ShouldBe(3);
        sut.ConstructionFactor.ShouldBe(120);
    }

    [Fact]
    public void UpdateFromHex_ShouldClearRoadBridgeWhenRemoved()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(0));
        hex.AddTerrain(new BridgeTerrain(1, 60));
        var sut = new HexViewModel(hex, _localizationService);
        sut.HasRoadBridge.ShouldBeTrue();

        var clearHex = new Hex(new HexCoordinates(0, 0));
        clearHex.AddTerrain(new ClearTerrain());

        sut.UpdateFromHex(clearHex);

        sut.HasRoadBridge.ShouldBeFalse();
        sut.IsBridge.ShouldBeFalse();
        sut.BridgeHeight.ShouldBeNull();
        sut.ConstructionFactor.ShouldBeNull();
    }

    [Fact]
    public void UpdateFromHex_ShouldRaisePropertyChangedForRoadBridge()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        var sut = new HexViewModel(hex, _localizationService);

        var hasRoadBridgeChanged = false;
        var isBridgeChanged = false;
        var bridgeHeightChanged = false;
        var constructionFactorChanged = false;
        sut.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(HexViewModel.HasRoadBridge): hasRoadBridgeChanged = true; break;
                case nameof(HexViewModel.IsBridge): isBridgeChanged = true; break;
                case nameof(HexViewModel.BridgeHeight): bridgeHeightChanged = true; break;
                case nameof(HexViewModel.ConstructionFactor): constructionFactorChanged = true; break;
            }
        };

        var bridgeHex = new Hex(new HexCoordinates(0, 0));
        bridgeHex.AddTerrain(new WaterTerrain(0));
        bridgeHex.AddTerrain(new BridgeTerrain(1, 60));
        sut.UpdateFromHex(bridgeHex);

        hasRoadBridgeChanged.ShouldBeTrue();
        isBridgeChanged.ShouldBeTrue();
        bridgeHeightChanged.ShouldBeTrue();
        constructionFactorChanged.ShouldBeTrue();
    }
}
