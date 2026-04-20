<div align="center">

![Starfield Weapon Icon File Builder](https://pxcnet.xyz/Starfield/images/StarfieldWeaponIconFileBuilderBanner.png)
[![Build and Release](https://github.com/ProfX66/StarfieldWeaponIconFileBuilder/actions/workflows/release.yml/badge.svg?event=push)](https://github.com/ProfX66/StarfieldWeaponIconFileBuilder/actions/workflows/release.yml)
![Latest Release](https://img.shields.io/github/v/release/ProfX66/StarfieldWeaponIconFileBuilder)
[![Documentation](https://img.shields.io/badge/Documentation-blue)](https://pxcnet.xyz/Starfield/Resources/Custom%20Weapon%20Icons/StarfieldWeaponIconFileBuilder/)
[![License: GPL-3.0](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

</div>

## About
Custom built multi-platform GUI application which aids Starfield mod authors in creating custom weapon icon SWF files for their mods. It automates the entire process so you do not need to manually use JPEXS FFDec anymore.

This tool exists because I was able to add code and automation inside the [Custom Weapon Icon Template](https://www.nexusmods.com/starfield/mods/13816) which auto scales and positions your weapon icon. The latest version of that template is bundled with this application, as well as a specific version of FFDec which properly imports SVG files and updates their bounds.

All you have to do is download the latest release and unpack it somewhere on your computer and then run it, fill out your weapons details, load a SVG icon and click generate. You will now have a specialized "CCSUP_" SWF file that is ready to be dropped into your BA2 archive.

> **Note**: You will still need to setup your UI_Icon keywords in your mod for it to work correctly.

### Required Dependencies
- Java 8 or newer
- FFDec v26.0.0 or newer _(this is included in the download)_
- .NET 9.0 _(this is embedded with the application)_

## Creating a new Weapon Icon File
Below is an example GIF which shows the "Create New" process.

> Check out the [documentation](https://pxcnet.xyz/Starfield/Resources/Custom%20Weapon%20Icons/StarfieldWeaponIconFileBuilder/#creating-a-new-weapon-icon-file) for more information

[![Documentation](https://img.shields.io/badge/Documentation-blue?style=for-the-badge&logo=read-the-docs)](https://pxcnet.xyz/Starfield/Resources/Custom%20Weapon%20Icons/StarfieldWeaponIconFileBuilder/#creating-a-new-weapon-icon-file)

![Create New Process](https://pxcnet.xyz/Starfield/Resources/images/StarfieldWeaponIconFileBuilder-Create.gif)

## Copying and Renaming an existing Weapon Icon File
Below is an example GIF which shows the "Copy & Rename" process.

> Check out the [documentation](https://pxcnet.xyz/Starfield/Resources/Custom%20Weapon%20Icons/StarfieldWeaponIconFileBuilder/#copying-and-renaming-an-existing-weapon-icon-file) for more information

[![Documentation](https://img.shields.io/badge/Documentation-blue?style=for-the-badge&logo=read-the-docs)](https://pxcnet.xyz/Starfield/Resources/Custom%20Weapon%20Icons/StarfieldWeaponIconFileBuilder/#copying-and-renaming-an-existing-weapon-icon-file)

![Copy & Rename Process](https://pxcnet.xyz/Starfield/Resources/images/StarfieldWeaponIconFileBuilder-Clone.gif)


---


## Contributing

Contributions are welcome! To submit a Pull Request:

1. Fork the repository.
2. Create a feature branch.
3. Make your changes and follow the existing code style.
4. Submit a Pull Request with a clear description of your changes.

**Important:** By submitting a PR, you agree to license your contributions under the same GPL v3 license as this project.

## Project Name and Logo

The name `StarfieldWeaponIconFileBuilder` and its logo are **not covered under the GPL license**.
You may fork and modify the code, but you **may not use the same project name or logo** for redistributed versions without permission.
Please choose a new name for forks or modified versions.

## Third-Party Dependencies

This project uses the following third-party software:

1. **[Avalonia UI Framework](https://github.com/AvaloniaUI/Avalonia)**
   - Packages included: Core, Desktop, Themes, Fonts, Diagnostics, Svg.Controls
   - Version: 11.3.11 (core), 11.x.x subpackages
   - License: MIT

2. **[CustomMessageBox.Avalonia](https://github.com/Nickelony/CustomMessageBox.Avalonia)**
   - Version: 11.0.0.2
   - License: MIT

3. **[LoadingIndicators.Avalonia.New](https://github.com/Der-Floh/LoadingIndicators.Avalonia)**
   - Version: 2.0.8
   - License: MIT

4. **[Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia)**
   - Version: 3.7.0
   - License: MIT

5. **[CsvHelper](https://joshclose.github.io/CsvHelper/)**
   - Version: 33.1.0
   - License: MIT

6. **[Newtonsoft.Json](https://www.newtonsoft.com/json)**
   - Version: 13.0.4
   - License: MIT

7. **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)**
   - Version: 8.2.1
   - License: MIT

8. **[JPEXS Free Flash Decompiler (FFDec)](https://github.com/jindrapetrik/jpexs-decompiler)**
   - Version: v26.0.0
   - License: GNU General Public License v3 (GPL v3)
   - Copyright (C) JPEXS
   - Notes: FFDec binaries are included unmodified. A copy of the GPL v3 license is included in the `FFDec/` folder.