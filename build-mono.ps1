param(
    [Parameter(Mandatory = $true)][string]$GameDataPath,
    [Parameter(Mandatory = $true)][string]$BepInExCorePath
)
$ErrorActionPreference = 'Stop'
$gameData = (Resolve-Path -LiteralPath $GameDataPath).Path
$loaderCore = (Resolve-Path -LiteralPath $BepInExCorePath).Path
foreach ($required in @('Assembly-CSharp.dll','Assembly-CSharp-firstpass.dll','UnityEngine.dll','UnityEngine.CoreModule.dll','UnityEngine.UI.dll','UnityEngine.UIModule.dll','UnityEngine.TextRenderingModule.dll')) {
    if (!(Test-Path -LiteralPath (Join-Path $gameData "Managed/$required"))) { throw "Missing game reference: $required" }
}
foreach ($required in @('BepInEx.Core.dll','BepInEx.Unity.Mono.dll','0Harmony.dll')) {
    if (!(Test-Path -LiteralPath (Join-Path $loaderCore $required))) { throw "Missing loader reference: $required" }
}
& dotnet build (Join-Path $PSScriptRoot 'Mono/KingdomUnlimitedWallet.Mono.csproj') -c Release --nologo "-p:MonoReferencePath=$gameData" "-p:MonoCorePath=$loaderCore"
if ($LASTEXITCODE -ne 0) { throw 'Mono source build failed.' }
Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Mono/bin/Release/netstandard2.1/KingdomUnlimitedWallet.dll') -Algorithm SHA256
