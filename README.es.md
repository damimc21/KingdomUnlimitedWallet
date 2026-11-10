# Unlimited Wallet

Contadores de monedas y gemas con los iconos del juego, sin el límite habitual
de la bolsa. Escudos opcionales para mantener el peligro de los enemigos.

## Descargar

**[Descargar Unlimited Wallet 1.3.2.zip](https://github.com/damimc21/KingdomUnlimitedWallet/releases/download/v1.3.2/Unlimited-Wallet-1.3.2.zip)**

[English](README.md) · [Todas las versiones](https://github.com/damimc21/KingdomUnlimitedWallet/releases)

Un único ZIP incluye IL2CPP y Mono. Descarga el archivo de arriba: los archivos
**Source code** de GitHub contienen el código, no el mod listo para instalar.

## Antes de instalar

Cierra el juego. Necesitas **BepInEx 6 compatible con tu edición**, instalado
aparte: [preparación del cargador](INSTALL.md).
Desactiva o retira la instalación anterior de Unlimited Wallet antes de actualizar.

| En la carpeta del juego aparece | Elige |
|---|---|
| `GameAssembly.dll` (Windows / Proton) | **IL2CPP** |
| `KingdomTwoCrowns_Data/Managed/Assembly-CSharp.dll` (Windows antiguo / Linux nativo) | **Mono** |

Abre la carpeta desde Steam: **Propiedades → Archivos instalados → Explorar**.
Instala solo una edición.

## Con Vortex

1. Selecciona **Kingdom Two Crowns**. Si Vortex lo solicita, instala el soporte del juego desde su catálogo.
2. En **Mods → Install From File**, selecciona el ZIP y elige **IL2CPP** o **Mono**.
3. Activa el mod, pulsa **Deploy Mods** y abre el juego desde Steam.

No necesitas otro ZIP de soporte de Unlimited Wallet.

## Manual

1. Extrae el ZIP fuera de la carpeta del juego.
2. Copia el **contenido** de `Manual/IL2CPP` o `Manual/Mono` en la carpeta del
   juego, junto a su ejecutable. Combina la carpeta `BepInEx` cuando lo pida.
3. Abre el juego desde Steam. En Linux/Proton, consulta las [opciones de lanzamiento](INSTALL.md#linux-and-proton).

La DLL debe quedar en `BepInEx/plugins/KingdomUnlimitedWallet/KingdomUnlimitedWallet.dll`.

## Uso

Pulsa **F2** para configurar escudos y pérdida de recursos. Los escudos se
recuperan recogiendo recursos y están desactivados por defecto. Las opciones
nuevas de dificultad funcionan en solitario y cooperativo local; online conserva
el daño original.

[Código, compilación y hashes](BUILD.md) · [Pruebas y compatibilidad](REVIEW-1.3.2.md)
