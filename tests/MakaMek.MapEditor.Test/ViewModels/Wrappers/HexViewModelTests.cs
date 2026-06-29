using Sanet.MakaMek.Map.Models;
using Sanet.MakaMek.Map.Models.Terrains;
using Sanet.MakaMek.MapEditor.ViewModels.Wrappers;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.ViewModels.Wrappers;

public class HexViewModelTests
{
    [Fact]
    public void Constructor_ShouldReadLevel()
    {
        var hex = new Hex(new HexCoordinates(0, 0), level: 5);

        var sut = new HexViewModel(hex);

        sut.Level.ShouldBe(5);
    }

    [Fact]
    public void Constructor_ShouldReadCoordinates()
    {
        var hex = new Hex(new HexCoordinates(3, 7));

        var sut = new HexViewModel(hex);

        sut.Coordinates.ShouldNotBeNullOrEmpty();
        sut.Coordinates.ShouldContain("3");
        sut.Coordinates.ShouldContain("7");
    }

    [Fact]
    public void Constructor_ShouldReadTerrainTypes()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new LightWoodsTerrain());

        var sut = new HexViewModel(hex);

        sut.TerrainTypes.ShouldContain("LightWoods");
    }

    [Fact]
    public void Constructor_ShouldReadMultipleTerrainTypes()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());
        hex.AddTerrain(new WaterTerrain(-1));

        var sut = new HexViewModel(hex);

        sut.TerrainTypes.Count.ShouldBe(2);
        sut.TerrainTypes.ShouldContain("Clear");
        sut.TerrainTypes.ShouldContain("Water");
    }

    [Fact]
    public void Constructor_WhenHexHasWater_ShouldReadWaterDepth()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-2));

        var sut = new HexViewModel(hex);

        sut.WaterDepth.ShouldBe(2);
        sut.IsWater.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WhenHexHasNoWater_WaterDepthShouldBeNull()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new ClearTerrain());

        var sut = new HexViewModel(hex);

        sut.WaterDepth.ShouldBeNull();
        sut.IsWater.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WhenHexHasShallowWater_WaterDepthShouldBeZero()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(0));

        var sut = new HexViewModel(hex);

        sut.WaterDepth.ShouldBe(0);
        sut.IsWater.ShouldBeTrue();
    }

    [Fact]
    public void UpdateFromHex_ShouldUpdateAllProperties()
    {
        var hex = new Hex(new HexCoordinates(0, 0), level: 1);
        hex.AddTerrain(new ClearTerrain());
        var sut = new HexViewModel(hex);

        var updatedHex = new Hex(new HexCoordinates(5, 9), level: 3);
        updatedHex.AddTerrain(new WaterTerrain(-1));

        sut.UpdateFromHex(updatedHex);

        sut.Level.ShouldBe(3);
        sut.Coordinates.ShouldContain("5");
        sut.Coordinates.ShouldContain("9");
        sut.TerrainTypes.ShouldContain("Water");
        sut.WaterDepth.ShouldBe(1);
        sut.IsWater.ShouldBeTrue();
    }

    [Fact]
    public void UpdateFromHex_ShouldClearWaterWhenRemoved()
    {
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new WaterTerrain(-1));
        var sut = new HexViewModel(hex);
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
        var sut = new HexViewModel(hex);

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
}