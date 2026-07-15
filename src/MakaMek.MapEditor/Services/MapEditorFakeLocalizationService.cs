using Sanet.MakaMek.Localization;

namespace Sanet.MakaMek.MapEditor.Services;

public class MapEditorFakeLocalizationService : FakeLocalizationService
{
    public MapEditorFakeLocalizationService()
    {
        Strings["MainMenu_Title"] = "MakaMek Map Editor";
        Strings["MainMenu_CreateNewMap"] = "Create New Map";
        Strings["MainMenu_LoadMap"] = "Load Map";
        Strings["MainMenu_LoadingTerrain"] = "Loading terrain data...";
        Strings["MainMenu_ErrorLoadingTerrain"] = "Error loading terrain data";
        Strings["MainMenu_Retry"] = "Retry";
        Strings["Status_BiomesLoaded"] = "{0} biomes loaded";
        Strings["Status_NoBiomesFound"] = "No biomes found";
        Strings["Status_ErrorLoadingBiomes"] = "Error loading biomes: {0}";

        Strings["NewMap_Title"] = "New Map Configuration";
        Strings["NewMap_Width"] = "Width:";
        Strings["NewMap_Height"] = "Height:";
        Strings["NewMap_PreGenerateTerrain"] = "Pre-generate Terrain";
        Strings["NewMap_ForestCoverage"] = "Forest Coverage %";
        Strings["NewMap_LightWoods"] = "Light Woods %";
        Strings["NewMap_CreateMap"] = "Edit map";

        Strings["EditMap_ExportMap"] = "Save Map";
        Strings["EditMap_RaiseLevel"] = "▲ Raise Level";
        Strings["EditMap_LowerLevel"] = "▼ Lower Level";
        Strings["EditMap_IncreaseWaterDepth"] = "▼ Increase Depth";
        Strings["EditMap_DecreaseWaterDepth"] = "▲ Decrease Depth";
        Strings["EditMap_ExportMapDialogTitle"] = "Save Map";
        Strings["EditMap_ExportPdf"] = "Export PDF";
        Strings["EditMap_ExportPdfDialogTitle"] = "Export Map as PDF";
        Strings["EditMap_PdfFilesFilter"] = "PDF Files";
        Strings["EditMap_Cursor"] = "Select";
        Strings["EditMap_Elevation"] = "Elevation:";
        Strings["EditMap_Terrains"] = "Terrains:";
        Strings["EditMap_WaterDepth"] = "Water Depth:";
        Strings["EditMap_Settings"] = "☰ Settings";
        Strings["EditMap_CloseEditMap"] = "Exit Editor";
        Strings["EditMap_CloseConfirmTitle"] = "Close Map";
        Strings["EditMap_CloseConfirmMessage"] = "Are you sure you want to close this map? Any unsaved changes will be lost.";
        Strings["EditMap_CloseHexInfo"] = "Close";

        Strings["Window_Title"] = "MakaMek Map Editor";

        Strings["About_Title"] = "MakaMek Map Editor";
        Strings["About_Description"] = "A companion tool for the MakaMek tabletop wargame, providing hex map creation and editing capabilities.";
        Strings["About_Attribution"] = "Can be used to create generic hex-based maps for tabletops.";
        Strings["About_FossStatement"] = "This software is free and open source, licensed under the GPL-3.0.";
        Strings["About_TrademarkDisclaimer"] = "This project is not affiliated with the copyright or trademark holders of any existing wargames.";
        Strings["About_GitHubButton"] = "View on GitHub";
        Strings["About_ContactButton"] = "Contact";
        Strings["About_CloseButton"] = "Close";
        Strings["NewMap_AboutButton"] = "About";
        Strings["EditMap_AboutButton"] = "About";
    }
}
