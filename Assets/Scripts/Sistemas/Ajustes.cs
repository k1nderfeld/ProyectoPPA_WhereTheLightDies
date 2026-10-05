using UnityEngine;

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

    // Aplica las opciones que afectan a todo el juego.
    public static void Aplicar()
    {
        AudioListener.volume = VolumenGeneral;
        Screen.fullScreen = PantallaCompleta;
    }

    public static void Guardar()
    {
        PlayerPrefs.Save();
    }
}
