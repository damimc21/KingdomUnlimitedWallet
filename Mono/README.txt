UNLIMITED WALLET 1.3.2 - Mono

Requires matching BepInEx 6, installed separately. Install ONE edition only.
Windows / Proton with GameAssembly.dll: IL2CPP.
Native Linux / older Windows with Data/Managed/Assembly-CSharp.dll: Mono.
Runtime checks cover Windows IL2CPP 2.4.3, Windows Mono 2.1.3 and
native Linux Mono 2.4.2, including independent local-player protection.
No guarantee is made for every past/future build, DLC or other mod.

HUD: centred icons, a protection separator fitted to the visible digit height and slightly more row spacing.
F2: difficulty settings. Arrow keys / mouse: select and adjust. Esc: close.
Shields: optional finite protection, independent of accumulated money.
Resource loss: separate switch. Native damage deducts from counters and
drops resources using the original rules. At zero protection the crown drops.
Maximum: 60 points by default. Damage 5 consumes five points.
Recovery: collecting one eligible coin/gem/resource restores one point,
configurable from 1 to 20, capped at the maximum. Stored money never heals.
Retrieving a dropped crown restores one point. Special crown stealing remains.

Defaults preserve the old behavior: shields OFF, resource loss ON.
Both OFF: normal hits cause neither penalty; special crown stealing remains.
These new difficulty options apply offline and in local coop. Online uses
the original damage behavior; no custom shield is shown.

Config: BepInEx/config/com.codex.kingdom.unlimitedwallet.cfg
Shield state: unlimited-wallet-protection-v1.txt in the native save folder,
separate for each campaign/reign/player. Local file; cloud sync not guaranteed.
Back up saves before changing mods. Uninstall by removing this plugin only.
Source: https://github.com/damimc21/KingdomUnlimitedWallet
