# Build and review Unlimited Wallet 1.3.2

## Reviewed download

[Nexus mod 45, Kingdom Two Crowns](https://www.nexusmods.com/kingdomtwocrowns/mods/45).
File name: **Unlimited-Wallet-1.3.2.zip** (38,087 bytes).

| File | SHA-256 |
|---|---|
| ZIP | `821ccacb799d78d9982d557b4bd28a7e1a49f38fc55d5a1c9b0512f2909f9322` |
| IL2CPP DLL | `8c4db7e2c596f7d94fd8ceaff2fa6b726c5ddb440c3d0250a3e2b4819af8c530` |
| Mono DLL | `557be92f9634a2e12951d93928d532061d05dee1413c742f0f791e618f6ca755` |

The [manifest](Packaging/release-manifest.json) lists all nine members, including
the executable permission on the Bash launcher. All seven non-DLL payload files
are committed verbatim under `Packaging/files`. No loader, game dependency,
nested archive or private test helper is bundled.

## Prerequisites

Use **.NET SDK 6.0.428** (selected by `global.json`), PowerShell and Python 3.
Supply game and matching BepInEx reference libraries locally; they are not in
this repository. The projects have no explicit NuGet PackageReference entries.
Restore may consult configured NuGet sources for SDK/framework references.

**IL2CPP, tested with Windows game 2.4.3:** install BepInEx 6 IL2CPP x64 build 753
and the compatible Cpp2IL/Il2CppInterop patches described in
[abevol's loader guide](https://github.com/abevol/KingdomMod#install).
Start once to generate `BepInEx/interop`, then close the game. The build needs
both `BepInEx/core` and those generated interop assemblies.

**Mono:** use original managed assemblies from the native Linux 2.4.2 game Data
directory (`Managed/Assembly-CSharp.dll`, Unity modules, etc.) plus BepInEx 6
Mono build 753 core libraries. These match the reference set used for the
shipped Mono build. That same binary was also tested on Windows game 2.1.3.
IL2CPP interop assemblies cannot replace Mono managed assemblies.

## Compile

From this repository in PowerShell, replacing the example paths:

```powershell
./build-il2cpp.ps1 -GamePath 'D:/IL2CPPGame/Kingdom Two Crowns'
./build-mono.ps1 -GameDataPath 'D:/MonoGame/KingdomTwoCrowns_Data' -BepInExCorePath 'D:/BepInExMono753/BepInEx/core'
```

Outputs:

- `IL2CPP/bin/Release/net6.0/KingdomUnlimitedWallet.dll`
- `Mono/bin/Release/netstandard2.1/KingdomUnlimitedWallet.dll`

Both projects compile the four shared source files listed in
[source-manifest.json](Packaging/source-manifest.json); Mono defines `MONO`.
The build scripts only validate references and run `dotnet build -c Release`.
They do not launch the game, install the plugin or copy dependency DLLs to output.

## Build the ZIP

```powershell
python ./build-package.py --il2cpp-dll ./IL2CPP/bin/Release/net6.0/KingdomUnlimitedWallet.dll --mono-dll ./Mono/bin/Release/netstandard2.1/KingdomUnlimitedWallet.dll --output ./Unlimited-Wallet-1.3.2.zip
```

The builder combines the supplied DLLs with the seven exact committed payload
files. It uses the release's entry order, compression, timestamps and modes,
then reports each member's hash and whether the archive matches the release.
Non-DLL payload mismatches are rejected.

Supplying the original two DLLs extracted from the Nexus download reproduces
the release ZIP byte for byte. Fresh compilation in another directory or with
different reference assemblies can change build/PDB metadata and DLL hashes;
byte-identical fresh compilation is not claimed. The builder reports such
differences rather than hiding them. It never changes the Nexus download.

See [REVIEW-1.3.2.md](REVIEW-1.3.2.md) for runtime behavior, file access and tests.
