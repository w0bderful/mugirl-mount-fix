# Mugirl Mount Fix

A standalone RimWorld 1.7 patch that prevents Mugirl riders from automatically dismounting solely because of minor injuries.

Version: **3.0.0** · Package ID: `w0bderful.mugirlmountfix`

## Requirements

- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [Mugirl Race](https://steamcommunity.com/sharedfiles/filedetails/?id=3578882219)

Load this mod after both requirements.

## Behavior

The patch replaces only the medical-rest check in Mugirl's automatic dismount decision. With the default settings, an injured rider may remain mounted when all of these conditions are met:

- Overall health is **above 80%**.
- Total bleeding rate is **at most 5% per day**.
- Pain is **below 30%**.
- There is no other medical reason that requires dismounting.

Other medical causes, including labor, conditions requiring immunity, surgery, and tendable conditions other than injuries, retain their original behavior. Manual dismounting and other automatic or emergency dismount reasons remain controlled by Mugirl. The patch does not change damage, healing, or the rider's actual medical needs.

Open **Options → Mod settings → Mugirl Mount Fix** to enable or disable the correction and adjust the health, bleeding, and pain thresholds. Reset restores the defaults above.

## Installation

1. Download or clone this repository.
2. Place its contents in a folder such as `RimWorld/Mods/Mugirl-Mount-Fix`. The folder must contain `About/About.xml` and `LoadFolders.xml` directly.
3. Enable Harmony, Mugirl Race, and Mugirl Mount Fix, in that order.
4. Restart RimWorld.

The repository includes the compiled mod at `1.7/Assemblies/MugirlMountFix.dll`; building it is optional. The `Source` folder is not needed to play.

## Compatibility

- English and Korean translations are included.
- The original Mugirl DLL is not modified or distributed. If a Mugirl update changes its automatic dismount method, the patch logs a warning or error instead of assuming that the new method is compatible.

## Building

Requires the .NET 10 SDK and a local RimWorld 1.7 installation. From the repository root, run:

```powershell
./Source/build.ps1 `
  -GameDir 'D:/SteamLibrary/steamapps/common/RimWorld' `
  -WorkshopDir 'D:/SteamLibrary/steamapps/workshop/content/294100'
```

The game and Harmony references are read from those installations and are not included in this repository. The output is written to `1.7/Assemblies/`.

## Validation

The mod builds without warnings or errors against the installed RimWorld 1.7 assemblies. Existing project checks cover medical thresholds, disabled behavior, other medical causes, and Harmony patch generation against the installed Mugirl DLL. The patch's installation message was also observed in a full mod-list game startup. These checks do not establish compatibility with every Mugirl version or every mod combination.
