using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Agrega el inventario a la escena que está abierta:
// - el componente Inventario al objeto del Protagonista (y lo conecta), y
// - la pantalla del inventario (Canvas_Inventario con MenuInventario), que se abre con Tab.
// La pantalla tiene estilo de maletín (inspirado en Resident Evil 4), adaptado a los colores del juego.
// Se usa desde el menú de Unity: Goty Devs > Agregar inventario a la escena abierta
//
// No borra nada más de la escena. Si ya existe la pantalla del inventario, la reemplaza por una nueva.
public static class CrearInventario
{
    const string RutaProtagonista = "Assets/Arte/Concepto/protagonista_concepto.png";
    const string RutaGrilla = "Assets/Arte/Interfaz/inventario_grilla.png";
    const string RutaMarco = "Assets/Arte/Interfaz/inventario_marco_slot.png"; // contorno de 4 px, centro transparente

    static readonly Color Rojo = new Color(0.75f, 0.05f, 0.05f);
    static readonly Color TextoClaro = new Color(0.86f, 0.86f, 0.83f);
    static readonly Color TextoGris = new Color(1f, 1f, 1f, 0.5f);
    static readonly Color Barra = new Color(0.10f, 0.10f, 0.11f);
    static readonly Color MarcoExterior = new Color(0.30f, 0.30f, 0.32f);
    static readonly Color MarcoInterior = new Color(0.17f, 0.17f, 0.18f);
    static readonly Color FondoMaletin = new Color(0.05f, 0.05f, 0.06f);
    static readonly Color Metal = new Color(0.42f, 0.42f, 0.44f);
    static readonly Color BordeSlot = new Color(0.45f, 0.04f, 0.04f);
    static readonly Color FondoSlot = new Color(0f, 0f, 0f, 0.35f);

    static Font fuente;

    [MenuItem("Goty Devs/Agregar inventario a la escena abierta")]
    public static void Agregar()
    {
        var protagonista = Object.FindAnyObjectByType<Protagonista>();
        if (protagonista == null)
        {
            Debug.LogError("No hay ningún Protagonista en la escena abierta.");
            return;
        }

        // Si ya había una pantalla de inventario, se reemplaza
        var anterior = Object.FindAnyObjectByType<MenuInventario>(FindObjectsInactive.Include);
        if (anterior != null)
        {
            Object.DestroyImmediate(anterior.gameObject);
            Debug.Log("Se reemplazó la pantalla del inventario.");
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
        objetoCanvas.AddComponent<GraphicRaycaster>(); // para hacer clic en los slots

        // Fondo oscuro que cubre toda la pantalla
        var panel = CrearRect("PanelInventario", objetoCanvas.transform);
        Estirar(panel, 0);
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.94f);

        // Barra superior con el título
        var barraSuperior = CrearCaja("BarraSuperior", panel, new Vector2(0, 470), new Vector2(1800, 80), Barra);
        CrearCaja("LineaRoja", barraSuperior, new Vector2(0, -40), new Vector2(1800, 3), Rojo);
        var titulo = CrearTexto("Titulo", barraSuperior, "INVENTARIO", 48, Rojo, FontStyle.Bold, TextAnchor.MiddleLeft);
        Estirar(titulo.rectTransform, 0);
        titulo.rectTransform.offsetMin = new Vector2(40, 0);

        // El maletín: marco metálico, fondo cuadriculado y los 4 slots
        var maletin = CrearCaja("Maletin", panel, new Vector2(-250, 15), new Vector2(1180, 700), MarcoExterior);
        var interior = CrearCaja("Interior", maletin, Vector2.zero, new Vector2(1152, 672), MarcoInterior);
        var fondo = CrearCaja("Fondo", interior, Vector2.zero, new Vector2(1120, 640), FondoMaletin);
        var grilla = CrearRect("Grilla", fondo).gameObject.AddComponent<Image>();
        grilla.sprite = ConfigurarSprite(RutaGrilla);
        grilla.raycastTarget = false;
        Estirar(grilla.rectTransform, 0);
        // Cierres del maletín, en el borde de abajo
        CrearCaja("Cierre_Izquierdo", maletin, new Vector2(-300, -350), new Vector2(70, 26), Metal);
        CrearCaja("Cierre_Derecho", maletin, new Vector2(300, -350), new Vector2(70, 26), Metal);

        Vector2[] posiciones = { new Vector2(-280, 160), new Vector2(280, 160), new Vector2(-280, -160), new Vector2(280, -160) };
        var textos = new Text[posiciones.Length];
        var bordes = new Image[posiciones.Length];
        var fondos = new Image[posiciones.Length];
        var botones = new Button[posiciones.Length];
        for (int i = 0; i < posiciones.Length; i++)
            CrearSlot(fondo, i, posiciones[i], out textos[i], out bordes[i], out fondos[i], out botones[i]);

        // El protagonista a la derecha (solo se ve en el inventario)
        var dibujo = CrearRect("Protagonista", panel).gameObject.AddComponent<Image>();
        dibujo.sprite = ConfigurarSprite(RutaProtagonista);
        dibujo.preserveAspect = true;
        dibujo.raycastTarget = false;
        Colocar(dibujo.rectTransform, new Vector2(640, 15), new Vector2(520, 766));

        // Barra inferior: qué hay en el slot elegido y los controles
        var barraInferior = CrearCaja("BarraInferior", panel, new Vector2(0, -455), new Vector2(1800, 90), Barra);
        CrearCaja("LineaRoja", barraInferior, new Vector2(0, 45), new Vector2(1800, 3), Rojo);
        var nombre = CrearTexto("Nombre", barraInferior, "Vacío", 36, TextoClaro, FontStyle.Bold, TextAnchor.MiddleLeft);
        Estirar(nombre.rectTransform, 0);
        nombre.rectTransform.offsetMin = new Vector2(40, 0);
        nombre.rectTransform.offsetMax = new Vector2(-1300, 0);
        var detalle = CrearTexto("Detalle", barraInferior, "", 28, TextoGris, FontStyle.Normal, TextAnchor.MiddleLeft);
        Estirar(detalle.rectTransform, 0);
        detalle.rectTransform.offsetMin = new Vector2(520, 0);
        detalle.rectTransform.offsetMax = new Vector2(-560, 0);
        var controles = CrearTexto("Controles", barraInferior, "FLECHAS / WASD  Elegir      TAB  Cerrar", 26, TextoGris, FontStyle.Normal, TextAnchor.MiddleRight);
        Estirar(controles.rectTransform, 0);
        controles.rectTransform.offsetMax = new Vector2(-40, 0);

        // Script que abre y cierra el inventario, con sus referencias conectadas
        var menu = objetoCanvas.AddComponent<MenuInventario>();
        menu.panelInventario = panel.gameObject;
        menu.inventario = inventario;
        menu.textosSlots = textos;
        menu.bordesSlots = bordes;
        menu.fondosSlots = fondos;
        menu.botonesSlots = botones;
        menu.textoNombre = nombre;
        menu.textoDetalle = detalle;
        panel.gameObject.SetActive(false); // oculto al empezar; se muestra con Tab

        // Sin EventSystem no se puede hacer clic en los slots (este proyecto usa el Input System nuevo)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var eventos = new GameObject("EventSystem");
            eventos.AddComponent<EventSystem>();
            eventos.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        Scene escena = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("Inventario agregado a " + escena.name);
    }

