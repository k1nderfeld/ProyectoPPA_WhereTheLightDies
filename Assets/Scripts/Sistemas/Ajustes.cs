using UnityEngine;
using UnityEngine.Rendering.Universal;

// Guarda y aplica las opciones del jugador (volumen, pantalla completa y sensibilidad).
// Los valores se guardan con PlayerPrefs, así se recuerdan al cerrar el juego.
// Es "static": se usa desde cualquier script con Ajustes.VolumenGeneral, sin crear objetos.
public static class Ajustes
{
    // Nombres con los que se guarda cada opción
    const string ClaveGeneral = "volumen_general";
    const string ClaveMusica = "volumen_musica";
    const string ClaveEfectos = "volumen_efectos";
    const string ClavePantallaCompleta = "pantalla_completa";
    const string ClaveSensibilidad = "sensibilidad";
    const string ClaveCalidad = "calidad";

    // Volúmenes de 0 a 1
    public static float VolumenGeneral
    {
        get { return PlayerPrefs.GetFloat(ClaveGeneral, 0.8f); }
        set { PlayerPrefs.SetFloat(ClaveGeneral, value); }
    }

    public static float VolumenMusica
    {
        get { return PlayerPrefs.GetFloat(ClaveMusica, 0.7f); }
        set { PlayerPrefs.SetFloat(ClaveMusica, value); }
    }

    public static float VolumenEfectos
    {
        get { return PlayerPrefs.GetFloat(ClaveEfectos, 0.8f); }
        set { PlayerPrefs.SetFloat(ClaveEfectos, value); }
    }

    public static bool PantallaCompleta
    {
        get { return PlayerPrefs.GetInt(ClavePantallaCompleta, 1) == 1; }
        set { PlayerPrefs.SetInt(ClavePantallaCompleta, value ? 1 : 0); }
    }

    // Sensibilidad del mouse (se usará en el control del jugador): de 0.2 a 3
    public static float Sensibilidad
    {
        get { return PlayerPrefs.GetFloat(ClaveSensibilidad, 1f); }
        set { PlayerPrefs.SetFloat(ClaveSensibilidad, value); }
    }

    // Calidad gráfica: 0 = Baja, 1 = Media, 2 = Alta. Por defecto Media.
    public static int Calidad
    {
        get { return PlayerPrefs.GetInt(ClaveCalidad, 1); }
        set { PlayerPrefs.SetInt(ClaveCalidad, value); }
    }

    // Aplica las opciones que afectan a todo el juego.
    public static void Aplicar()
    {
        AudioListener.volume = VolumenGeneral;
        Screen.fullScreen = PantallaCompleta;
        AplicarCalidad();
    }

    // Cambia la resolución interna y la distancia de sombras según la calidad elegida.
    // En "Baja" el juego se dibuja a menor resolución y casi sin sombras: así corre en PC de gama baja.
    public static void AplicarCalidad()
    {
        // Máximo 60 cuadros por segundo: no tiene sentido calentar el PC dibujando más.
        Application.targetFrameRate = 60;

        UniversalRenderPipelineAsset urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urp == null) return;

        if (Calidad == 0)        // Baja
        {
            urp.renderScale = 0.7f;
            urp.shadowDistance = 8f;
        }
        else if (Calidad == 1)   // Media
        {
            urp.renderScale = 0.85f;
            urp.shadowDistance = 15f;
        }
        else                     // Alta
        {
            urp.renderScale = 1f;
            urp.shadowDistance = 25f;
        }
    }

    public static void Guardar()
    {
        PlayerPrefs.Save();
    }
}
