UNLIMITED WALLET 1.2.0 - IL2CPP
Kingdom Two Crowns | BepInEx 6 Unity.IL2CPP | Windows x64 / Linux via Proton

ENGLISH

Separate coin/gem counters with native icons and the game's Kingdom font.
Removes normal player purse capacity and physical overflow. Collect and spend
normally; this mod does not grant resources or change the game's save format.
Other held wallet resources can appear in additional rows.

1.2.0 gives the native font enough vertical layout space to keep complete
digits visible, including at small window resolutions, preserving its scale.
Gem icon alignment from 1.1.1 is retained. The unified ZIP also contains a Mono edition.

INSTALLATION
1. Close the game and back up saves.
2. Extract BepInEx Unity.IL2CPP Windows x64 build 753 beside the game's EXE:
   https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753%2B0d275a4.zip
3. Apply both loader compatibility patches to the game root:
   https://github.com/abevol/KingdomMod/releases/download/2.4.0/Cpp2IL.Patch.zip
   https://github.com/abevol/KingdomMod/releases/download/2.4.3/Il2CppInterop.Patch.zip
4. Start once to generate BepInEx interop files, then exit. First start is slow.
5. Copy the CONTENTS of Manual/IL2CPP into the game root:
   BepInEx/plugins/KingdomUnlimitedWallet/KingdomUnlimitedWallet.dll
6. Start through Steam. Install only one wallet DLL and the matching loader.

PROTON
Use the Windows game's IL2CPP edition and Windows loader above. In Steam,
force the chosen Proton compatibility tool for this game. To load BepInEx,
set this game's launch options to:
   WINEDLLOVERRIDES=winhttp=n,b %command%
Do not use the native Linux Mono launcher for a Windows game under Proton.
https://docs.bepinex.dev/articles/advanced/proton_wine.html

VORTEX (WINDOWS)
Manage Kingdom Two Crowns in Vortex; its catalogue supplies game support.
Import the unified ZIP, choose IL2CPP, enable and deploy. No support ZIP
from this mod page is needed. Install the loader and both patches separately.
Disable previous wallet installations before enabling this package.

CONFIGURATION
BepInEx/config/com.codex.kingdom.unlimitedwallet.cfg
[Pantalla]
TamanoTexto = 24  (number size, 14-48)
OtrosRecursos = true  (show other held wallet resources)
Thousands use the Spanish separator: 100.123.

COMPATIBILITY
Game runtime must be IL2CPP (original *_Data/il2cpp_data). A game with original
*_Data/Managed/Assembly-CSharp.dll uses Mono: choose Manual/Mono from the unified ZIP.
BepInEx-generated interop DLLs do not indicate a Mono game. BepInEx 5,
consoles and macOS are not supported by this archive. Multiplayer, every
DLC/biome, travel and death have not been separately verified.
The game's signed 32-bit integer resource amounts still apply; "unlimited"
means removal of the normal purse limit, not mathematically infinite storage.

REMOVAL / TROUBLESHOOTING
Close the game before updating. Remove the plugin folder manually, or disable
and deploy in Vortex. The normal purse limit returns; spend excess resources
first or use a pre-mod save. If the HUD is absent, inspect BepInEx/LogOutput.log.
Report OS, game and loader versions and relevant error lines without personal
information. Avoid mods which replace the purse/HUD or the same wallet code.

CREDITS
BepInEx, Harmony, Il2CppInterop contributors; abevol for loader patches.
Sprites and font are read from the installed game; no game assets, assemblies,
loader files or private test helpers are distributed. Created with assistance
from OpenAI Codex.

ESPANOL

Version IL2CPP para Windows x64 y el juego Windows bajo Proton. Incluye
contadores con iconos nativos y fuente Kingdom. La 1.2.0 adapta el tamano de
la zona vertical del texto para evitar cifras recortadas, manteniendo la alineacion
de gemas. No genera recursos gratis ni cambia el formato del guardado.

Instala BepInEx IL2CPP x64 753 y AMBOS parches enlazados arriba junto al EXE.
Arranca una vez para generar interop y cierra. Copia el CONTENIDO de Manual/IL2CPP del ZIP en la
carpeta del juego conservando las carpetas. Instala una sola DLL de cartera.
Para Proton, elige el cargador Windows y configura Steam con:
   WINEDLLOVERRIDES=winhttp=n,b %command%
Para Linux nativo Mono utiliza Manual/Mono y sus instrucciones propias.
Haz copia de tus partidas. Al quitar el mod vuelve el limite original.
Vortex obtiene el soporte del juego de su catalogo; importa el ZIP unificado;
el cargador y sus parches se instalan por separado. La instalacion manual
funciona sin Vortex. Consulta los resultados exactos publicados en Nexus.
