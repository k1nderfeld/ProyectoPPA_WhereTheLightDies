using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Crea la escena conceptual (no funcional) del sótano a partir del boceto de arte.
// Se usa desde el menú de Unity: Goty Devs > Crear escena conceptual del sótano
//
// Si falta algún modelo del pack Infima Games, se usa una figura simple en su lugar
// y aparece una advertencia en la Console.
public static class CrearEscenaConceptual
{
    // ---------- Rutas ----------
    const string RutaEscena = "Assets/Escenas/Sotano_Conceptual.unity";
    const string CarpetaMateriales = "Assets/Arte/Materiales";
    const string RutaPerfil = "Assets/Configuracion/PerfilSotano.asset";
    const string CarpetaModelos = "Assets/Terceros/Infima Games/Low Poly Shooter Pack - Free Sample/Art/Meshes/";

    // ---------- Medidas de la habitación (metros) ----------
    const float Tamano = 10f;       // ancho y largo
    const float Alto = 3f;
    const float Grosor = 0.2f;      // grosor de paredes, piso y techo
    const float MitadTamano = Tamano / 2f;

    // ---------- Jugador ----------
    const float AlturaOjos = 1.7f;
    const float CampoVision = 70f;

    [MenuItem("Goty Devs/Crear escena conceptual del sótano")]
    public static void Crear()
    {
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AsegurarCarpeta(CarpetaMateriales);

        var materiales = new Materiales();
        var habitacion = new GameObject("Habitacion").transform;

        ConstruirHabitacion(habitacion, materiales);
        ConstruirProps(habitacion, materiales);
        ConfigurarAmbiente();
        var camara = ConstruirJugador(materiales);
        ConstruirOjos(camara.transform.position, materiales);
        ConstruirPostProceso();
        ConstruirHudDePrueba();

        EditorSceneManager.SaveScene(escena, RutaEscena);
        AssetDatabase.SaveAssets();
        Debug.Log("Escena conceptual creada en " + RutaEscena);
    }

    // =====================================================================
    // Materiales
    // =====================================================================

    // Agrupa los materiales que usan las figuras simples de la escena.
    class Materiales
    {
        public Material Concreto = Crear("Concreto", new Color(0.30f, 0.30f, 0.31f), 0.15f);
        public Material Piso = Crear("Piso", new Color(0.17f, 0.17f, 0.18f), 0.25f);
        public Material Metal = Crear("Metal", new Color(0.12f, 0.12f, 0.14f), 0.5f);
        public Material Madera = Crear("Madera", new Color(0.25f, 0.17f, 0.11f), 0.1f);
        public Material Saco = Crear("Saco", new Color(0.55f, 0.48f, 0.36f), 0.05f);
        public Material Arma = Crear("Arma", new Color(0.10f, 0.10f, 0.11f), 0.4f);
        public Material Barril = Crear("Barril", new Color(0.45f, 0.10f, 0.07f), 0.3f);
        public Material Tanque = Crear("Tanque", new Color(0.18f, 0.24f, 0.17f), 0.35f);
        public Material OjoBlanco = Crear("OjoBlanco", new Color(0.9f, 0.86f, 0.78f), 0.6f, new Color(0.6f, 0.56f, 0.5f));
        public Material OjoIris = Crear("OjoIris", new Color(0.75f, 0.05f, 0.05f), 0.5f, new Color(0.9f, 0.05f, 0.03f));
        public Material OjoPupila = Crear("OjoPupila", Color.black, 0.9f);
    }

