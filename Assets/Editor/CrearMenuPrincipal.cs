using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Crea la escena del menú principal.
// Se usa desde el menú de Unity: Goty Devs > Crear menú principal
//
// Al terminar, la escena queda guardada en Assets/Escenas/MenuPrincipal.unity
// y agregada como primera escena en Build Settings (File > Build Profiles).
public static class CrearMenuPrincipal
{
    // ---------- Rutas ----------
    const string RutaEscena = "Assets/Escenas/MenuPrincipal.unity";
    const string RutaEscenaJuego = "Assets/Escenas/Sotano_Conceptual.unity";
    const string RutaFondo = "Assets/Arte/Concepto/pov_concepto.png";
    const string RutaDegradado = "Assets/Arte/Interfaz/degradado_izquierda.png";
    const string RutaMusica = "Assets/Audio/Musica/ambiente_menu.wav";
    const string RutaHover = "Assets/Audio/Efectos/hover.wav";
    const string RutaClick = "Assets/Audio/Efectos/click.wav";

    // ---------- Textos (el nombre del juego es provisorio) ----------
    const string NombreJuego = "WHERE THE\nLIGHT DIES";
    const string Version = "v0.1.0  ·  Goty Devs";

    // ---------- Colores ----------
    static readonly Color Rojo = new Color(0.75f, 0.05f, 0.05f);
    static readonly Color TextoClaro = new Color(0.86f, 0.86f, 0.83f);
    static readonly Color FondoPanel = new Color(0.04f, 0.04f, 0.05f, 0.92f);
    static readonly Color BordeRojo = new Color(0.75f, 0.05f, 0.05f, 0.55f);

    static Font fuente;

    [MenuItem("Goty Devs/Crear menú principal")]
    public static void Crear()
    {
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var fondo = ConfigurarSprite(RutaFondo);
        var degradado = ConfigurarSprite(RutaDegradado);

        CrearCamara();
        CrearEventSystem();
        var canvas = CrearCanvas();

        // Fondo: dibujo del POV, oscurecido, con degradado a la izquierda para leer el texto.
        CrearFondo(canvas.transform, fondo, degradado);
        CrearTitulo(canvas.transform);

        // Paneles (el principal visible; opciones y créditos ocultos al inicio)
        var principal = CrearPanelPrincipal(canvas.transform, out Button jugar, out Button opciones,
                                            out Button creditos, out Button salir);
        var audioMenu = CrearAudio();
        var panelOpciones = CrearPanelOpciones(canvas.transform, audioMenu, out Button volverOpciones);
        var panelCreditos = CrearPanelCreditos(canvas.transform, out Button volverCreditos);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(false);

        CrearTextoSimple("Version", canvas.transform, Version, 24, new Color(1f, 1f, 1f, 0.45f),
                         TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(40, 30), new Vector2(600, 40));

        var fundido = CrearFundido(canvas.transform);

        // Objeto que lleva el script principal y recibe todas las referencias.
        var objetoMenu = new GameObject("Menu");
        var menu = objetoMenu.AddComponent<MenuPrincipal>();
        menu.panelPrincipal = principal;
        menu.panelOpciones = panelOpciones;
        menu.panelCreditos = panelCreditos;
        menu.botonJugar = jugar;
        menu.botonOpciones = opciones;
        menu.botonCreditos = creditos;
        menu.botonSalir = salir;
        menu.botonVolverOpciones = volverOpciones;
        menu.botonVolverCreditos = volverCreditos;
        menu.fundido = fundido;

        EditorSceneManager.SaveScene(escena, RutaEscena);
        AgregarEscenasABuild();
        AssetDatabase.SaveAssets();
        Debug.Log("Menú principal creado en " + RutaEscena);
    }

    // =====================================================================
    // Utilidades
    // =====================================================================

