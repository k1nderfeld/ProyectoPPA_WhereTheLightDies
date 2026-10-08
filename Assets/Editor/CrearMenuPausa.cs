using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Agrega el menú de pausa (panel con "Reanudar" y "Volver al menú") a la escena que está abierta.
// Se usa desde el menú de Unity: Goty Devs > Agregar menú de pausa a la escena abierta
//
// No borra nada de la escena. Si la escena ya tiene un menú de pausa, no hace nada.
// También agrega la escena a Build Settings, para que el botón "Jugar" del menú pueda cargarla.
public static class CrearMenuPausa
{
    static readonly Color Rojo = new Color(0.75f, 0.05f, 0.05f);
    static readonly Color TextoClaro = new Color(0.86f, 0.86f, 0.83f);

    [MenuItem("Goty Devs/Agregar menú de pausa a la escena abierta")]
    public static void Agregar()
    {
        if (Object.FindAnyObjectByType<MenuPausa>() != null)
        {
            Debug.LogWarning("Esta escena ya tiene un menú de pausa. No se agregó otro.");
            return;
        }

        var fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Canvas (la superficie donde se dibuja la interfaz)
        var objetoCanvas = new GameObject("Canvas_Pausa");
        var canvas = objetoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var escalador = objetoCanvas.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);
        escalador.matchWidthOrHeight = 0.5f;
        objetoCanvas.AddComponent<GraphicRaycaster>();

        // Panel oscuro que cubre toda la pantalla
        var panel = CrearRect("PanelPausa", objetoCanvas.transform);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

        // Título y botones
        var titulo = CrearTexto("Titulo", panel, "PAUSA", 100, Rojo, fuente, FontStyle.Bold);
        Colocar(titulo.rectTransform, new Vector2(0, 220), new Vector2(900, 140));

        var reanudar = CrearBoton("Boton_Reanudar", panel, "REANUDAR", fuente, new Vector2(0, 40));
        var menu = CrearBoton("Boton_Menu", panel, "VOLVER AL MENÚ", fuente, new Vector2(0, -80));

        // Script que controla la pausa, con sus referencias ya conectadas
        var pausa = objetoCanvas.AddComponent<MenuPausa>();
        pausa.panelPausa = panel.gameObject;
        pausa.botonReanudar = reanudar;
        pausa.botonMenu = menu;
        panel.gameObject.SetActive(false); // oculto al empezar; se muestra con Escape

        // Sin EventSystem los botones no reciben clics (este proyecto usa el Input System nuevo)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var eventos = new GameObject("EventSystem");
            eventos.AddComponent<EventSystem>();
            eventos.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        Scene escena = SceneManager.GetActiveScene();
        AgregarABuildSettings(escena.path);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("Menú de pausa agregado a " + escena.name);
    }

    static RectTransform CrearRect(string nombre, Transform padre)
    {
        var objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        return objeto.GetComponent<RectTransform>();
    }

    // Coloca un elemento en el centro de la pantalla, con una posición y un tamaño.
    static void Colocar(RectTransform rect, Vector2 posicion, Vector2 tamano)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }

    static Text CrearTexto(string nombre, Transform padre, string contenido, int tamano,
                           Color color, Font fuente, FontStyle estilo)
    {
        var texto = CrearRect(nombre, padre).gameObject.AddComponent<Text>();
        texto.font = fuente;
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.fontStyle = estilo;
        texto.color = color;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.raycastTarget = false;
        return texto;
    }

    // Botón con los mismos colores que el menú principal.
    static Button CrearBoton(string nombre, Transform padre, string contenido, Font fuente, Vector2 posicion)
    {
        var rect = CrearRect(nombre, padre);
        Colocar(rect, posicion, new Vector2(560, 90));

        var imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = Color.white; // el color real lo definen los colores del botón

        var boton = rect.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;
        var colores = boton.colors;
        colores.normalColor = new Color(0.07f, 0.07f, 0.08f, 0.9f);
        colores.highlightedColor = new Color(0.40f, 0.04f, 0.04f, 0.95f);
        colores.pressedColor = new Color(0.60f, 0.06f, 0.06f, 1f);
        colores.selectedColor = colores.normalColor;
        colores.fadeDuration = 0.1f;
        boton.colors = colores;

        var texto = CrearTexto("Texto", rect, contenido, 40, TextoClaro, fuente, FontStyle.Normal);
        texto.rectTransform.anchorMin = Vector2.zero;
        texto.rectTransform.anchorMax = Vector2.one;
        texto.rectTransform.offsetMin = Vector2.zero;
        texto.rectTransform.offsetMax = Vector2.zero;
        return boton;
    }

    // Agrega la escena a la lista de Build Settings si todavía no está.
    static void AgregarABuildSettings(string rutaEscena)
    {
        var escenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var existente in escenas)
        {
            if (existente.path == rutaEscena) return;
        }
        escenas.Add(new EditorBuildSettingsScene(rutaEscena, true));
        EditorBuildSettings.scenes = escenas.ToArray();
    }
}