    // Crea (o reutiliza) un material URP Lit guardado en la carpeta Materials.
    static Material Crear(string nombre, Color color, float suavidad, Color? emision = null)
    {
        string ruta = CarpetaMateriales + "/" + nombre + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, ruta);
        }

        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", suavidad);
        if (emision.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emision.Value);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    // =====================================================================
    // Utilidades para crear objetos
    // =====================================================================

    static void AsegurarCarpeta(string ruta)
    {
        if (AssetDatabase.IsValidFolder(ruta)) return;
        int ultimaBarra = ruta.LastIndexOf('/');
        AssetDatabase.CreateFolder(ruta.Substring(0, ultimaBarra), ruta.Substring(ultimaBarra + 1));
    }

    // Crea una figura simple (cubo, esfera, cilindro...) con un material.
    static GameObject CrearFigura(string nombre, Transform padre, Vector3 posicion, Vector3 escala,
                                  Material material, PrimitiveType tipo = PrimitiveType.Cube)
    {
        var figura = GameObject.CreatePrimitive(tipo);
        figura.name = nombre;
        figura.transform.SetParent(padre, false);
        figura.transform.localPosition = posicion;
        figura.transform.localScale = escala;
        figura.GetComponent<Renderer>().sharedMaterial = material;
        return figura;
    }

    // Instancia un modelo del pack. Devuelve null si el archivo no existe.
    // Si se pasa un material, reemplaza al del modelo (la muestra gratis no trae el del barril ni el del tanque).
    static GameObject CrearModelo(string rutaRelativa, string nombre, Transform padre,
                                  Vector3 posicion, Vector3 rotacion, float escala = 1f,
                                  Material material = null)
    {
        var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(CarpetaModelos + rutaRelativa);
        if (modelo == null)
        {
            Debug.LogWarning("No se encontró el modelo " + rutaRelativa + ". Se usará una figura simple.");
            return null;
        }

        var instancia = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
        instancia.name = nombre;
        instancia.transform.SetParent(padre, false);
        instancia.transform.localPosition = posicion;
        instancia.transform.localEulerAngles = rotacion;
        instancia.transform.localScale = Vector3.one * escala;

        if (material != null)
        {
            foreach (var render in instancia.GetComponentsInChildren<Renderer>())
                render.sharedMaterial = material;
        }
        return instancia;
    }

    static void CrearLuz(string nombre, Vector3 posicion, Color color, float intensidad, float rango)
    {
        var objeto = new GameObject(nombre);
        objeto.transform.position = posicion;
        var luz = objeto.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = color;
        luz.intensity = intensidad;
        luz.range = rango;
        luz.shadows = LightShadows.None;
    }

    // =====================================================================
    // Habitación y objetos
    // =====================================================================

    static void ConstruirHabitacion(Transform padre, Materiales m)
    {
        float largoPared = Tamano + 0.4f;
        float yCentro = Alto / 2f;
        float afuera = MitadTamano + Grosor / 2f;

        CrearFigura("Piso", padre, new Vector3(0, -Grosor / 2f, 0), new Vector3(Tamano, Grosor, Tamano), m.Piso);
        CrearFigura("Techo", padre, new Vector3(0, Alto + Grosor / 2f, 0), new Vector3(Tamano, Grosor, Tamano), m.Concreto);
        CrearFigura("Pared_Fondo", padre, new Vector3(0, yCentro, afuera), new Vector3(largoPared, Alto, Grosor), m.Concreto);
        CrearFigura("Pared_Frente", padre, new Vector3(0, yCentro, -afuera), new Vector3(largoPared, Alto, Grosor), m.Concreto);
        CrearFigura("Pared_Izquierda", padre, new Vector3(-afuera, yCentro, 0), new Vector3(Grosor, Alto, Tamano), m.Concreto);

        // Pared derecha con una puerta: dos tramos de pared y un dintel arriba.
        CrearFigura("Pared_Derecha_A", padre, new Vector3(afuera, yCentro, -2.8f), new Vector3(Grosor, Alto, 4.4f), m.Concreto);
        CrearFigura("Pared_Derecha_B", padre, new Vector3(afuera, yCentro, 3.6f), new Vector3(Grosor, Alto, 2.8f), m.Concreto);
        CrearFigura("Puerta_Dintel", padre, new Vector3(afuera, 2.6f, 1.0f), new Vector3(Grosor, 0.8f, 2.4f), m.Concreto);
    }

    static void ConstruirProps(Transform padre, Materiales m)
    {
        // Barriles y tanque del pack. Si no están, se usan cilindros simples.
        PonerBarril("Barril_1", padre, new Vector3(-4.2f, 0f, 1.5f), m);
        PonerBarril("Barril_2", padre, new Vector3(-4.3f, 0f, 2.7f), m);

        // Estos modelos vienen acostados (rotación -90 en X para pararlos), miden cerca
        // de 1 metro de alto y su pivote está en el centro: se suben la mitad de su altura.
        var tanque = CrearModelo("Environment/Objects/Explosives/SM_Explosive_Gas_Tank.fbx", "Tanque",
                                 padre, new Vector3(-4.4f, 0.56f, -2.2f), new Vector3(-90, 30, 0), 1f, m.Tanque);
        if (tanque == null)
            CrearFigura("Tanque", padre, new Vector3(-4.4f, 0.6f, -2.2f), new Vector3(0.6f, 0.6f, 0.6f), m.Metal, PrimitiveType.Cylinder);

        // Sacos en el piso, al centro.
        CrearFigura("Saco_1", padre, new Vector3(-0.4f, 0.28f, 3.0f), new Vector3(0.9f, 0.55f, 0.7f), m.Saco, PrimitiveType.Sphere);
        CrearFigura("Saco_2", padre, new Vector3(0.7f, 0.25f, 2.6f), new Vector3(0.8f, 0.5f, 0.65f), m.Saco, PrimitiveType.Sphere);
        CrearFigura("Saco_3", padre, new Vector3(0.1f, 0.2f, 3.8f), new Vector3(0.7f, 0.4f, 0.6f), m.Saco, PrimitiveType.Sphere);

        // Cajas apiladas en la esquina del fondo.
        CrearFigura("Caja_1", padre, new Vector3(3.8f, 0.4f, 4.2f), new Vector3(0.8f, 0.8f, 0.8f), m.Madera);
        CrearFigura("Caja_2", padre, new Vector3(3.8f, 1.2f, 4.2f), new Vector3(0.7f, 0.8f, 0.7f), m.Madera);
        CrearFigura("Caja_3", padre, new Vector3(2.8f, 0.35f, 4.4f), new Vector3(0.7f, 0.7f, 0.7f), m.Madera);

        // Tubería que cruza el techo.
        var tubo = CrearFigura("Tuberia", padre, new Vector3(1.8f, 2.75f, 0f), new Vector3(0.14f, 5f, 0.14f), m.Metal, PrimitiveType.Cylinder);
        tubo.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    static void PonerBarril(string nombre, Transform padre, Vector3 posicion, Materiales m)
    {
        var barril = CrearModelo("Environment/Objects/Explosives/SM_Explosive_Barrel.fbx", nombre,
                                 padre, posicion + Vector3.up * 0.49f, new Vector3(-90, 0, 0), 0.9f, m.Barril);
        if (barril == null)
            CrearFigura(nombre, padre, posicion + Vector3.up * 0.5f, new Vector3(0.9f, 0.5f, 0.9f), m.Metal, PrimitiveType.Cylinder);
    }

    static void ConfigurarAmbiente()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.13f, 0.13f, 0.16f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.04f, 0.04f, 0.05f);
        RenderSettings.fogDensity = 0.06f;

        CrearLuz("Luz_Techo", new Vector3(0f, 2.8f, 0f), new Color(0.75f, 0.8f, 0.7f), 9f, 10f);
        CrearLuz("Luz_Puerta_Roja", new Vector3(4.2f, 1.6f, 1.5f), new Color(0.9f, 0.1f, 0.08f), 5f, 6f);
    }

    // =====================================================================
    // Jugador (cámara y arma)
    // =====================================================================

    static Camera ConstruirJugador(Materiales m)
    {
        var jugador = new GameObject("Jugador");
        jugador.transform.position = new Vector3(0f, 0f, -3.8f);

        var objetoCamara = new GameObject("Camara");
        objetoCamara.tag = "MainCamera";
        objetoCamara.transform.SetParent(jugador.transform, false);
        objetoCamara.transform.localPosition = new Vector3(0f, AlturaOjos, 0f);

        var camara = objetoCamara.AddComponent<Camera>();
        camara.fieldOfView = CampoVision;
        camara.nearClipPlane = 0.05f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Color.black;
        objetoCamara.AddComponent<AudioListener>();
        objetoCamara.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;

        ConstruirArma(objetoCamara.transform, m);
        return camara;
    }

    // Pistola del pack, pegada a la cámara abajo a la derecha (como en el boceto).
    // Los brazos del pack (SK_FP_CH_Default_Cubic.fbx) no se usan todavía: sin sus
    // animaciones quedan en pose de reposo y no sostienen el arma.
    static void ConstruirArma(Transform camara, Materiales m)
    {
        var posicion = new Vector3(0.2f, -0.2f, 0.45f); // derecha, abajo y delante de la cámara
        var rotacion = new Vector3(0f, -4f, 0f);        // apunta un poco hacia el centro

        var arma = CrearModelo("Weapons/Handguns/SK_Handgun_03.fbx", "Pistola", camara, posicion, rotacion);

        // Si falta el modelo del pack, se muestra un bloque simple como marcador.
        if (arma == null)
            CrearFigura("Arma_Marcador", camara, new Vector3(0.28f, -0.28f, 0.6f), new Vector3(0.13f, 0.17f, 0.42f), m.Arma);
    }

    // =====================================================================
    // Enemigos voladores (ojos)
    // =====================================================================

    static void ConstruirOjos(Vector3 objetivo, Materiales m)
    {
        var padre = new GameObject("Enemigos_Voladores_Ojos").transform;
        Vector3[] posiciones =
        {
            new Vector3(2.2f, 2.3f, 2.5f),
            new Vector3(-2.3f, 1.9f, 4.0f),
            new Vector3(0.4f, 2.6f, 4.6f),
        };

        for (int i = 0; i < posiciones.Length; i++)
        {
            var ojo = new GameObject("Ojo_" + (i + 1)).transform;
            ojo.SetParent(padre, false);
            ojo.position = posiciones[i];
            ojo.LookAt(objetivo); // el ojo mira hacia el jugador

            // Globo, iris y pupila, uno delante del otro.
            CrearFigura("Globo", ojo, Vector3.zero, Vector3.one * 0.8f, m.OjoBlanco, PrimitiveType.Sphere);
            CrearFigura("Iris", ojo, new Vector3(0f, 0f, 0.26f), Vector3.one * 0.5f, m.OjoIris, PrimitiveType.Sphere);
            CrearFigura("Pupila", ojo, new Vector3(0f, 0f, 0.42f), Vector3.one * 0.24f, m.OjoPupila, PrimitiveType.Sphere);
        }
    }

    // =====================================================================
    // Efectos de pantalla y HUD de prueba
    // =====================================================================

    // Viñeta roja y grano de película con un Volume de URP.
    static void ConstruirPostProceso()
    {
        var perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(RutaPerfil);
        if (perfil == null)
        {
            perfil = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(perfil, RutaPerfil);
        }

        if (!perfil.TryGet(out Vignette vineta))
        {
            vineta = perfil.Add<Vignette>(true);
            AssetDatabase.AddObjectToAsset(vineta, perfil);
        }
        vineta.active = true;
        vineta.color.Override(new Color(0.45f, 0f, 0f));
        vineta.intensity.Override(0.45f);
        vineta.smoothness.Override(0.6f);

        if (!perfil.TryGet(out FilmGrain grano))
        {
            grano = perfil.Add<FilmGrain>(true);
            AssetDatabase.AddObjectToAsset(grano, perfil);
        }
        grano.active = true;
        grano.type.Override(FilmGrainLookup.Medium1);
        grano.intensity.Override(0.35f);

        EditorUtility.SetDirty(perfil);

        var objeto = new GameObject("PostProceso");
        var volumen = objeto.AddComponent<Volume>();
        volumen.isGlobal = true;
        volumen.sharedProfile = perfil;
    }

    // Mira y munición de prueba. El diseño final no lleva HUD (RF-08):
    // para ocultarlos basta con desactivar el objeto "HUD_Prueba".
    static void ConstruirHudDePrueba()
    {
        var canvasObjeto = new GameObject("Canvas_Prueba");
        var canvas = canvasObjeto.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var escalador = canvasObjeto.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);

        var hud = new GameObject("HUD_Prueba", typeof(RectTransform));
        hud.transform.SetParent(canvasObjeto.transform, false);
        var rectHud = (RectTransform)hud.transform;
        rectHud.anchorMin = Vector2.zero;
        rectHud.anchorMax = Vector2.one;
        rectHud.offsetMin = rectHud.offsetMax = Vector2.zero;

        var rojo = new Color(0.8f, 0.08f, 0.08f, 0.9f);
        CrearRectangulo("Mira_Horizontal", hud.transform, new Vector2(26, 3), rojo);
        CrearRectangulo("Mira_Vertical", hud.transform, new Vector2(3, 26), rojo);

        var textoObjeto = new GameObject("Municion", typeof(RectTransform));
        textoObjeto.transform.SetParent(hud.transform, false);
        var rect = (RectTransform)textoObjeto.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-70f, 50f);
        rect.sizeDelta = new Vector2(300f, 100f);

        var texto = textoObjeto.AddComponent<Text>();
        texto.text = "0/6";
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 72;
        texto.alignment = TextAnchor.LowerRight;
        texto.color = new Color(0.8f, 0.8f, 0.8f, 0.85f);
    }

    // Rectángulo de color en el centro de la pantalla (para dibujar la mira).
    static void CrearRectangulo(string nombre, Transform padre, Vector2 tamano, Color color)
    {
        var objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        var rect = (RectTransform)objeto.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = tamano;
        objeto.AddComponent<Image>().color = color;
    }
}
