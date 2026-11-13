# Estructura del proyecto y convenciones

Todo el proyecto está en **español** para que el equipo lo entienda fácilmente. Este documento dice dónde va cada cosa y cómo se nombra. Si algo no está claro, se agrega aquí.

## 1. Carpetas

```
Assets/
├── Arte/                 Todo lo visual que hacemos nosotros
│   ├── Concepto/         Dibujos de referencia (POV, protagonista, enemigos)
│   ├── Interfaz/         Imágenes de menús y pantallas (fondos, degradados, íconos)
│   └── Materiales/       Materiales (.mat) de las figuras de las escenas
├── Audio/
│   ├── Musica/           Música y sonidos ambientales largos
│   └── Efectos/          Sonidos cortos (clics, disparos, pasos, golpes)
├── Configuracion/        Ajustes de render (URP) y perfiles de efectos
├── Editor/               Scripts que solo funcionan en el editor (menú "Goty Devs")
├── Escenas/              Escenas del juego (.unity)
├── Scripts/              Código del juego, una carpeta por tema
│   ├── Armas/            Tipos y datos de las armas (TipoMunicion)
│   ├── Enemigos/         Enemigos y su comportamiento
│   ├── Jugador/          El protagonista, su cámara, el inventario y las armas
│   ├── Menu/             Menú principal, opciones y pausa
│   ├── Recogibles/       Objetos que se recogen (munición)
│   └── Sistemas/         Código que usa todo el juego (por ejemplo, Ajustes)
└── Terceros/             Assets descargados (no se modifican). Ej.: Infima Games
```

**Carpetas que se crean solo cuando se necesiten** (Git no guarda carpetas vacías y Unity les crea un `.meta`):

| Carpeta | Para qué |
|---|---|
| `Assets/Prefabs/` | Objetos armados y reutilizables (enemigos, armas, puertas) |
| `Assets/Animaciones/` | Animaciones y animators propios |
| `Assets/Arte/Modelos/` | Modelos 3D propios |
| `Assets/Arte/Texturas/` | Texturas propias |
| `Assets/Scripts/Niveles/` | Puertas, escaleras, cambio de nivel |

Fuera de `Assets/`:

```
docs/                     Documentación del equipo (este archivo y la guía de scripts)
Packages/                 Paquetes de Unity (no se edita a mano)
ProjectSettings/          Configuración del proyecto Unity
README.md                 Presentación del proyecto
```

## 2. Reglas de nombres

| Qué | Regla | Ejemplo |
|---|---|---|
| Carpetas | Español, `PascalCase`, sin tildes ni espacios | `Scripts/Enemigos` |
| Scripts | Español, `PascalCase`; la clase se llama igual que el archivo | `MenuPrincipal.cs` |
| Variables y métodos | `camelCase` para variables, `PascalCase` para métodos | `velocidadCaminar`, `Disparar()` |
| Constantes | `PascalCase` | `AlturaOjos` |
| Escenas | `PascalCase`, con guion bajo si lleva detalle | `Sotano_Conceptual.unity` |
| Imágenes y sonidos | Minúsculas, sin tildes, con guion bajo | `ambiente_menu.wav` |
| Materiales | `PascalCase` | `Concreto.mat` |
| Objetos de la escena | Español, `PascalCase` con guion bajo | `Pared_Fondo`, `Luz_Techo` |

## 3. Reglas de orden

1. **Cada archivo en su carpeta.** Si no existe la carpeta del tema, se crea (y se agrega aquí).
2. **Mover o renombrar desde Unity** (arrastrando en la ventana *Project*), no desde el Explorador de Windows. Así los `.meta` viajan con el archivo y no se rompen las referencias.
3. **No se sube** lo que Unity regenera (`Library`, `Temp`, etc.): lo cubre el `.gitignore`.
4. **Terceros:** todo asset descargado va en `Assets/Terceros/` sin modificarlo, y se anota en `Assets/CREDITOS.md` con autor, fuente y licencia.
5. **Un cambio, un tema** por commit. El ámbito del commit es el tema o la carpeta: `feat(menu): añade panel de opciones`.
6. **Escenas:** se avisa al equipo antes de editar una escena que otra persona también usa (las escenas se fusionan mal en Git).

## 4. Cómo documentar el código

Todos los scripts siguen el mismo formato:

1. **Encabezado:** un comentario arriba de la clase que dice *qué hace* y *a qué objeto se agrega*.
2. **Variables públicas:** un comentario corto de qué es cada una (o `[Header]` / `[Tooltip]` para que se vea en el Inspector).
3. **Métodos:** un comentario de una línea que dice qué hace, no cómo.
4. **Comentarios simples y en español.** Se explica el *por qué* de lo que no es obvio.
5. **Guía de scripts:** al crear o cambiar un script se actualiza [`SCRIPTS.md`](SCRIPTS.md).

Ejemplo:

```csharp
// Controla la estamina del jugador: baja al correr y se recupera al descansar.
// Se agrega al objeto "Jugador".
public class Estamina : MonoBehaviour
{
    public float estaminaMaxima = 100f;   // valor con el que parte el jugador

    // Descuenta estamina; devuelve false si no alcanza.
    public bool Gastar(float cantidad) { ... }
}
```
