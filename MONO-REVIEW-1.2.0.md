# Unlimited Wallet 1.2.0: Mono source review

This is the unchanged source and project used for the quarantined Mono release.
`Plugin.cs` is shared with the IL2CPP edition. The Mono project defines `MONO`,
so IL2CPP-only sections are excluded from this build. `launch_wallet_steam.sh`
is the exact Linux launch helper shipped in the release.

## Requirements

- .NET SDK (the original build and this source review were compiled with 6.0.428).
- BepInEx 6 Unity.Mono build 753, extracted separately:
  https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-win-x64-6.0.0-be.753%2B0d275a4.zip
- Original, unmodified managed game assemblies from a legally installed Mono
  edition of Kingdom Two Crowns. The original compile references came from
  native Linux 2.4.2. Windows Mono 2.1.3 was separately checked and runtime tested.

The game assemblies and BepInEx dependencies are intentionally not distributed
in this source package. Select the original game's Data directory containing
`Managed/Assembly-CSharp.dll`. IL2CPP interop assemblies are not a substitute.

## Build on Windows / PowerShell

From this source directory, replacing the two example paths:

```powershell
./build-mono.ps1 -GameDataPath 'D:/MonoGame/KingdomTwoCrowns_Data' -BepInExCorePath 'D:/BepInEx753/BepInEx/core'
```

Output: `Mono/bin/Release/netstandard2.1/KingdomUnlimitedWallet.dll`.
The script runs `dotnet build -c Release` with the external reference paths.
It does not download or install anything and does not copy dependencies to the
release output. Build verification does not launch or modify the game.

## Review of the release contents

The public Mono ZIP contains exactly three members:

1. `BepInEx/plugins/KingdomUnlimitedWallet/KingdomUnlimitedWallet.dll`
2. `BepInEx/plugins/KingdomUnlimitedWallet/README.txt`
3. `launch_wallet_steam.sh`

There are no nested archives or password protection. The Bash helper preserves
Steam's command arguments, inserts the separately installed `run_bepinex.sh`
before the game executable, then runs that command. It makes no downloads.
The plugin changes the player's wallet capacity and physical purse display,
then renders counters using game-owned sprites and font loaded at runtime.
It uses Harmony to patch game methods. It has no native P/Invoke imports,
HTTP client, downloader, process launcher or registry modifications.
Its file probe checks only for `wallet-verify.flag` under BepInEx's plugin path;
the in-source verification routine is compiled only in the IL2CPP edition.
Normal configuration/log handling is provided by BepInEx.

Release ZIP SHA-256:
`B492C5CC1A51BB22C8F0BD87AC55225E560AE9398F24E07CB308C0383960A041`

Shipped Mono DLL SHA-256:
`3CBF73B6F18E84EF906DBC5ED95FC5AF76B33C62F1C0C0CD853EDDB5813FFAB0`

Source SHA-256 (`Plugin.cs`):
`469B42359E412A51A46FD7A8C4303DDDBC614507B77AC5697B852A360CEEDBB8`

Linux helper SHA-256:
`B21CC4B55A8C655AC36A97A5141B0A59EB3302604B5BA72CB94169023471AE3E`

An SDK build from a relocated source directory can produce a different DLL
hash due to build paths/PDB metadata; an identical binary hash is not claimed.

## Reported scan and runtime verification

On 11 October 2026, the VirusTotal report linked directly from Nexus reported
0/67 security-vendor detections for the exact release ZIP:
https://www.virustotal.com/gui/file/b492c5cc1a51bb22c8f0bd87ac55225e560ae9398f24e07cb308c0383960a041

The Nexus file remains quarantined; its internal reason is unknown. Zero
VirusTotal detections do not replace Nexus's own review.

The exact shipped Mono DLL passed coin/gem pickup and payment using native
game objects, unchanged NPC capacity, disk saving and a fresh-process reload
on Windows Mono 2.1.3 and native Linux Mono 2.4.2. Reload restored 100123 coins
and 4567 gems. Private test helpers were removed for normal HUD checks and
are excluded from both release and source-review packages. Other game builds,
all DLC, multiplayer, island travel and death have not been fully verified.
