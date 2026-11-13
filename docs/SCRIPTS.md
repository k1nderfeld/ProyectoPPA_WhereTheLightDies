# Guía de scripts

Lista de todos los scripts del proyecto, para qué sirve cada uno y dónde se usa. **Se actualiza cada vez que se crea o cambia un script** (ver reglas en [`ESTRUCTURA_Y_CONVENCIONES.md`](ESTRUCTURA_Y_CONVENCIONES.md)).

## Scripts del juego

| Script | Carpeta | Qué hace | Se agrega a |
|---|---|---|---|
| `Ajustes` | `Scripts/Sistemas` | Guarda y aplica las opciones del jugador (volúmenes, pantalla completa, sensibilidad) con `PlayerPrefs`. Es estático: se usa como `Ajustes.VolumenGeneral`. | Ningún objeto |
| `MenuPrincipal` | `Scripts/Menu` | Maneja los botones del menú (Jugar, Opciones, Créditos, Salir), cambia entre paneles, vuelve con Escape y carga la escena del juego con un fundido a negro. | Objeto `Menu` |
| `MenuOpciones` | `Scripts/Menu` | Controla los sliders y la casilla del panel de opciones. Cada cambio se guarda en `Ajustes` y se aplica al instante. | `PanelOpciones` |
| `GestorAudioMenu` | `Scripts/Menu` | Reproduce la música de ambiente (en bucle) y los sonidos de hover y clic. Aplica los volúmenes guardados. | Objeto `Audio` |
| `SonidoBoton` | `Scripts/Menu` | Hace sonar un botón al pasar el mouse y al hacer clic. | Cada botón |
| `TextoParpadeo` | `Scripts/Menu` | Hace parpadear un texto como una luz que falla (título). | `Titulo` |
| `MenuPausa` | `Scripts/Menu` | Pausa el juego con Escape (detiene el tiempo y libera el cursor) y ofrece "Reanudar" y "Volver al menú". | `Canvas_Pausa` |
| `MovimientoJugador` | `Scripts/Jugador` | Mueve al jugador con W, A, S y D, con gravedad. Corre con Shift si la estamina lo permite. Usa el `CharacterController`. | `Jugador` |
| `CamaraJugador` | `Scripts/Jugador` | Gira la vista con el mouse: izquierda y derecha giran el cuerpo, arriba y abajo giran la cámara (límite de ±85°). Usa la sensibilidad guardada en `Ajustes`. | `Jugador` |
| `Estamina` | `Scripts/Jugador` | Lleva la estamina: se gasta al correr, se regenera tras 2 s sin correr y, si se agota, no deja correr hasta recuperar el 20 %. | `Jugador` |

## Scripts de editor

Solo funcionan dentro del editor de Unity (no van en el juego compilado). Se ejecutan desde el menú **Goty Devs**.

| Script | Menú | Qué hace |
|---|---|---|
| `CrearMenuPrincipal` | Goty Devs > Crear menú principal | Arma la escena `Escenas/MenuPrincipal` completa (fondo, título, paneles, audio) y la agrega como primera escena de Build Settings. |
| `CrearEscenaConceptual` | Goty Devs > Crear escena conceptual del sótano | Arma la escena `Escenas/Sotano_Conceptual`: habitación, objetos, luces, ambiente, cámara con pistola, ojos voladores y HUD de prueba. |
| `CrearMenuPausa` | Goty Devs > Agregar menú de pausa a la escena abierta | Agrega el panel de pausa a la escena que esté abierta (no borra nada) y la agrega a Build Settings. Si ya tiene menú de pausa, no hace nada. |

> Si se vuelve a ejecutar un script de editor, **reemplaza** la escena que genera. No usarlo sobre una escena a la que ya le hicimos cambios a mano.

## Cómo funciona el menú

```
MenuPrincipal.unity
├── Camara
├── EventSystem            (permite hacer clic en los botones; usa el Input System nuevo)
├── Canvas
│   ├── Fondo, Oscurecer, Degradado, Titulo, Version
│   ├── PanelPrincipal     (4 botones)
│   ├── PanelOpciones      (lleva MenuOpciones)
│   ├── PanelCreditos
│   └── Fundido            (pantalla negra para entrar y salir)
├── Audio                  (lleva GestorAudioMenu)
└── Menu                   (lleva MenuPrincipal)
```

1. Al abrir, `MenuPrincipal` muestra el cursor, deja visible solo `PanelPrincipal` y hace el fundido de negro a normal.
2. `GestorAudioMenu` aplica los volúmenes guardados y empieza la música.
3. Al pulsar **Jugar**, la pantalla se oscurece y se carga `Sotano_Jugable`.
4. Las opciones se guardan en `PlayerPrefs` a través de `Ajustes`; el resto del juego las lee desde ahí.

## Escenas de juego

- `Sotano_Conceptual`: escena generada por script, solo para mostrar el ambiente.
- `Sotano_Jugable`: copia de la anterior donde el jugador ya se mueve, mira, corre y puede pausar. El botón **Jugar** del menú carga esta escena.

## Pendientes por documentar

- Scripts de armas y enemigos (aún no existen).
