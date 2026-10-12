using Microsoft.Extensions.DependencyInjection;
using Sanet.Localization;
using Sanet.Localization.Providers;
using Sanet.MakaMek.Localization;
using Sanet.MakaMek.MapEditor.Services;
using Shouldly;
using Xunit;
using EditorServices = Sanet.MakaMek.MapEditor.DI.ServiceCollectionExtensions;

namespace MakaMek.MapEditor.Test.Services;

public class EditorLocalizationTests
{
    private readonly LocalizationService _sut = new(EditorServices.CreateLocalizationResourcesProvider());

    [Theory]
    [InlineData("MainMenu_Title", "MakaMek Map Editor")]
    [InlineData("MainMenu_CreateNewMap", "Create New Map")]
    [InlineData("MainMenu_LoadMap", "Load Map")]
    [InlineData("MainMenu_LoadingTerrain", "Loading terrain data...")]
    [InlineData("MainMenu_ErrorLoadingTerrain", "Error loading terrain data")]
    [InlineData("MainMenu_Retry", "Retry")]
    public void GetString_MainMenu_ReturnsExpectedString(string key, string expected)
    {
        var result = _sut.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("Status_BiomesLoaded", "{0} biomes loaded")]
    [InlineData("Status_NoBiomesFound", "No biomes found")]
    [InlineData("Status_ErrorLoadingBiomes", "Error loading biomes: {0}")]
    public void GetString_StatusMessages_ReturnsExpectedString(string key, string expected)
    {
        var result = _sut.GetString(key);

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
    [InlineData("NewMap_AboutButton", "About")]
    public void GetString_NewMapView_ReturnsExpectedString(string key, string expected)
    {
        var result = _sut.GetString(key);

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
    [InlineData("EditMap_RoadBridge", "Road/Bridge")]
    [InlineData("EditMap_BridgeHeight", "Bridge Height:")]
    [InlineData("EditMap_ConstructionFactor", "Construction:")]
    [InlineData("EditMap_RaiseBridge", "▲ Raise Bridge")]
    [InlineData("EditMap_LowerBridge", "▼ Lower Bridge")]
    [InlineData("EditMap_IncreaseConstructionFactor", "▲ Increase CF")]
    [InlineData("EditMap_DecreaseConstructionFactor", "▼ Decrease CF")]
    [InlineData("EditMap_AboutButton", "About")]
    public void GetString_EditMapView_ReturnsExpectedString(string key, string expected)
    {
        var result = _sut.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("Window_Title", "MakaMek Map Editor")]
    public void GetString_Window_ReturnsExpectedString(string key, string expected)
    {
        var result = _sut.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("About_Title", "MakaMek Map Editor")]
    [InlineData("About_Description", "A companion tool for the MakaMek tabletop wargame, providing hex map creation and editing capabilities.")]
    [InlineData("About_Attribution", "Can be used to create generic hex-based maps for tabletops.")]
    [InlineData("About_FossStatement", "This software is free and open source, licensed under the GPL-3.0.")]
    [InlineData("About_TerrainAttribution", "Terrain artwork is taken from the MegaMek Data repository (github.com/MegaMek/mm-data). MegaMek Data (C) by The MegaMek Team is licensed under CC BY-NC-SA 4.0 (creativecommons.org/licenses/by-nc-sa/4.0).")]
    [InlineData("About_TrademarkDisclaimer", "This project is not affiliated with the copyright or trademark holders of any existing wargames.")]
    [InlineData("About_GitHubButton", "View on GitHub")]
    [InlineData("About_ContactButton", "Contact")]
    [InlineData("About_PrivacyPolicyButton", "Privacy Policy")]
    [InlineData("About_CloseButton", "Close")]
    public void GetString_About_ReturnsExpectedString(string key, string expected)
    {
        var result = _sut.GetString(key);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("NonExistentKey")]
    [InlineData("Some_Random_Key")]
    [InlineData("")]
    public void GetString_UnknownKey_ReturnsKeyItself(string key)
    {
        var result = _sut.GetString(key);

        result.ShouldBe(key);
    }

    [Theory]
    [InlineData("Command_JoinGame", "{0} has joined game with {1} units")]
    [InlineData("BattleMap_Settings", "Settings")]
    [InlineData("BattleMap_ShowLabels", "Show Labels")]
    [InlineData("BattleMap_ShowHexOutlines", "Show Hex Outlines")]
    [InlineData("Dialog_Yes", "Yes")]
    [InlineData("Dialog_No", "No")]
    [InlineData("Terrain_Clear", "Clear")]
    [InlineData("Terrain_LightWoods", "Light Woods")]
    [InlineData("Terrain_HeavyWoods", "Heavy Woods")]
    [InlineData("Terrain_Rough", "Rough")]
    [InlineData("Terrain_Water", "Water")]
    [InlineData("Terrain_Road", "Road")]
    [InlineData("Terrain_Pavement", "Pavement")]
    [InlineData("Terrain_Bridge", "Bridge")]
    [InlineData("Terrain_Rubble", "Rubble")]
    public void GetString_InheritedKey_ReturnsMakaMekResourceValue(string key, string expected)
    {
        var makaMekProvider = new EmbeddedResourcesProvider(
            typeof(LocalizationResourcesAnchor).Assembly, LocalizationResourcesAnchor.ResourceBaseName);
        var defaultLanguage = makaMekProvider.GetAvailableLanguages().Single(language => language.IsDefault);

        makaMekProvider.GetStrings(defaultLanguage)[key].ShouldBe(expected);
        _sut.GetString(key).ShouldBe(expected);
    }

    [Fact]
    public void ActiveLanguage_OnCreation_IsDefaultEnglish()
    {
        _sut.ActiveLanguage.Code.ShouldBe("en");
        _sut.ActiveLanguage.IsDefault.ShouldBeTrue();
        _sut.ActiveLanguage.Name.ShouldBe("English");
    }

    [Fact]
    public void EditorResources_ManifestName_MatchesAnchor()
    {
        typeof(EditorLocalizationResources).Assembly.GetManifestResourceNames()
            .ShouldContain($"{EditorLocalizationResources.ResourceBaseName}.resources");
    }

    [Fact]
    public void EditorResources_OnlyDuplicateDeliberateOverrides()
    {
        var editorProvider = new EmbeddedResourcesProvider(
            typeof(EditorLocalizationResources).Assembly, EditorLocalizationResources.ResourceBaseName);
        var makaMekProvider = new EmbeddedResourcesProvider(
            typeof(LocalizationResourcesAnchor).Assembly, LocalizationResourcesAnchor.ResourceBaseName);
        var editorStrings = editorProvider.GetStrings(editorProvider.GetAvailableLanguages().Single(l => l.IsDefault));
        var makaMekStrings = makaMekProvider.GetStrings(makaMekProvider.GetAvailableLanguages().Single(l => l.IsDefault));

        editorStrings.Keys.Intersect(makaMekStrings.Keys)
            .Where(key => key != "language_name").Order(StringComparer.Ordinal).ShouldBe(
                new[] { "EditMap_ExportMapDialogTitle", "Window_Title" });
    }

    [Theory]
    [InlineData("EditMap_ExportMapDialogTitle", "Export Map File", "Save Map")]
    [InlineData("Window_Title", "MakaMek", "MakaMek Map Editor")]
    public void GetString_SharedKey_EditorOverridesMakaMek(string key, string sharedValue, string editorValue)
    {
        var makaMekProvider = new EmbeddedResourcesProvider(
            typeof(LocalizationResourcesAnchor).Assembly, LocalizationResourcesAnchor.ResourceBaseName);
        var sharedLocalization = new LocalizationService(makaMekProvider);

        sharedLocalization.GetString(key).ShouldBe(sharedValue);
        _sut.GetString(key).ShouldBe(editorValue);
    }

    [Fact]
    public void RegisterServices_RegistersSingletonCompositeLocalization()
    {
        var services = new ServiceCollection();
        EditorServices.RegisterServices(services);
        using var serviceProvider = services.BuildServiceProvider();

        var localization = serviceProvider.GetRequiredService<ILocalizationService>();

        localization.ShouldBeOfType<LocalizationService>();
        localization.ShouldBeSameAs(serviceProvider.GetRequiredService<ILocalizationService>());
        serviceProvider.GetRequiredService<ILocalizationResourcesProvider>().ShouldBeOfType<CompositeResourcesProvider>();
        localization.GetString("MainMenu_Title").ShouldBe("MakaMek Map Editor");
        localization.GetString("BattleMap_Settings").ShouldBe("Settings");
    }
}