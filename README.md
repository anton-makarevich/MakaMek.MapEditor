# MakaMek.MapEditor

A cross-platform standalone map editor for [MakaMek](https://github.com/anton-makarevich/MakaMek), built with .NET 10 and AvaloniaUI.

## Overview

MakaMek.MapEditor is a dedicated tool for creating and editing maps to be used with the MakaMek game. This standalone application allows users to design custom battlefields with various terrain types that can be imported into MakaMek for gameplay.

## Features

- **Visual Map Design**: Intuitive graphical interface for creating hex-based maps
- **Terrain Types**: Support for various terrain types (clear, woods, rough, water, etc.)
- **Map Validation**: Built-in validation to ensure maps are compatible with MakaMek
- **Import/Export**: Save and load maps in MakaMek-compatible formats
- **Cross-Platform**: Runs on Windows, Linux, macOS, Web and Mobile

## Technology Stack

- .NET 10
- AvaloniaUI for cross-platform UI

## Project Status

| Component                  | Build Status                                                                                                                                                                                            | Package/Download                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
|----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **MakaMek.MapEditor**      | [![build](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/mapeditor.yml/badge.svg)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/mapeditor.yml)      | [![NuGet Version](https://img.shields.io/nuget/vpre/Sanet.MakaMek.MapEditor?logo=nuget)](https://www.nuget.org/packages/Sanet.MakaMek.MapEditor)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| **Web Version (WASM)**     | [![Build and Release Browser App](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-browser.yml/badge.svg)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-browser.yml)| [![Play in Browser](https://img.shields.io/badge/Play-in%20Browser-blue?logo=github)](https://anton-makarevich.github.io/MakaMek.MapEditor/)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| **Desktop Version**        | [![Build and Release Desktop App](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-desktop.yml/badge.svg)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-desktop.yml)      | [![Download Desktop App](https://img.shields.io/badge/Download-Windows%20Desktop-orange?logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAACXBIWXMAAAsTAAALEwEAmpwYAAABE0lEQVR4nO3aMUrEYBDF8R/Y2W1hoa29CF7BwgvoEWw9gI2lF9ADWNraiTarWwhewcJKOytL/UTIQhrjsolk17w/pBnCzDd88xjICyGEZWUV2zjAMS7wgDeMLSAb2MUhTnGFJ3ygNDy9MMIO9nGCSzzi/ZfDlj4a+R6FLazXYufVKMx72PJXjaxgE3s4qg56g2d8VgVea+9PY701Mmo5ClNKn41cd1ik10ZKGpEbKRmtBqIR0YhopIloRDQiGmnivqVOxh3mKjPUCEvDpOW133WYq8xQ40eyR2SPyB5pIhoRjYhGmohGRCOikUF8xP43tkJbo+dlkYyeNi7sWi12tqjW2yDM0EHY013/MHA7V8YQBswXmfZIX4+AWlMAAAAASUVORK5CYII=)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-desktop.yml) |
| **Android Version**        | [![Build and Release Android APK](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-android.yml/badge.svg)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-android.yml)      | [![Download Android APK](https://img.shields.io/badge/Download-Android%20APK-green?logo=android)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-android.yml)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| **iOS Version**            | [![Build and Release iOS App](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-ios.yml/badge.svg)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-ios.yml)            | [![Download iOS App](https://img.shields.io/badge/Download-iOS%20App-orange?logo=apple)](https://github.com/anton-makarevich/MakaMek.MapEditor/actions/workflows/build-ios.yml)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |

## Development Setup

### Prerequisites

- .NET 10 SDK
- Your favorite IDE (Visual Studio, Rider or VS Code)

### Building

1. Clone the repository
2. Open `MakaMek.MapEditor.slnx` in your IDE
3. Build the solution

## License

The source code for this project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This is a companion tool for the MakaMek fan-made project and is not affiliated with or endorsed by any commercial mech combat game properties. All trademarks belong to their respective owners.
