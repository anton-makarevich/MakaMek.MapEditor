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
        Strings["NewMap_CreateMap"] = "Create Map";

        Strings["EditMap_ExportMap"] = "Export Map";
        Strings["EditMap_RaiseLevel"] = "▲ Raise Level";
        Strings["EditMap_LowerLevel"] = "▼ Lower Level";
        Strings["EditMap_IncreaseWaterDepth"] = "▼ Increase Depth";
        Strings["EditMap_DecreaseWaterDepth"] = "▲ Decrease Depth";
        Strings["EditMap_ExportMapDialogTitle"] = "Export Map";
        Strings["EditMap_ExportPdf"] = "Export PDF";
        Strings["EditMap_ExportPdfDialogTitle"] = "Export Map as PDF";
        Strings["EditMap_PdfFilesFilter"] = "PDF Files";
        Strings["EditMap_Cursor"] = "Cursor";
        Strings["EditMap_Elevation"] = "Elevation:";
        Strings["EditMap_Terrains"] = "Terrains:";
        Strings["EditMap_WaterDepth"] = "Water Depth:";
        Strings["EditMap_Settings"] = "☰ Settings";
        Strings["EditMap_CloseHexInfo"] = "Close";

        Strings["Window_Title"] = "MakaMek Map Editor";
    }
}
