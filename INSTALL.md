# BepInEx setup

[Back to installation](README.md) · [Volver a la instalación](README.es.md)

BepInEx is a separate prerequisite. If a matching BepInEx 6 installation already
works with your other mods, continue with Unlimited Wallet's installation.

## Windows IL2CPP

1. Download [BepInEx 6 IL2CPP x64, build 753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753%2B0d275a4.zip).
2. Extract it into the game folder, beside `KingdomTwoCrowns.exe`.
3. For the tested Unity 6 game builds, also extract [Cpp2IL.Patch](https://github.com/abevol/KingdomMod/releases/download/2.4.0/Cpp2IL.Patch.zip)
   and [Il2CppInterop.Patch](https://github.com/abevol/KingdomMod/releases/download/2.4.3/Il2CppInterop.Patch.zip)
   there, replacing matching loader files. See [abevol's loader guide](https://github.com/abevol/KingdomMod#install).
4. Start the game once, let BepInEx finish its first-run setup, then close it.
   Install Unlimited Wallet using the main instructions.

## Windows Mono

Download [BepInEx 6 Mono x64, build 753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-win-x64-6.0.0-be.753%2B0d275a4.zip),
extract it into the game folder, start once and close the game. Then install
the **Mono** edition of Unlimited Wallet.

## Linux and Proton

**Native Linux:** extract [BepInEx 6 Mono Linux x64, build 753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-linux-x64-6.0.0-be.753%2B0d275a4.zip)
into the game folder, then install Unlimited Wallet **Mono**. In a terminal
opened in that folder, run:

```bash
chmod +x run_bepinex.sh launch_wallet_steam.sh
```

Set the game's Steam launch options to:

```text
./launch_wallet_steam.sh %command%
```

**Proton:** use the Windows **IL2CPP** loader/mod and set Steam launch options to:

```text
WINEDLLOVERRIDES=winhttp=n,b %command%
```

These instructions match the reference setup used for testing. See
[tested builds and limitations](REVIEW-1.3.2.md) for the compatibility scope.

## Resumen en español

Si ya tienes BepInEx 6 funcionando, instala el mod directamente.
En una instalación limpia, descarga el cargador de tu edición y extráelo en
la carpeta del juego. En Windows IL2CPP/Unity 6, aplica también los dos parches
enlazados arriba. Abre una vez el juego y ciérralo antes de instalar el mod.
Para Linux nativo o Proton, utiliza las opciones de lanzamiento indicadas arriba.
