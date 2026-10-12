UNLIMITED WALLET 1.2.0 - MONO EXPERIMENTAL
Kingdom Two Crowns | BepInEx 6 Unity.Mono | Windows x64 / native Linux x64

ENGLISH

This is the Mono edition of Unlimited Wallet. It keeps the native coin and
gem icons, the game's pixel font, aligned amounts and additional wallet rows.
Text layout accounts for the native font's line height to avoid cropped digits.
It removes the normal player purse limit and physical overflow. It does not
grant free resources or change the game's save format.

STATUS: EXPERIMENTAL. Tested on native Steam Linux game 2.4.2, Ubuntu 24.04.5
LTS, Unity 6000.0.66f2 and BepInEx Mono 753 in VirtualBox on 11 October 2026.
Native wallet pickup/payment at 100000, separate counters, NPC capacity and
disk save/restart passed. Reload restored exactly 100123 coins / 4567 gems.
Windows Mono game 2.1.3 (official Steam historical build), Unity 2022.3.51f1,
Windows 11 and BepInEx Mono 753 passed the same native transactions and disk
save/restart using this exact DLL. The normal HUD was checked again with the
test helper removed. Other game builds may have different APIs.
The DLL requires a game runtime supporting .NET Standard 2.1.

WHICH DOWNLOAD?
- Mono: your original game has *_Data/Managed/Assembly-CSharp.dll. Choose Manual/Mono from the unified ZIP.
- IL2CPP: your game has *_Data/il2cpp_data. Choose Manual/IL2CPP from the unified ZIP.
- Linux running the Windows game through Proton: choose according to that
  game's runtime. Proton does not turn an IL2CPP game into a Mono game.
Install ONLY ONE wallet DLL. BepInEx/interop generated DLLs are not evidence
that an IL2CPP installation is Mono.

WINDOWS MONO
1. Close the game and back up your save.
2. Install BepInEx 6 Unity.Mono-win-x64. Build 753 is the baseline used to
   compile this release (newer compatible Mono builds may also work):
   https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-win-x64-6.0.0-be.753%2B0d275a4.zip
3. Extract the loader beside the game EXE and start the game once, then exit.
4. Copy the CONTENTS of Manual/Mono from the unified ZIP into the game root.
   BepInEx/plugins/KingdomUnlimitedWallet/KingdomUnlimitedWallet.dll
5. Start the game normally. Check BepInEx/LogOutput.log if the HUD is absent.

NATIVE LINUX MONO
1. Close the game and back up your save.
2. Install BepInEx 6 Unity.Mono-linux-x64 in the native game directory:
   https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-linux-x64-6.0.0-be.753%2B0d275a4.zip
3. Copy the CONTENTS of Manual/Mono into the game directory, preserving case.
   BepInEx/plugins/KingdomUnlimitedWallet/KingdomUnlimitedWallet.dll
   launch_wallet_steam.sh goes beside KingdomTwoCrowns.x86.
4. In a terminal opened in the game directory, run:
   chmod u+x run_bepinex.sh launch_wallet_steam.sh
5. For Steam, set this game's launch options to:
   ./launch_wallet_steam.sh %command%
   This launcher preserves Steam's runtime and paths containing spaces. The
   baseline loader's ./run_bepinex.sh %command% failed with the current Steam
   runtime on our test system. The included launcher is for the native game.
6. Start through Steam. A .dll is correct on Linux too: this
   is managed game code; the OS-specific native loader comes from BepInEx.

Loader documentation:
https://docs.bepinex.dev/master/articles/user_guide/installation/unity_mono.html
https://docs.bepinex.dev/articles/advanced/steam_interop.html
Loader builds: https://builds.bepinex.dev/projects/bepinex_be
This ZIP does not include BepInEx, game assemblies or game assets. The IL2CPP
Cpp2IL/Il2CppInterop patches are not required by this Mono plugin. BepInEx 5
uses a different plugin API and is not supported by this build.
Vortex is for Windows. Import the unified ZIP and choose Mono. Its catalogue
supplies game support; no extra ZIP from this mod page is needed. Use the
manual instructions above for native Linux.

CONFIGURATION
BepInEx/config/com.codex.kingdom.unlimitedwallet.cfg
[Pantalla]
TamanoTexto = 24       (allowed range 14-48)
OtrosRecursos = true   (show additional held wallet resources)
Thousands use the Spanish separator, for example 1.000.

TESTING / REPORTING
Confirm pickup beyond the normal purse limit, spending, gem alignment, save
and reload. Online multiplayer, split-screen, and every DLC/biome remain
unverified. If reporting a problem, provide OS, game version, BepInEx version
and relevant error lines from LogOutput.log (remove personal information).
The normal purse limit returns if you disable the mod; spend excess resources
first or use a pre-mod save. The game still uses signed 32-bit integer amounts.

CREDITS
BepInEx and Harmony contributors. abevol for the game reference assemblies
and Mono installation reference. Sprites/fonts are read from the installed
game at runtime. Created with assistance from OpenAI Codex.

ESPANOL

Version Mono experimental para Windows x64 y Linux nativo x64. Probada en
Steam Linux nativo 2.4.2, Ubuntu 24.04.5 LTS, Unity 6000.0.66f2 y BepInEx
Mono 753: recogida/gasto nativos con 100000 unidades y guardado/reinicio con
100123 monedas y 4567 gemas superados. Tambien probada en Windows 11 con la
edicion Mono oficial Steam 2.1.3, Unity 2022.3.51f1 y BepInEx Mono 753:
recogida/gasto nativos y guardado/reinicio superados con la misma DLL.
Las ediciones antiguas pueden tener una API diferente.

Necesita BepInEx 6 Unity.Mono para tu sistema y .NET Standard 2.1 en el juego.
En Windows, instala el cargador junto al EXE. En Linux nativo, da permiso de
ejecucion a run_bepinex.sh y al launch_wallet_steam.sh incluido en este ZIP.
Configura Steam con ./launch_wallet_steam.sh %command%.
Copia el CONTENIDO de Manual/Mono del ZIP en la carpeta del juego. Instala una sola DLL de la
cartera. No instales esta variante en IL2CPP ni con BepInEx 5.

El juego Windows bajo Proton conserva su runtime original; elige la descarga
Mono o IL2CPP segun ese juego. Vortex es para Windows;
en Linux utiliza la instalacion manual. Haz copia de tus partidas antes de
probar. Para comunicar errores, indica version del juego/cargador y sistema.