    // Un slot: relleno semitransparente (se ve la cuadrícula), marco rojo encima,
    // número arriba a la izquierda y el contenido al centro. Es un botón para elegirlo con el mouse.
    static void CrearSlot(Transform padre, int indice, Vector2 posicion,
                          out Text contenido, out Image borde, out Image relleno, out Button boton)
    {
        var rectSlot = CrearRect("Slot_" + (indice + 1), padre);
        Colocar(rectSlot, posicion, new Vector2(540, 300));

        // Relleno primero (queda detrás del marco)
        var rectRelleno = CrearRect("Relleno", rectSlot);
        Estirar(rectRelleno, 0);
        relleno = rectRelleno.gameObject.AddComponent<Image>();
        relleno.color = FondoSlot;
        relleno.raycastTarget = false;

        // Marco: solo el contorno, estirado sin deformar las esquinas (9-slice)
        var rectMarco = CrearRect("Marco", rectSlot);
        Estirar(rectMarco, 0);
        borde = rectMarco.gameObject.AddComponent<Image>();
        borde.sprite = ConfigurarSprite(RutaMarco, 4);
        borde.type = Image.Type.Sliced;
        borde.color = BordeSlot;

        boton = rectMarco.gameObject.AddComponent<Button>();
        boton.targetGraphic = borde;
        boton.transition = Selectable.Transition.None;        // los colores los maneja MenuInventario
        boton.navigation = new Navigation { mode = Navigation.Mode.None };

        var numero = CrearTexto("Numero", rectSlot, (indice + 1).ToString(), 26, TextoGris, FontStyle.Normal, TextAnchor.UpperLeft);
        Estirar(numero.rectTransform, 14);

        contenido = CrearTexto("Contenido", rectSlot, "Vacío", 40, TextoClaro, FontStyle.Normal, TextAnchor.MiddleCenter);
        Estirar(contenido.rectTransform, 0);
    }

    // Deja una imagen lista para usarse en la interfaz (UI).
    // "borde" son los píxeles de cada lado que no se estiran (para imágenes de 9-slice, como el marco).
    static Sprite ConfigurarSprite(string ruta, int borde = 0)
    {
        var importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (importador == null)
        {
            Debug.LogError("No se encontró la imagen " + ruta);
            return null;
        }
        var bordeSprite = new Vector4(borde, borde, borde, borde);
        if (importador.textureType != TextureImporterType.Sprite || importador.spriteBorder != bordeSprite)
        {
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.mipmapEnabled = false;
            importador.wrapMode = TextureWrapMode.Clamp;
            importador.spriteBorder = bordeSprite;
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

    // Un rectángulo de color, con una posición y un tamaño respecto del centro de su padre.
    static RectTransform CrearCaja(string nombre, Transform padre, Vector2 posicion, Vector2 tamano, Color color)
    {
        var rect = CrearRect(nombre, padre);
        Colocar(rect, posicion, tamano);
        var imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return rect;
    }

    static void Colocar(RectTransform rect, Vector2 posicion, Vector2 tamano)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }

    // Hace que el elemento ocupe todo su padre, dejando un margen.
    static void Estirar(RectTransform rect, float margen)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(margen, margen);
        rect.offsetMax = new Vector2(-margen, -margen);
    }

    static Text CrearTexto(string nombre, Transform padre, string contenido, int tamano, Color color,
                           FontStyle estilo, TextAnchor alineacion)
    {
        var texto = CrearRect(nombre, padre).gameObject.AddComponent<Text>();
        texto.font = fuente;
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.fontStyle = estilo;
        texto.color = color;
        texto.alignment = alineacion;
        texto.raycastTarget = false;
        return texto;
    }
}
