using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Agrega el inventario a la escena que está abierta:
// - el componente Inventario al objeto del Protagonista (y lo conecta), y
// - la pantalla del inventario (Canvas_Inventario con MenuInventario), que se abre con Tab.
// Se usa desde el menú de Unity: Goty Devs > Agregar inventario a la escena abierta
//
// No borra nada de la escena. Si la escena ya tiene la pantalla del inventario, no hace nada.
public static class CrearInventario
{
    const string RutaProtagonista = "Assets/Arte/Concepto/protagonista_concepto.png";

    static readonly Color Rojo = new Color(0.75f, 0.05f, 0.05f);
    static readonly Color TextoClaro = new Color(0.86f, 0.86f, 0.83f);
    static readonly Color FondoSlot = new Color(0.07f, 0.07f, 0.08f, 1f);
    static readonly Color BordeSlot = new Color(0.45f, 0.04f, 0.04f, 1f);

    static Font fuente;

    [MenuItem("Goty Devs/Agregar inventario a la escena abierta")]
    public static void Agregar()
    {
        if (Object.FindAnyObjectByType<MenuInventario>() != null)
        {
            Debug.LogWarning("Esta escena ya tiene la pantalla del inventario. No se agregó otra.");
            return;
        }

        var protagonista = Object.FindAnyObjectByType<Protagonista>();
        if (protagonista == null)
        {
            Debug.LogError("No hay ningún Protagonista en la escena abierta.");
            return;
        }

        // 1) Componente Inventario en el Jugador, conectado al Protagonista
        var inventario = protagonista.GetComponent<Inventario>();
        if (inventario == null) inventario = protagonista.gameObject.AddComponent<Inventario>();
        var datos = new SerializedObject(protagonista);
        datos.FindProperty("inventario").objectReferenceValue = inventario;
        datos.ApplyModifiedPropertiesWithoutUndo();

        // 2) Pantalla del inventario
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var objetoCanvas = new GameObject("Canvas_Inventario");
        var canvas = objetoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // por encima del HUD de prueba
        var escalador = objetoCanvas.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);
        escalador.matchWidthOrHeight = 0.5f;

        // Fondo oscuro que cubre toda la pantalla
        var panel = CrearRect("PanelInventario", objetoCanvas.transform);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);

        var titulo = CrearTexto("Titulo", panel, "INVENTARIO", 80, Rojo, FontStyle.Bold);
        Colocar(titulo.rectTransform, new Vector2(0, 430), new Vector2(1200, 110));

        // El protagonista solo se ve aquí, en el inventario
        var dibujo = CrearRect("Protagonista", panel).gameObject.AddComponent<Image>();
        dibujo.sprite = ConfigurarSprite(RutaProtagonista);
        dibujo.preserveAspect = true;
        dibujo.raycastTarget = false;
        Colocar(dibujo.rectTransform, new Vector2(-480, -40), new Vector2(560, 825));

        // Los 4 slots en una grilla de 2 x 2
        Vector2[] posiciones = { new Vector2(120, 130), new Vector2(540, 130), new Vector2(120, -150), new Vector2(540, -150) };
        var textos = new Text[posiciones.Length];
        for (int i = 0; i < posiciones.Length; i++)
            textos[i] = CrearSlot(panel, i, posiciones[i]);

        var ayuda = CrearTexto("Ayuda", panel, "TAB para cerrar", 28, new Color(1f, 1f, 1f, 0.5f), FontStyle.Normal);
        Colocar(ayuda.rectTransform, new Vector2(0, -470), new Vector2(800, 50));

        // Script que abre y cierra el inventario, con sus referencias conectadas
        var menu = objetoCanvas.AddComponent<MenuInventario>();
        menu.panelInventario = panel.gameObject;
        menu.textosSlots = textos;
        menu.inventario = inventario;
        panel.gameObject.SetActive(false); // oculto al empezar; se muestra con Tab

        Scene escena = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("Inventario agregado a " + escena.name);
    }

    // Un slot: borde rojo oscuro, fondo oscuro, número arriba a la izquierda y el contenido al centro.
    static Text CrearSlot(Transform padre, int indice, Vector2 posicion)
    {
        var borde = CrearRect("Slot_" + (indice + 1), padre);
        Colocar(borde, posicion, new Vector2(380, 240));
        borde.gameObject.AddComponent<Image>().color = BordeSlot;

        var fondo = CrearRect("Fondo", borde);
        fondo.anchorMin = Vector2.zero;
        fondo.anchorMax = Vector2.one;
        fondo.offsetMin = new Vector2(3, 3);
        fondo.offsetMax = new Vector2(-3, -3);
        fondo.gameObject.AddComponent<Image>().color = FondoSlot;

        var numero = CrearTexto("Numero", borde, (indice + 1).ToString(), 26, new Color(1f, 1f, 1f, 0.4f), FontStyle.Normal);
        numero.alignment = TextAnchor.UpperLeft;
        numero.rectTransform.anchorMin = Vector2.zero;
        numero.rectTransform.anchorMax = Vector2.one;
        numero.rectTransform.offsetMin = new Vector2(14, 10);
        numero.rectTransform.offsetMax = new Vector2(-14, -10);

        var contenido = CrearTexto("Contenido", borde, "Vacío", 36, TextoClaro, FontStyle.Normal);
        contenido.rectTransform.anchorMin = Vector2.zero;
        contenido.rectTransform.anchorMax = Vector2.one;
        contenido.rectTransform.offsetMin = Vector2.zero;
        contenido.rectTransform.offsetMax = Vector2.zero;
        return contenido;
    }

    // Deja una imagen lista para usarse en la interfaz (UI).
    static Sprite ConfigurarSprite(string ruta)
    {
        var importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (importador == null)
        {
            Debug.LogError("No se encontró la imagen " + ruta);
            return null;
        }
        if (importador.textureType != TextureImporterType.Sprite)
        {
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.mipmapEnabled = false;
            importador.wrapMode = TextureWrapMode.Clamp;
            importador.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    static RectTransform CrearRect(string nombre, Transform padre)
    {
        var objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        return objeto.GetComponent<RectTransform>();
    }

    // Coloca un elemento respecto del centro de la pantalla, con una posición y un tamaño.
    static void Colocar(RectTransform rect, Vector2 posicion, Vector2 tamano)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }

    static Text CrearTexto(string nombre, Transform padre, string contenido, int tamano, Color color, FontStyle estilo)
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
}
