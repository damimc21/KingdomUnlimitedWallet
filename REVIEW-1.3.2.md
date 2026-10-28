# Review notes — Unlimited Wallet 1.3.2

## Changes since 1.2.0

- Optional finite shield health; attacks consume their native damage amount.
- Independent resource-loss setting; eligible pickups restore configurable
  protection, capped at the configured maximum. Stored money does not heal.
- Protection saved separately per campaign/reign/player. Crown loss/retrieval
  and special crown stealing retain their documented behavior.
- F2 settings menu with mouse/keyboard controls and pause restoration.
- Centered resource icons, slightly increased row spacing and a shield fraction
  separator fitted to the visible digit height.
- One manual/Vortex download with an exclusive IL2CPP/Mono selector.

Defaults: shields off, resource loss on. Maximum protection 60, recovery 1 per
eligible pickup. New difficulty options are offline/local-co-op only; online
keeps native damage behavior. See the shipped READMEs for all settings.

## Runtime and file access

Harmony patches the game's player wallet, damage, pickup, campaign and input
methods. The HUD reuses sprites and font obtained from the running game.
Configuration and logging use BepInEx. There are no native P/Invoke imports,
network clients/downloaders, process-launch APIs or registry writes in the
four plugin source files.

`CrownProtection.cs` reads/writes `unlimited-wallet-protection-v1.txt` beside the
game's native saves, using a temporary file and replace/move for updates. The
state is keyed by campaign/reign/player and respects the game's save-enabled
flag. This sidecar must be included in backups; Steam Cloud sync is not guaranteed.

As in 1.2.0, `Plugin.cs` probes for `wallet-verify.flag` below BepInEx's plugin
directory. An opt-in diagnostic wallet routine is included in IL2CPP and is
inactive without that flag; Mono excludes the routine. Neither the flag nor
external/private test-helper DLLs are shipped.

The native Linux `launch_wallet_steam.sh` preserves Steam arguments and invokes
the separately installed `run_bepinex.sh`. It contains no downloader. The FOMOD
selects exactly one edition. `vortex_override_instructions.json` asks Vortex to
use its built-in `dinput` deployment type (game root); this metadata is consumed
by Vortex and is not deployed into the game.

## Completed runtime validation, 24 October 2026

| Environment | Initial session | Fresh process/reload |
|---|---:|---:|
| Windows IL2CPP, game 2.4.3, Unity 6000.0.66f2 | 37 PASS | 7 PASS |
| Native Linux Mono, game 2.4.2, Unity 6000.0.66f2 | 36 PASS | 7 PASS |
| Windows Mono, official historical game 2.1.3, Unity 2022.3.51f1 | 36 PASS | 7 PASS |

**130 assertions passed.** Private helpers prepared isolated state, then invoked
real game damage, pickup, payment, save and co-op activation methods. Checks
covered the four shield/resource-loss combinations, 60 damage units and crown
loss, multi-unit damage, coin/gem loss, recovery limits, grants/payments without
healing, crown retrieval, unchanged NPC capacity, hidden pouch and settings pause.

Native local co-op activation produced two active locally authoritative monarchs.
Damage, recovery, crown loss, state keys and HUD rows were independent. Fresh
processes restored protection exactly to P1 37/60 and P2 23/60, and coin balances
to 100123 and 200123. The game's native transfer of P2 gems to the host when
initially loading solo was verified and did not heal shields.

Normal starts without private helpers also loaded the public 1.3.2 plugin and
HUD. The Linux loader logged `Unable to start Unity log writer`; plugin startup,
HUD and functional checks still passed. This loader diagnostic is retained,
not presented as an error-free loader log.

The final ZIP was imported into an isolated Vortex 2.8.0 installation. Both
edition selections were installed/deployed/disabled in turn. Four stage checks
passed: one correct DLL, correct removal, launcher only for Mono, internal
metadata not deployed, and all 238 baseline game/loader files unchanged.
Both manual payloads, hashes and launcher permissions were separately checked.

## Limits

Tests do not cover every build/DLC, other mods, a full campaign, online play or
two physical controllers. Local co-op was activated through the native menu
function. Proton was tested with 1.2.0, not rerun for 1.3.2. Runtime/build tests
are functional evidence, not a replacement for Nexus's independent security
review. Historical 1.2.0 review data remains available in Git history.