    // Deja una imagen lista para usarse en la interfaz (UI).
    static Sprite ConfigurarSprite(string ruta)
    {
        var importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (importador == null)
        {
            Debug.LogError("No se encontró la imagen " + ruta);
            return null;
        }
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.mipmapEnabled = false;
        importador.wrapMode = TextureWrapMode.Clamp;
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    // Crea un objeto de interfaz con RectTransform.
    static RectTransform CrearRect(string nombre, Transform padre)
    {
        var objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        return objeto.GetComponent<RectTransform>();
    }

    // Hace que el objeto ocupe todo el espacio de su padre.
    static void Estirar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static Text CrearTexto(string nombre, Transform padre, string contenido, int tamano, Color color,
                           TextAnchor alineacion, FontStyle estilo = FontStyle.Normal)
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

    // Texto anclado a una esquina o borde, con posición y tamaño fijos.
    static Text CrearTextoSimple(string nombre, Transform padre, string contenido, int tamano, Color color,
                                 TextAnchor alineacion, Vector2 ancla, Vector2 posicion, Vector2 tamanoRect)
    {
        var texto = CrearTexto(nombre, padre, contenido, tamano, color, alineacion);
        var rect = texto.rectTransform;
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.pivot = ancla;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamanoRect;
        return texto;
    }

    static Image CrearImagen(string nombre, Transform padre, Sprite sprite, Color color)
    {
        var imagen = CrearRect(nombre, padre).gameObject.AddComponent<Image>();
        imagen.sprite = sprite;
        imagen.color = color;
        imagen.raycastTarget = false;
        return imagen;
    }

    static void AgregarBorde(GameObject objeto)
    {
        var borde = objeto.AddComponent<Outline>();
        borde.effectColor = BordeRojo;
        borde.effectDistance = new Vector2(2, -2);
    }

    // =====================================================================
    // Cámara, sistema de eventos y canvas
    // =====================================================================

    static void CrearCamara()
    {
        var objeto = new GameObject("Camara");
        objeto.tag = "MainCamera";
        var camara = objeto.AddComponent<Camera>();
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Color.black;
        objeto.AddComponent<AudioListener>();
    }

    // Sin EventSystem los botones no reciben clics.
    // Este proyecto usa el Input System nuevo, por eso se usa InputSystemUIInputModule.
    static void CrearEventSystem()
    {
        var objeto = new GameObject("EventSystem");
        objeto.AddComponent<EventSystem>();
        objeto.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    static Canvas CrearCanvas()
    {
        var objeto = new GameObject("Canvas");
        var canvas = objeto.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // El canvas se adapta a cualquier resolución, tomando 1920x1080 como referencia.
        var escalador = objeto.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);
        escalador.matchWidthOrHeight = 0.5f;

        objeto.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    // =====================================================================
    // Fondo y título
    // =====================================================================

    static void CrearFondo(Transform padre, Sprite fondo, Sprite degradado)
    {
        // El dibujo cubre toda la pantalla sin deformarse (se recorta si sobra).
        var imagenFondo = CrearImagen("Fondo", padre, fondo, Color.white);
        Estirar(imagenFondo.rectTransform);
        var ajuste = imagenFondo.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        ajuste.aspectRatio = fondo.rect.width / fondo.rect.height;

        // Capa oscura general + degradado más oscuro a la izquierda.
        Estirar(CrearImagen("Oscurecer", padre, null, new Color(0f, 0f, 0f, 0.35f)).rectTransform);
        Estirar(CrearImagen("Degradado", padre, degradado, Color.white).rectTransform);
    }

    static void CrearTitulo(Transform padre)
    {
        var titulo = CrearTextoSimple("Titulo", padre, NombreJuego, 110, Rojo, TextAnchor.UpperLeft,
                                      new Vector2(0, 1), new Vector2(130, -110), new Vector2(1000, 320));
        titulo.fontStyle = FontStyle.Bold;
        titulo.lineSpacing = 0.85f;
        titulo.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.8f);
        titulo.gameObject.AddComponent<TextoParpadeo>();
    }

    // =====================================================================
    // Paneles
    // =====================================================================

    static Button CrearBoton(string nombre, Transform padre, string contenido)
    {
        var rect = CrearRect(nombre, padre);
        var imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = Color.white; // el color real lo definen los colores del botón

        var boton = rect.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;
        var colores = boton.colors;
        colores.normalColor = new Color(0.07f, 0.07f, 0.08f, 0.85f);
        colores.highlightedColor = new Color(0.40f, 0.04f, 0.04f, 0.95f);
        colores.pressedColor = new Color(0.60f, 0.06f, 0.06f, 1f);
        colores.selectedColor = colores.normalColor;
        colores.fadeDuration = 0.1f;
        boton.colors = colores;

        AgregarBorde(rect.gameObject);
        rect.gameObject.AddComponent<SonidoBoton>();

        var texto = CrearTexto("Texto", rect, contenido, 40, TextoClaro, TextAnchor.MiddleLeft);
        Estirar(texto.rectTransform);
        texto.rectTransform.offsetMin = new Vector2(34, 0);
        return boton;
    }

    static VerticalLayoutGroup AgregarColumna(GameObject objeto, int separacion, int margen)
    {
        var columna = objeto.AddComponent<VerticalLayoutGroup>();
        columna.spacing = separacion;
        columna.padding = new RectOffset(margen, margen, margen, margen);
        columna.childControlWidth = true;
        columna.childControlHeight = true;
        columna.childForceExpandWidth = true;
        columna.childForceExpandHeight = false;
        return columna;
    }

    static void FijarAltura(GameObject objeto, float alto)
    {
        objeto.AddComponent<LayoutElement>().preferredHeight = alto;
    }

    static GameObject CrearPanelPrincipal(Transform padre, out Button jugar, out Button opciones,
                                          out Button creditos, out Button salir)
    {
        var rect = CrearRect("PanelPrincipal", padre);
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(130, -170);
        rect.sizeDelta = new Vector2(520, 460);
        AgregarColumna(rect.gameObject, 18, 0);

        jugar = CrearBoton("Boton_Jugar", rect, "JUGAR");
        opciones = CrearBoton("Boton_Opciones", rect, "OPCIONES");
        creditos = CrearBoton("Boton_Creditos", rect, "CRÉDITOS");
        salir = CrearBoton("Boton_Salir", rect, "SALIR");
        FijarAltura(jugar.gameObject, 86);
        FijarAltura(opciones.gameObject, 86);
        FijarAltura(creditos.gameObject, 86);
        FijarAltura(salir.gameObject, 86);
        return rect.gameObject;
    }

    // Panel oscuro centrado, para opciones y créditos.
    static RectTransform CrearPanelCentral(string nombre, Transform padre, Vector2 tamano)
    {
        var rect = CrearRect(nombre, padre);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(400, 0); // a la derecha, para no tapar el título
        rect.sizeDelta = tamano;

        var imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = FondoPanel;
        AgregarColumna(rect.gameObject, 14, 50);
        return rect;
    }

    static Text CrearTituloPanel(Transform padre, string contenido)
    {
        var texto = CrearTexto("Titulo", padre, contenido, 60, Rojo, TextAnchor.MiddleLeft, FontStyle.Bold);
        FijarAltura(texto.gameObject, 90);
        return texto;
    }

    static GameObject CrearPanelOpciones(Transform padre, GestorAudioMenu audioMenu, out Button volver)
    {
        var panel = CrearPanelCentral("PanelOpciones", padre, new Vector2(1000, 780));
        CrearTituloPanel(panel, "OPCIONES");

        var opciones = panel.gameObject.AddComponent<MenuOpciones>();
        opciones.audioMenu = audioMenu;

        opciones.sliderGeneral = CrearFilaSlider(panel, "Volumen general", 0f, 1f, out opciones.textoGeneral);
        opciones.sliderMusica = CrearFilaSlider(panel, "Música", 0f, 1f, out opciones.textoMusica);
        opciones.sliderEfectos = CrearFilaSlider(panel, "Efectos", 0f, 1f, out opciones.textoEfectos);
        opciones.sliderSensibilidad = CrearFilaSlider(panel, "Sensibilidad del mouse", 0.2f, 3f,
                                                      out opciones.textoSensibilidad);
        opciones.togglePantallaCompleta = CrearToggle(panel, "Pantalla completa");

        // Espacio flexible para empujar el botón "Volver" hacia abajo.
        var espacio = CrearRect("Espacio", panel);
        espacio.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

        volver = CrearBoton("Boton_Volver", panel, "VOLVER");
        FijarAltura(volver.gameObject, 80);
        return panel.gameObject;
    }

    // Una fila con: nombre de la opción, slider y valor (por ejemplo "80%").
    static Slider CrearFilaSlider(Transform padre, string etiqueta, float minimo, float maximo, out Text valor)
    {
        var fila = CrearRect("Fila_" + etiqueta, padre);
        var horizontal = fila.gameObject.AddComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 24;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false; // el slider mantiene su altura y la perilla se ve redonda
        horizontal.childAlignment = TextAnchor.MiddleLeft;
        FijarAltura(fila.gameObject, 64);

        var nombre = CrearTexto("Etiqueta", fila, etiqueta, 32, TextoClaro, TextAnchor.MiddleLeft);
        nombre.gameObject.AddComponent<LayoutElement>().preferredWidth = 400;

        var slider = CrearSlider(fila);
        slider.minValue = minimo;
        slider.maxValue = maximo;
        slider.value = Mathf.Lerp(minimo, maximo, 0.7f);
        var elementoSlider = slider.gameObject.AddComponent<LayoutElement>();
        elementoSlider.flexibleWidth = 1;
        elementoSlider.preferredHeight = 30;

        valor = CrearTexto("Valor", fila, "", 32, TextoClaro, TextAnchor.MiddleRight);
        valor.gameObject.AddComponent<LayoutElement>().preferredWidth = 110;
        return slider;
    }

    static Slider CrearSlider(Transform padre)
    {
        var objeto = DefaultControls.CreateSlider(RecursosUI());
        objeto.transform.SetParent(padre, false);
        var slider = objeto.GetComponent<Slider>();

        // Colores: fondo oscuro, relleno rojo y perilla clara.
        objeto.transform.Find("Background").GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.18f);
        objeto.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Rojo;
        var perilla = objeto.transform.Find("Handle Slide Area/Handle");
        perilla.GetComponent<Image>().color = TextoClaro;
        perilla.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 0); // 30 de ancho y alto del slider = círculo
        return slider;
    }

    static Toggle CrearToggle(Transform padre, string etiqueta)
    {
        var objeto = DefaultControls.CreateToggle(RecursosUI());
        objeto.transform.SetParent(padre, false);
        FijarAltura(objeto, 56);

        // Se agranda la casilla y se aclara el texto.
        var fondo = objeto.transform.Find("Background").GetComponent<RectTransform>();
        fondo.sizeDelta = new Vector2(44, 44);
        fondo.anchoredPosition = new Vector2(22, -28);
        fondo.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.18f);

        var marca = objeto.transform.Find("Background/Checkmark");
        marca.GetComponent<RectTransform>().sizeDelta = new Vector2(34, 34);
        marca.GetComponent<Image>().color = Rojo;

        var texto = objeto.transform.Find("Label").GetComponent<Text>();
        texto.text = etiqueta;
        texto.font = fuente;
        texto.fontSize = 32;
        texto.color = TextoClaro;
        texto.rectTransform.offsetMin = new Vector2(64, 0);
        texto.rectTransform.offsetMax = Vector2.zero;
        return objeto.GetComponent<Toggle>();
    }

    // Imágenes que trae Unity para sliders y casillas.
    static DefaultControls.Resources RecursosUI()
    {
        var recursos = new DefaultControls.Resources();
        recursos.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        recursos.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        recursos.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        recursos.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        return recursos;
    }

    static GameObject CrearPanelCreditos(Transform padre, out Button volver)
    {
        var panel = CrearPanelCentral("PanelCreditos", padre, new Vector2(1000, 780));
        CrearTituloPanel(panel, "CRÉDITOS");

        string contenido =
            "Goty Devs  ·  Programación Avanzada, UCN (2026)\n\n" +
            "Baltazar Rodríguez  -  Líder\n" +
            "Pablo Velásquez  -  Programador líder\n" +
            "Geancarlo Bisquett  -  Programador asistente y control de versiones\n" +
            "Luis Leyton  -  Diseñador de arte y sonido\n\n" +
            "Arte conceptual: equipo Goty Devs\n" +
            "Modelos de armas: Low Poly Shooter Pack (Infima Games)\n" +
            "Sonidos del menú: provisorios, generados con código";
        var texto = CrearTexto("Contenido", panel, contenido, 28, TextoClaro, TextAnchor.UpperLeft);
        texto.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

        volver = CrearBoton("Boton_Volver", panel, "VOLVER");
        FijarAltura(volver.gameObject, 80);
        return panel.gameObject;
    }

    // =====================================================================
    // Audio y fundido
    // =====================================================================

    static GestorAudioMenu CrearAudio()
    {
        var objeto = new GameObject("Audio");
        var gestor = objeto.AddComponent<GestorAudioMenu>();
        gestor.musica = CrearFuenteDeAudio("Musica", objeto.transform, RutaMusica);
        gestor.efectos = CrearFuenteDeAudio("Efectos", objeto.transform, null);
        gestor.sonidoHover = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaHover);
        gestor.sonidoClick = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaClick);
        return gestor;
    }

    static AudioSource CrearFuenteDeAudio(string nombre, Transform padre, string rutaClip)
    {
        var objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        var fuenteAudio = objeto.AddComponent<AudioSource>();
        fuenteAudio.playOnAwake = false; // el GestorAudioMenu decide cuándo suena
        if (rutaClip != null) fuenteAudio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(rutaClip);
        return fuenteAudio;
    }

    // Pantalla negra que se desvanece al abrir y aparece al pasar al juego.
    // Queda transparente en el editor; MenuPrincipal la pone negra al empezar el juego.
    static CanvasGroup CrearFundido(Transform padre)
    {
        var imagen = CrearImagen("Fundido", padre, null, Color.black);
        Estirar(imagen.rectTransform);
        var grupo = imagen.gameObject.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        return grupo;
    }

    // =====================================================================
    // Build Settings
    // =====================================================================

    // El menú va primero (índice 0) para que sea lo primero que se ve al abrir el juego.
    static void AgregarEscenasABuild()
    {
        var escenas = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(RutaEscena, true) };
        if (File.Exists(RutaEscenaJuego))
            escenas.Add(new EditorBuildSettingsScene(RutaEscenaJuego, true));
        EditorBuildSettings.scenes = escenas.ToArray();
    }
}
