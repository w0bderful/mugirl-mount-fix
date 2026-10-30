param([string]$GameDir = 'D:/SteamLibrary/steamapps/common/RimWorld', [string]$WorkshopDir = 'D:/SteamLibrary/steamapps/workshop/content/294100')
$ErrorActionPreference = 'Stop'
& dotnet build "$PSScriptRoot/MugirlMountFix.csproj" -c Release -nologo -v q "-p:GameDir=$GameDir" "-p:WorkshopDir=$WorkshopDir"
if ($LASTEXITCODE -ne 0) { throw 'Mugirl Mount Fix build failed' }
