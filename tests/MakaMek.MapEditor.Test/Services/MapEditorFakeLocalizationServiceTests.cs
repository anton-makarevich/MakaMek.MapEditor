using Sanet.MakaMek.MapEditor.Services;
using Shouldly;
using Xunit;

namespace MakaMek.MapEditor.Test.Services;

public class MapEditorFakeLocalizationServiceTests
{
    [Theory]
    [InlineData("MainMenu_Title", "MakaMek Map Editor")]
    [InlineData("MainMenu_CreateNewMap", "Create New Map")]
    [InlineData("MainMenu_LoadMap", "Load Map")]
    [InlineData("MainMenu_LoadingTerrain", "Loading terrain data...")]
    [InlineData("MainMenu_ErrorLoadingTerrain", "Error loading terrain data")]
    [InlineData("MainMenu_Retry", "Retry")]
    public void GetString_MainMenu_ReturnsExpectedString(string key, string expected)
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("Status_BiomesLoaded", "{0} biomes loaded")]
    [InlineData("Status_NoBiomesFound", "No biomes found")]
    [InlineData("Status_ErrorLoadingBiomes", "Error loading biomes: {0}")]
    public void GetString_StatusMessages_ReturnsExpectedString(string key, string expected)
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("NewMap_Title", "New Map Configuration")]
    [InlineData("NewMap_Width", "Width:")]
    [InlineData("NewMap_Height", "Height:")]
    [InlineData("NewMap_PreGenerateTerrain", "Pre-generate Terrain")]
    [InlineData("NewMap_ForestCoverage", "Forest Coverage %")]
    [InlineData("NewMap_LightWoods", "Light Woods %")]
    [InlineData("NewMap_CreateMap", "Edit map")]
    public void GetString_NewMapView_ReturnsExpectedString(string key, string expected)
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("EditMap_ExportMap", "Save Map")]
    [InlineData("EditMap_RaiseLevel", "▲ Raise Level")]
    [InlineData("EditMap_LowerLevel", "▼ Lower Level")]
    [InlineData("EditMap_IncreaseWaterDepth", "▼ Increase Depth")]
    [InlineData("EditMap_DecreaseWaterDepth", "▲ Decrease Depth")]
    [InlineData("EditMap_Cursor", "Select")]
    [InlineData("EditMap_ExportMapDialogTitle", "Save Map")]
    [InlineData("EditMap_ExportPdf", "Export PDF")]
    [InlineData("EditMap_ExportPdfDialogTitle", "Export Map as PDF")]
    [InlineData("EditMap_PdfFilesFilter", "PDF Files")]
    [InlineData("EditMap_Settings", "☰ Settings")]
    [InlineData("EditMap_CloseEditMap", "Exit Editor")]
    [InlineData("EditMap_CloseConfirmTitle", "Close Map")]
    [InlineData("EditMap_CloseConfirmMessage", "Are you sure you want to close this map? Any unsaved changes will be lost.")]
    [InlineData("EditMap_CloseHexInfo", "Close")]
    [InlineData("EditMap_Elevation", "Elevation:")]
    [InlineData("EditMap_Terrains", "Terrains:")]
    [InlineData("EditMap_WaterDepth", "Water Depth:")]
    public void GetString_EditMapView_ReturnsExpectedString(string key, string expected)
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("Window_Title", "MakaMek Map Editor")]
    public void GetString_Window_ReturnsExpectedString(string key, string expected)
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("About_Title", "MakaMek Map Editor")]
    [InlineData("About_Description", "A companion tool for the MakaMek tabletop wargame, providing hex map creation and editing capabilities.")]
    [InlineData("About_Attribution", "Can be used to create generic hex-based maps for tabletops.")]
    [InlineData("About_FossStatement", "This software is free and open source, licensed under the GPL-3.0.")]
    [InlineData("About_TrademarkDisclaimer", "This project is not affiliated with the copyright or trademark holders of any existing wargames.")]
    [InlineData("About_GitHubButton", "View on GitHub")]
    [InlineData("About_ContactButton", "Contact")]
    [InlineData("About_CloseButton", "Close")]
    public void GetString_About_ReturnsExpectedString(string key, string expected)
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("NonExistentKey")]
    [InlineData("Some_Random_Key")]
    [InlineData("")]
    public void GetString_UnknownKey_ReturnsKeyItself(string key)
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString(key);

        result.ShouldBe(key);
    }

    [Fact]
    public void GetString_InheritedKey_ReturnsBaseClassValue()
    {
        var localizationService = new MapEditorFakeLocalizationService();

        var result = localizationService.GetString("Command_JoinGame");

        result.ShouldBe("{0} has joined game with {1} units");
    }
}
