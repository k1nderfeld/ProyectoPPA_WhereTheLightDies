# Proyecto PPA

Videojuego de horror psicológico en primera persona, ambientado en un sótano cuyos niveles se vuelven cada vez más grotescos a medida que el jugador desciende. El jugador usa armas con munición limitada, gestiona su estamina y su inventario, y enfrenta enemigos y un jefe final. No hay HUD: el estado del personaje se comunica con efectos visuales y sonoros.

Proyecto de la asignatura **Programación Avanzada**, Universidad Católica del Norte (2026).

- **Nombre del juego:** por definir
- **Versión actual:** `v0.1.0` (primera iteración)
- **Público objetivo:** 16 años o más, en PC con Windows

## Equipo Goty Devs

| Integrante | Rol | GitHub |
|---|---|---|
| Baltazar Rodríguez | Líder | [@k1nderfeld](https://github.com/k1nderfeld) |
| Pablo Velásquez | Programador líder | [@pablovm01](https://github.com/pablovm01) |
| Geancarlo Bisquett | Programador asistente y control de versiones | [@Geannn27](https://github.com/Geannn27) |
| Luis Leyton | Diseñador de arte y sonido | [@Over-hub](https://github.com/Over-hub) |

## Tecnologías

- **Motor:** Unity `6000.6.0f1` (Unity 6)
- **Lenguaje:** C#
- **Render pipeline:** Universal Render Pipeline (URP) 17.6.0
- **Paquetes principales:** Input System 1.20.0, AI Navigation 2.0.14, Timeline 6.6.0, UI (uGUI) 2.6.0

## Cómo abrir el proyecto

1. Instala [Git](https://git-scm.com) y [Unity Hub](https://unity.com/download).
2. Desde Unity Hub, en **Installs → Install Editor**, instala la versión **6000.6.0f1** (si no aparece en la lista, usa la pestaña *Archive* y el enlace *Download Archive*). Es importante usar exactamente esa versión.
3. Clona el repositorio:
   ```bash
   git clone https://github.com/k1nderfeld/proyecto_PPA.git
   ```
   El repositorio es privado: hay que iniciar sesión con una cuenta que sea colaboradora. También se puede clonar con GitHub Desktop.
4. En Unity Hub pulsa **Add → Add project from disk** y selecciona la carpeta del repositorio.
5. Abre el proyecto con el editor `6000.6.0f1`. La primera vez Unity genera la carpeta `Library`, que está ignorada por Git y puede tardar varios minutos.
6. Abre la escena `Assets/Escenas/MenuPrincipal.unity` y pulsa **Play**. El botón *Jugar* carga la escena conceptual del sótano (`Assets/Escenas/Sotano_Conceptual.unity`).

## Estructura del repositorio

```
Assets/
├── Arte/             Concepto, Interfaz y Materiales
├── Audio/            Musica y Efectos
├── Configuracion/    Ajustes de render (URP) y perfiles de efectos
├── Editor/           Scripts del editor (menú "Goty Devs")
├── Escenas/          Escenas del juego
├── Scripts/          Código del juego (Menu, Sistemas, ...)
└── Terceros/         Assets descargados (no se modifican)
docs/                 Documentación del equipo
Packages/             Paquetes de Unity (manifest.json)
ProjectSettings/      Configuración del proyecto Unity
README.md             Este archivo
.gitignore            Archivos ignorados (plantilla de Unity)
```

El detalle de cada carpeta, las reglas de nombres y cómo documentar el código están en [`docs/ESTRUCTURA_Y_CONVENCIONES.md`](docs/ESTRUCTURA_Y_CONVENCIONES.md).

## Flujo de trabajo

### Ramas
- `main`: versiones estables del proyecto. Solo recibe cambios desde `Develop`.
- `Develop`: integración de los avances del equipo.
- `Baltazar`, `Pablo`, `Gean` y `Luis`: ramas personales de cada integrante.

Flujo:

1. Cada integrante trabaja en su rama personal y trae los cambios de `Develop` con `git merge origin/Develop`.
2. Los avances se integran a `Develop` mediante pull request, revisado por al menos otro integrante. Al fusionar se usa **Create a merge commit** (no *Squash*), para conservar los commits de cada integrante.
3. `Develop` se integra a `main` mediante un pull request por cada versión (por ejemplo, `release: v0.2.0`), y luego se crea el tag de esa versión sobre `main`.

### Mensajes de commit
Se usa la convención de Angular: `tipo(ámbito): descripción`. La descripción va en imperativo, con la primera letra en minúscula y sin punto final. El ámbito es opcional.

| Tipo | Se usa cuando |
|---|---|
| `feat` | se añade una funcionalidad nueva |
| `fix` | se corrige un error |
| `docs` | solo cambia la documentación |
| `style` | cambia el formato del código sin afectar su funcionamiento |
| `refactor` | se mejora el código sin corregir errores ni añadir funcionalidades |
| `perf` | se mejora el rendimiento |
| `test` | se añaden o corrigen pruebas |
| `build` | cambia el compilado del proyecto |
| `ci` | cambian la configuración o los scripts de integración continua |
| `chore` | tareas rutinarias (por ejemplo, editar `.gitignore`) |
| `revert` | se revierte un commit anterior |

Ejemplos: `feat(inventario): añade inventario básico`, `docs(readme): añade pasos para abrir el proyecto`.

### Planificación
Las tareas se gestionan como **issues** de este repositorio, agrupadas en **milestones** que corresponden a las etapas del ciclo de desarrollo:

1. Requisitos
2. Análisis y diseño
3. Desarrollo
4. Validación y verificación
5. Producción
6. Mantención

El seguimiento se hace en un tablero Kanban y una Carta Gantt (vista Roadmap) de GitHub Projects (enlace por definir).

## Versionado semántico

Las versiones siguen el formato `MAJOR.MINOR.PATCH` (por ejemplo, `0.1.0`):

- **MAJOR:** cambio muy grande en el software, con funcionalidades añadidas o eliminadas en bloque; la versión anterior puede ser incompatible con la nueva.
- **MINOR:** se añade una nueva funcionalidad, manteniendo la compatibilidad con la versión anterior.
- **PATCH:** se corrigen fallas (también cambios estéticos), manteniendo la compatibilidad.

Mientras el `MAJOR` sea `0` el juego está en desarrollo inicial. `v0.1.0` corresponde a la primera iteración (proyecto Unity base); la versión `v1.0.0` será el juego completo.

## Documentación

- Estructura del proyecto y convenciones: [`docs/ESTRUCTURA_Y_CONVENCIONES.md`](docs/ESTRUCTURA_Y_CONVENCIONES.md)
- Guía de scripts: [`docs/SCRIPTS.md`](docs/SCRIPTS.md)
- Especificación de Requerimientos de Software (ERS): por definir
- Tablero Kanban y Carta Gantt: por definir

## Créditos de assets

Los recursos de terceros que se integren al proyecto se registrarán en `Assets/CREDITOS.md`, con su autor, fuente y licencia (por definir).
