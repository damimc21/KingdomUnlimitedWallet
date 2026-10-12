# Unlimited Wallet 1.2.0 source

Source and packaging review for [Unlimited Wallet](https://www.nexusmods.com/kingdomtwocrowns/mods/45), by RompeChopos.

The final ZIP (Nexus file **157**) contains the existing IL2CPP and experimental
Mono 1.2.0 plugin DLLs, a Linux launch helper, installation READMEs and a FOMOD
edition selector for Vortex. BepInEx is a separate requirement. The plugin
source and both shipped DLLs are unchanged from files 155 and 156.

- [Build both editions and reproduce the ZIP](BUILD.md)
- [Shared plugin source](Plugin.cs)
- [IL2CPP project](IL2CPP/KingdomUnlimitedWallet.IL2CPP.csproj)
- [Mono project](Mono/KingdomUnlimitedWallet.Mono.csproj)
- [Exact package inventory and hashes](Packaging/release-manifest.json)
- [Package builder](build-package.py)
- [Vortex installer](Packaging/files/fomod/ModuleConfig.xml)
- [Linux launch helper](Mono/launch_wallet_steam.sh)

Builds need the original game and matching BepInEx reference libraries, supplied
by the person compiling. This repository does not redistribute game assemblies,
sprites, fonts, BepInEx or other third-party dependencies. Build scripts do not
launch or alter the game; build output is local and ignored by Git.
