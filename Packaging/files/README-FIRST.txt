UNLIMITED WALLET 1.3.2

ES
BepInEx es un requisito aparte y NO esta incluido.
Manual: extrae este ZIP fuera del juego y copia el CONTENIDO de UNA carpeta
Manual/IL2CPP o Manual/Mono en la raiz del juego. No copies ambas ediciones.
Vortex: importa este ZIP, elige una edicion y despliega. Desactiva la version
anterior antes de activar esta. No requiere nuestro antiguo ZIP de soporte.
Windows actual / Proton: IL2CPP. Windows antiguo / Linux nativo: Mono.

F2 abre la configuracion: escudos, perdida de recursos, resistencia maxima
y recuperacion por recurso. Los escudos son opcionales y se recuperan
recogiendo monedas/gemas, sin bolsa. El dano conserva su proporcion original.
Las dos opciones se activan por separado. Por defecto: escudos NO y perdida SI.
Sin ambas penalizaciones, los golpes normales no penalizan. El robo especial
de corona sigue funcionando. Al llegar a cero escudos, cae la corona.
Opciones nuevas: un jugador y cooperativo local; online conserva el original.
Consulta el README de tu edicion para el guardado y la compatibilidad comprobada.

EN
Install matching BepInEx separately. For manual installation, copy the contents
of ONE Manual/<edition> folder into the game root. Vortex uses the included
edition selector. Disable the previous wallet mod first. Do not combine DLLs.
Press F2 for optional shield health and independent resource loss. Recover
protection by collecting resources. Default maximum 60, recovery 1 per item.
Defaults preserve legacy difficulty. The new difficulty options are offline
and local-coop only; online retains native behavior. See the edition README.

Linux: chmod +x run_bepinex.sh launch_wallet_steam.sh
Steam Linux launch option: ./launch_wallet_steam.sh %command%
Proton launch option: WINEDLLOVERRIDES=winhttp=n,b %command%
