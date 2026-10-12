param([Parameter(Mandatory = $true)][string]$GamePath)
$ErrorActionPreference = 'Stop'
$walletGame = (Resolve-Path -LiteralPath $GamePath).Path
foreach ($required in @('GameAssembly.dll', 'BepInEx/core/BepInEx.Unity.IL2CPP.dll', 'BepInEx/interop/Assembly-CSharp.dll')) {
    if (!(Test-Path -LiteralPath (Join-Path $walletGame $required))) { throw "Missing reference: $required" }
}
& dotnet build (Join-Path $PSScriptRoot 'IL2CPP/KingdomUnlimitedWallet.IL2CPP.csproj') -c Release --nologo "-p:GamePath=$walletGame"
if ($LASTEXITCODE -ne 0) { throw 'IL2CPP source build failed.' }
Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'IL2CPP/bin/Release/net6.0/KingdomUnlimitedWallet.dll') -Algorithm SHA256
