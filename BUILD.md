# Build and review Unlimited Wallet 1.2.0

## Files being reviewed

- Nexus game 2695, mod 45, final ZIP **file 157**, uploaded 12 October 2026.
- Earlier IL2CPP file 155 passed Nexus scans; earlier Mono file 156 is in review.
- ZIP SHA-256: `1EC068F068F299FB2E45BB4BFAB2C822ED02079F19AA8652B36B5176F0ACC254`.
- IL2CPP DLL SHA-256: `4B3A1AF40618F2C40ED311D16BB93E3EEC432ADAD389E9B394B070122224D92F`.
- Mono DLL SHA-256: `3CBF73B6F18E84EF906DBC5ED95FC5AF76B33C62F1C0C0CD853EDDB5813FFAB0`.
- Shared `Plugin.cs` SHA-256: `469B42359E412A51A46FD7A8C4303DDDBC614507B77AC5697B852A360CEEDBB8`.

The final package reuses both DLLs byte for byte. Its nine entries are listed in
[the manifest](Packaging/release-manifest.json). There are no nested archives,
password protection, game assets, bundled loaders or external test-helper DLLs.

## Prerequisites

Use .NET SDK **6.0.428**, matching the original builds, and Python 3 for packaging.
`global.json` selects that SDK. Install BepInEx separately in a legally owned
game installation; third-party/game references are deliberately not included.

**IL2CPP (current Windows game, tested 2.4.2):**

1. Install [BepInEx 6 IL2CPP Windows x64 build 753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753%2B0d275a4.zip).
2. Apply [Cpp2IL.Patch](https://github.com/abevol/KingdomMod/releases/download/2.4.0/Cpp2IL.Patch.zip)
   and [Il2CppInterop.Patch](https://github.com/abevol/KingdomMod/releases/download/2.4.3/Il2CppInterop.Patch.zip)
   following [abevol's guide](https://github.com/abevol/KingdomMod#install).
3. Start that game once to generate `BepInEx/interop`, then close it before building.

**Mono (Windows or native Linux):** obtain the original Mono game's Data directory
with `Managed/Assembly-CSharp.dll` and [BepInEx 6 Mono Windows x64 build 753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-win-x64-6.0.0-be.753%2B0d275a4.zip)
or [Linux x64 build 753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-linux-x64-6.0.0-be.753%2B0d275a4.zip).
The original Mono compile references were from native Linux 2.4.2; Windows Mono
2.1.3 was separately checked and runtime tested. IL2CPP interop files are not a
substitute for original Mono managed assemblies. The Mono runtime needs .NET Standard 2.1.

## Compile (PowerShell)

Run from this repository, replacing these example reference paths:

```powershell
./build-il2cpp.ps1 -GamePath 'D:/IL2CPPGame/Kingdom Two Crowns'
./build-mono.ps1 -GameDataPath 'D:/MonoGame/KingdomTwoCrowns_Data' -BepInExCorePath 'D:/BepInExMono753/BepInEx/core'
```

Outputs:

- `IL2CPP/bin/Release/net6.0/KingdomUnlimitedWallet.dll`
- `Mono/bin/Release/netstandard2.1/KingdomUnlimitedWallet.dll`

Both projects compile the same unchanged `Plugin.cs`; only Mono defines `MONO`.
The helpers run `dotnet build -c Release`; they do not download or install game
content and do not copy reference DLLs to the outputs. Restore may consult the
configured NuGet sources for SDK framework references. There are no explicit
NuGet PackageReference dependencies in these projects.

## Package

```powershell
python ./build-package.py --il2cpp-dll ./IL2CPP/bin/Release/net6.0/KingdomUnlimitedWallet.dll --mono-dll ./Mono/bin/Release/netstandard2.1/KingdomUnlimitedWallet.dll --output ./Unlimited-Wallet-1.2.0.zip
```

This combines the two supplied plugin DLLs with the seven exact text/launcher
files under `Packaging/files`. It prints the output SHA-256 and whether it
matches the file 157 release hash. Supplying the two original shipped DLLs
reproduces that ZIP byte for byte. Recompiling in a different directory can
change build/PDB metadata and binary hashes, so a byte-identical newly compiled
DLL is not claimed. The package builder accepts different compiler outputs but
reports the difference transparently; it requires exact non-DLL payload hashes.

## Behaviour to review

The plugin patches player wallet methods with Harmony, removes normal pouch
capacity/physical overflow and draws counters with sprites/font from the game
at runtime. Configuration and logging use BepInEx. There are no native P/Invoke
imports, HTTP clients/downloaders, process-launch APIs or registry writes.
The source probes for `wallet-verify.flag` below BepInEx's plugin directory.
The IL2CPP build includes an opt-in diagnostic wallet routine; it snapshots and
restores wallet data and is inactive without that flag. Mono excludes this
routine. No flag or private test-helper binaries are in the shipped package.

`launch_wallet_steam.sh` is a short native-Linux helper: it preserves Steam
arguments and invokes the separately installed `run_bepinex.sh` before the game
executable. It has no downloader. Its exact source is included here.

`fomod/ModuleConfig.xml` selects exactly ONE edition and copies its files.
`vortex_override_instructions.json` requests Vortex's built-in `dinput` (Engine
Injector) deployment type, which deploys relative to the game root. This fixes
the catalogue extension's default Data/Plugins destination for this BepInEx
plugin. The JSON is consumed by Vortex and is not deployed. This package does
not modify Vortex's installation or redistribute another author's extension.

## Validation and limits

The original DLLs passed native coin/gem pickup, spending, unchanged NPC wallet
capacity and save/fresh-process reload on Windows IL2CPP 2.4.2, Windows Mono
2.1.3, native Linux Mono 2.4.2 and Windows IL2CPP through Proton. Reload restored
100123 coins and 4567 gems. Helpers were removed for normal HUD checks.
The final installer separately passed both edition selections and activation/
deactivation in Vortex 2.8.0 with catalogue game support and an isolated game
copy. Exactly one correct wallet DLL was deployed, removal worked and all 228
separately installed BepInEx loader files remained unchanged. Manual copies and
Linux launcher archive permissions were checked. The automation tool could not
perform a physical drag across windows; real imports used Install From File.

Other builds, multiplayer, every DLC, travel and death are not fully verified.
Neither the earlier file's VirusTotal report nor a local build substitutes for
Nexus's independent security review. See [the historical Mono review](MONO-REVIEW-1.2.0.md)
for the original file 156 details.
