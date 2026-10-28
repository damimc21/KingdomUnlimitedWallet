# Unlimited Wallet 1.3.2

Source and packaging review for [Unlimited Wallet](https://www.nexusmods.com/kingdomtwocrowns/mods/45), by RompeChopos.

Native resource counters without pouch overflow, plus optional shield health and
independent resource loss. Press **F2** for difficulty settings. Protection recovers
through actual resource pickups and persists per campaign/reign/player. New
difficulty options apply in solo/local co-op; online retains native damage.

One ZIP contains both editions: select exactly one in Vortex, or copy one
`Manual/<edition>` folder into the game root. Matching BepInEx 6 is installed
separately; there is no additional wallet support package.

- [Build both editions and reproduce the release ZIP](BUILD.md)
- [Review notes and validation scope](REVIEW-1.3.2.md)
- Shared source: [Plugin.cs](Plugin.cs), [CrownProtection.cs](CrownProtection.cs),
  [ProtectionPool.cs](ProtectionPool.cs), [WalletSettings.cs](WalletSettings.cs)
- [IL2CPP project](IL2CPP/KingdomUnlimitedWallet.IL2CPP.csproj) / [Mono project](Mono/KingdomUnlimitedWallet.Mono.csproj)
- [Exact ZIP inventory and hashes](Packaging/release-manifest.json)
- [Source hashes](Packaging/source-manifest.json) / [package builder](build-package.py)
- [Vortex selector](Packaging/files/fomod/ModuleConfig.xml) / [Linux launcher](Mono/launch_wallet_steam.sh)

The two shipped DLLs were tested on Windows IL2CPP 2.4.3, Windows Mono 2.1.3 and
native Linux Mono 2.4.2. See the review notes for the checks and limitations.

Builds require locally supplied game/BepInEx reference libraries. This repository
does not redistribute game assemblies, sprites, fonts or third-party loaders.
Private test helpers and personal saves are not published here.

The previous 1.2.0 source and review are preserved in Git history at
[`64329a4`](https://github.com/damimc21/KingdomUnlimitedWallet/tree/64329a4).
