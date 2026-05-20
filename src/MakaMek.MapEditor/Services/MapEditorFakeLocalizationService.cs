using Sanet.MakaMek.Localization;

namespace Sanet.MakaMek.MapEditor.Services;

public class MapEditorFakeLocalizationService : FakeLocalizationService
{
    public MapEditorFakeLocalizationService()
    {
        _strings["MainMenu_Title"] = "MakaMek Map Editor";
        _strings["MainMenu_CreateNewMap"] = "Create New Map";
        _strings["MainMenu_LoadMap"] = "Load Map";
        _strings["MainMenu_LoadingTerrain"] = "Loading terrain data...";
        _strings["MainMenu_ErrorLoadingTerrain"] = "Error loading terrain data";
        _strings["Status_BiomesLoaded"] = "{0} biomes loaded";
        _strings["Status_NoBiomesFound"] = "No biomes found";
        _strings["Status_ErrorLoadingBiomes"] = "Error loading biomes: {0}";

        _strings["NewMap_Title"] = "New Map Configuration";
        _strings["NewMap_Width"] = "Width:";
        _strings["NewMap_Height"] = "Height:";
        _strings["NewMap_PreGenerateTerrain"] = "Pre-generate Terrain";
        _strings["NewMap_ForestCoverage"] = "Forest Coverage %";
        _strings["NewMap_LightWoods"] = "Light Woods %";
        _strings["NewMap_CreateMap"] = "Create Map";

        _strings["EditMap_Toolbox"] = "Toolbox";
        _strings["EditMap_ExportMap"] = "Export Map";
        _strings["EditMap_RaiseLevel"] = "▲ Raise Level";
        _strings["EditMap_LowerLevel"] = "▼ Lower Level";
        _strings["EditMap_ExportMapDialogTitle"] = "Export Map";

        _strings["Window_Title"] = "MakaMek Map Editor";
    }
}
