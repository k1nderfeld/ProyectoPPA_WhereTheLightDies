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
| `Protagonista` | `Scripts/Jugador` | El personaje del jugador, según el diagrama de clases. Movimiento con W, A, S y D (con gravedad), correr con Shift, **estamina** (se gasta al correr, se regenera tras 2 s y, si se agota, no deja correr hasta recuperar el 20 %), **salud** (`RecibirDanio`), recoger con **E** a 2 m o menos (RF-10), atacar con **clic izquierdo**, recargar con **R** (RF-15) y equipar armas del inventario (`EquiparArma`, RF-11). `Empujar` sigue pendiente. | `Jugador` |
| `CamaraJugador` | `Scripts/Jugador` | Gira la vista con el mouse: izquierda y derecha giran el cuerpo, arriba y abajo giran la cámara (límite de ±85°). Usa la sensibilidad guardada en `Ajustes`. | `Jugador` |
| `Inventario` | `Scripts/Jugador` | El inventario del protagonista: 4 slots. Agrega munición (primero completa los slots del mismo tipo y después usa slots vacíos), la quita al recargar y la cuenta. En el Inspector se elige con cuánta munición parte el jugador. | `Jugador` |
| `Slot` | `Scripts/Jugador` | Un espacio del inventario: guarda un arma, munición de un solo tipo (hasta **4 cargadores** o **18 cartuchos**) o queda reservado cuando un arma ocupa varios espacios. No es un componente: el `Inventario` tiene los suyos. | Ningún objeto |
| `Arma` | `Scripts/Armas` | Clase **abstracta** base de todas las armas (diagrama de clases). Tiene los datos del diagrama (daño por rayo, rango, rayos por ataque, cadencia, tiempo de recarga, munición por carga y actual, slots que ocupa y tipo de munición) y los métodos `usar()` (lanza rayos desde el centro de la pantalla y, si pegan a un enemigo, le hacen daño) y `sePuedeUsar()`. También se recarga, se recoge del suelo y se muestra en la mano. | Cada arma |
| `Pistola` | `Scripts/Armas` | Hereda de `Arma`. Un disparo de 20 de daño, cargador de 6 balas, ocupa 1 slot (RF-13). | Objeto `Pistola` |
| `Escopeta` | `Scripts/Armas` | Hereda de `Arma`. 8 perdigones con dispersión, 6 cartuchos que se recargan de a uno, ocupa **2 slots** (RF-14). | Objeto `Escopeta` |
| `CuerpoACuerpo` | `Scripts/Armas` | Hereda de `Arma`. La palanca: golpe de 25 a 2 m o menos, sin munición (RF-12). | Objeto `Palanca` |
| `TipoMunicion` | `Scripts/Armas` | Tipos de munición del diagrama: `Cargador` y `Cartucho`. | Ningún objeto |
| `IRecogible` | `Scripts/Recogibles` | **Interfaz** de lo que se recoge con E: un texto para el aviso y el método `Recoger`. La implementan `Arma` y `Municion`. | Ningún objeto |
| `Municion` | `Scripts/Recogibles` | Caja de cargadores o cartuchos en el suelo. Al recogerla guarda una unidad por vez con `Protagonista.RecogerMunicion`; lo que no cabe se queda en el suelo. | Cada caja de munición |
| `AvisoRecoger` | `Scripts/Menu` | Muestra abajo "E  Recoger…" cuando hay algo cerca, o mensajes cortos como "No hay espacio en el inventario". No muestra salud, munición ni estamina (RF-08). | `Canvas_Avisos` |

## Comandos del menú Goty Devs

Para armar escenas usamos comandos en el menú **Goty Devs** de Unity (scripts en `Assets/Editor/`). **No se suben al repositorio**: la carpeta `Assets/Editor/` está en el `.gitignore` y cada uno los tiene solo en su computador. Lo que sí se sube es la escena ya armada.

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
| `Protagonista` | `Scripts/Jugador/Protagonista.cs` | Hecha: movimiento, correr, salud, estamina, recoger, atacar, recargar y equipar. Falta `Empujar` (RF-05) |
| `TipoMunicion` | `Scripts/Armas/TipoMunicion.cs` | Hecha |
| `Inventario` y `Slot` | `Scripts/Jugador/Inventario.cs` y `Slot.cs` | Hechas: 4 slots, capacidad fija, munición y armas (un arma grande reserva slots) |
| `Arma` y subclases | `Scripts/Armas/` | Hechas `Arma`, `Pistola`, `Escopeta` y `CuerpoACuerpo`. Falta `Granada` (RF-16) |
| `Enemigo` y `EnemigoTerrestre` | `Scripts/Enemigos/EnemigoTerrestre.cs` | En progreso: solo tiene la visión por rayos; no hay clase `Enemigo` base (#11) |
| `EnemigoVolador` y `JefeFinal` | — | No existen (#11 y #13) |
| `GameManager` y `NivelManager` | — | No existen (#13) |
| Munición recogible (no está en el diagrama) | `Scripts/Recogibles/Municion.cs` | Hecha (se recoge con E) |

`Inventario` y `Slot` agregan métodos de apoyo que no están en el diagrama (`AgregarMunicion`, `QuitarMunicion`, `ContarMunicion`, `GuardarArma`, `Reservar`, `Vaciar`); no cambian sus atributos ni sus relaciones.

`Arma` respeta los nombres del diagrama, incluidos `usar()` y `sePuedeUsar()` en minúscula. Además agrega lo necesario para Unity que no está en el diagrama: la posición en la mano, los sonidos, `Recargar`, `Recoger` y `Mostrar`. `Protagonista` agrega `mano`, `distanciaRecoger`, el aviso y la búsqueda de lo que se puede recoger.

## Pendientes por documentar

- La granada y los enemigos (hoy el enemigo es un esqueleto).
