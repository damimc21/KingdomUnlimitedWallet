# Unlimited Wallet — Mono 1.2.0 source

Source review for [Unlimited Wallet — Native Coin and Gem Counters](https://www.nexusmods.com/kingdomtwocrowns/mods/45), by RompeChopos.

This repository contains the source of the experimental BepInEx 6 Mono edition
for Kingdom Two Crowns, tested on Windows Mono 2.1.3 and native Linux Mono 2.4.2.
The C# source is shared with the IL2CPP edition; this Mono project defines `MONO`
and compiles the matching sections.

- [Build instructions and release hashes](BUILD.md)
- [Plugin source](Plugin.cs)
- [Mono project](Mono/KingdomUnlimitedWallet.Mono.csproj)
- [Native Linux Steam launcher](Mono/launch_wallet_steam.sh)
- [Installation instructions (English / Spanish)](Mono/README.txt)

The build requires your own legally installed Mono game's managed assemblies
and a separate BepInEx Mono 753 installation. Game assemblies, sprites, fonts,
third-party dependencies, credentials, and private test helpers are not included.

The source package was rebuilt with .NET SDK 6.0.428 with zero errors/warnings.
The exact release hashes and known testing limits are documented in BUILD.md.
