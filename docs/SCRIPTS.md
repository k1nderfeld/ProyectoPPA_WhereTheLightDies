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
| `MenuPausa` | `Scripts/Menu` | Pausa el juego con Escape (detiene el tiempo y libera el cursor) y ofrece "Reanudar" y "Volver al menú". No se abre mientras el inventario está abierto. | `Canvas_Pausa` |
| `MenuInventario` | `Scripts/Menu` | Abre y cierra el inventario con **Tab** (RF-09), con estilo de maletín inspirado en Resident Evil 4: los 4 slots dentro del maletín, el protagonista a la derecha y abajo lo que hay en el slot elegido. El slot se elige con flechas, W, A, S, D o con clic. Mientras está abierto, el juego se detiene. No se abre si el juego está en pausa. | `Canvas_Inventario` |
| `Protagonista` | `Scripts/Jugador` | El personaje del jugador, según el diagrama de clases. Reúne el movimiento con W, A, S y D (con gravedad), correr con Shift, la **estamina** (se gasta al correr, se regenera tras 2 s y, si se agota, no deja correr hasta recuperar el 20 %) y la **salud** (`RecibirDanio`). Usa el `CharacterController`. `Empujar`, `RecogerMunicion`, `RecargarArma` y `EquiparArma` están pendientes (issue #10). | `Jugador` |
| `CamaraJugador` | `Scripts/Jugador` | Gira la vista con el mouse: izquierda y derecha giran el cuerpo, arriba y abajo giran la cámara (límite de ±85°). Usa la sensibilidad guardada en `Ajustes`. | `Jugador` |
| `Inventario` | `Scripts/Jugador` | El inventario del protagonista: 4 slots. Agrega munición (primero completa los slots del mismo tipo y después usa slots vacíos), la quita al recargar y la cuenta. En el Inspector se elige con cuánta munición parte el jugador. | `Jugador` |
| `Slot` | `Scripts/Jugador` | Un espacio del inventario: guarda un arma, munición de un solo tipo (hasta **4 cargadores** o **18 cartuchos**) o queda reservado cuando un arma ocupa varios espacios. No es un componente: el `Inventario` tiene los suyos. | Ningún objeto |

## Scripts de editor

Solo funcionan dentro del editor de Unity (no van en el juego compilado). Se ejecutan desde el menú **Goty Devs**.

| Script | Menú | Qué hace |
|---|---|---|
| `CrearMenuPrincipal` | Goty Devs > Crear menú principal | Arma la escena `Escenas/MenuPrincipal` completa (fondo, título, paneles, audio) y la agrega como primera escena de Build Settings. |
| `CrearEscenaConceptual` | Goty Devs > Crear escena conceptual del sótano | Arma la escena `Escenas/Sotano_Conceptual`: habitación, objetos, luces, ambiente, cámara con pistola, ojos voladores y HUD de prueba. |
| `CrearMenuPausa` | Goty Devs > Agregar menú de pausa a la escena abierta | Agrega el panel de pausa a la escena que esté abierta (no borra nada) y la agrega a Build Settings. Si ya tiene menú de pausa, no hace nada. |
| `CrearInventario` | Goty Devs > Agregar inventario a la escena abierta | Agrega el componente `Inventario` al Protagonista (y lo conecta) y la pantalla del inventario (`Canvas_Inventario`), con el maletín, la barra superior, el protagonista y la barra inferior. No borra nada más; si la pantalla ya existe, la reemplaza por una nueva. |

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
- `Sotano_Jugable`: copia de la anterior donde el jugador ya se mueve, mira, corre, puede pausar (Escape) y abrir el inventario (Tab). El botón **Jugar** del menú carga esta escena.

## Clases del diagrama y su estado

El diagrama de clases del equipo está en [`diagrama_clases.png`](diagrama_clases.png) (fuente editable: [`diagrama_clases.puml`](diagrama_clases.puml)). **El código debe respetarlo.**

| Clase del diagrama | Script | Estado |
|---|---|---|
| `Protagonista` | `Scripts/Jugador/Protagonista.cs` | Hecha: movimiento, correr, salud y estamina. Faltan `Empujar`, `RecogerMunicion`, `RecargarArma` y `EquiparArma` (#10) |
| `TipoMunicion` | `Scripts/Armas/TipoMunicion.cs` | Hecha |
| `Inventario` y `Slot` | `Scripts/Jugador/Inventario.cs` y `Slot.cs` | Hechas: 4 slots, capacidad fija y manejo de munición. Falta guardar armas (siguiente PR de la #10) |
| `Arma` y subclases (`CuerpoACuerpo`, `Granada`, `Pistola`, `Escopeta`) | `Scripts/Jugador/Arma.cs` | Esqueleto vacío; las subclases no existen (#10) |
| `Enemigo` y `EnemigoTerrestre` | `Scripts/Enemigos/EnemigoTerrestre.cs` | En progreso: solo tiene la visión por rayos; no hay clase `Enemigo` base (#11) |
| `EnemigoVolador` y `JefeFinal` | — | No existen (#11 y #13) |
| `GameManager` y `NivelManager` | — | No existen (#13) |
| Munición recogible (no está en el diagrama) | `Scripts/Recogibles/Municion.cs` | Esqueleto vacío |

`Inventario` y `Slot` agregan métodos de apoyo que no están en el diagrama (`AgregarMunicion`, `QuitarMunicion`, `ContarMunicion`, `GuardarArma`, `Reservar`, `Vaciar`); no cambian sus atributos ni sus relaciones.

## Pendientes por documentar

- Scripts de las armas y los enemigos (hoy son esqueletos).
