# Unlimited Wallet

Coin and gem counters without the usual pouch limit, using the game's own icons.
Optional shields keep enemy attacks dangerous. Press **F2** to configure difficulty.

## Download

**[Download Unlimited Wallet 1.3.2.zip](https://github.com/damimc21/KingdomUnlimitedWallet/releases/download/v1.3.2/Unlimited-Wallet-1.3.2.zip)**

[Español](README.es.md) · [All releases](https://github.com/damimc21/KingdomUnlimitedWallet/releases) · [Nexus Mods](https://www.nexusmods.com/kingdomtwocrowns/mods/45)

One ZIP includes both editions. Download the file above; GitHub's **Source code**
archives contain source files, not the installable mod.

## Before installing

Close the game. **Matching BepInEx 6 is required separately** — [loader setup](INSTALL.md).
Disable/remove your previous Unlimited Wallet installation before updating.

| Your game folder contains | Select |
|---|---|
| `GameAssembly.dll` (Windows / Proton) | **IL2CPP** |
| `KingdomTwoCrowns_Data/Managed/Assembly-CSharp.dll` (older Windows / native Linux) | **Mono** |

Find the game folder in Steam: **Properties → Installed Files → Browse**.
Install only one edition.

## Vortex

1. Select **Kingdom Two Crowns** in Vortex. If prompted, install its game support from the Vortex catalogue.
2. In **Mods → Install From File**, select the downloaded ZIP and choose **IL2CPP** or **Mono**.
3. Enable the mod, **Deploy Mods**, then launch the game through Steam.

No separate Unlimited Wallet support ZIP is needed.

## Manual

1. Extract the ZIP outside the game folder.
2. Copy the **contents** of `Manual/IL2CPP` or `Manual/Mono` into the game folder,
   beside its executable. Merge the `BepInEx` folder when asked.
3. Launch the game through Steam. For Linux/Proton, see the [launch options](INSTALL.md#linux-and-proton).

The installed plugin is `BepInEx/plugins/KingdomUnlimitedWallet/KingdomUnlimitedWallet.dll`.

## In game

**F2** opens the optional shield/resource-loss settings. Collect resources to
recover shields. New difficulty options apply in solo and local co-op; online
keeps native damage. Shields are off by default.

<details>
<summary>Source, builds and review</summary>

- [Build instructions and release hashes](BUILD.md)
- [Validation and compatibility](REVIEW-1.3.2.md)
- [Exact package inventory](Packaging/release-manifest.json)
- [Source hashes](Packaging/source-manifest.json)
- [Previous 1.2.0 source](https://github.com/damimc21/KingdomUnlimitedWallet/tree/64329a4)

</details>

By RompeChopos. Credits: BepInEx, Harmony, Il2CppInterop and abevol.
