using System.Collections.Generic;
using UnityEngine;

// Sonidos del protagonista. Lee lo que está pasando en Protagonista y suena según eso:
// - Pasos o carrera (en loop) mientras se mueve.
// - Respiración agitada (en loop) cuando se le acaba la estamina.
// - Latido (en loop) cuando le queda poca salud.
// - Quejido al recibir un golpe y grito al morir.
// - Sonido al recoger algo (lo pide Protagonista).
// Se agrega al objeto "Jugador", junto a Protagonista.
[RequireComponent(typeof(Protagonista))]
public class SonidosJugador : MonoBehaviour
{
    public AudioClip paso;
    public AudioClip correr;
    public AudioClip respiracion;
    public AudioClip latido;
    public AudioClip dolor;
    public AudioClip muerte;
    public AudioClip recoger;

    public float saludParaLatido = 35f;      // con esta salud o menos se oye el corazón
    public float estaminaParaRespirar = 0.3f; // empieza a jadear con menos del 30 % de estamina

    Protagonista protagonista;
    AudioSource fuentePasos;       // pasos y carrera
    AudioSource fuenteRespiracion;
    AudioSource fuenteLatido;
    AudioSource fuenteEfectos;     // golpes, muerte, recoger
    float ultimoDanioEscuchado = -10f;
    bool muerteSono;
    Dictionary<AudioClip, float> inicios = new Dictionary<AudioClip, float>(); // se calcula una vez por sonido

    void Awake()
    {
        protagonista = GetComponent<Protagonista>();
        fuentePasos = CrearFuente(true);
        fuenteRespiracion = CrearFuente(true);
        fuenteLatido = CrearFuente(true);
        fuenteEfectos = CrearFuente(false);
    }

    // Los sonidos del jugador son 2D: se oyen igual por los dos lados (salen "de uno mismo")
    AudioSource CrearFuente(bool repetir)
    {
        AudioSource fuente = gameObject.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.loop = repetir;
        fuente.spatialBlend = 0f;
        return fuente;
    }

    void Update()
    {
        bool detenido = Time.timeScale == 0f; // pausa, inventario o pantalla final
        float volumen = Ajustes.VolumenEfectos;

        // Pasos: carrera si corre, pasos si camina, nada si está quieto
        AudioClip clipPasos = null;
        if (!detenido && !protagonista.EstaMuerto && protagonista.SeMueve)
            clipPasos = protagonista.EstaCorriendo ? correr : paso;
        Loop(fuentePasos, clipPasos, 0.6f * volumen);

        // Respiración cuando está cansado
        // (empieza antes de agotarse, para que no suene tarde)
        float estamina = protagonista.EstaminaActual / protagonista.EstaminaMaxima;
        bool cansado = !detenido && !protagonista.EstaMuerto
                       && (protagonista.EstaCansado || estamina < estaminaParaRespirar);
        Loop(fuenteRespiracion, cansado ? respiracion : null, 0.8f * volumen);

        // Latido con poca salud
        bool pocaSalud = !protagonista.EstaMuerto && protagonista.SaludActual <= saludParaLatido;
        Loop(fuenteLatido, pocaSalud ? latido : null, volumen);

        // Golpe recibido: Protagonista guarda cuándo fue el último
        if (protagonista.TiempoUltimoDanio > ultimoDanioEscuchado)
        {
            ultimoDanioEscuchado = protagonista.TiempoUltimoDanio;
            if (!protagonista.EstaMuerto) Tocar(dolor);
        }

        // Muerte (una sola vez)
        if (protagonista.EstaMuerto && !muerteSono)
        {
            muerteSono = true;
            Tocar(muerte);
        }
    }

    // Deja sonando el clip en loop, o lo para si el clip es null
    void Loop(AudioSource fuente, AudioClip clip, float volumen)
    {
        fuente.volume = volumen;
        if (clip == null)
        {
            if (fuente.isPlaying) fuente.Stop();
            return;
        }
        if (fuente.clip != clip || !fuente.isPlaying)
        {
            fuente.clip = clip;
            fuente.time = InicioDelSonido(clip); // se salta el silencio del comienzo del archivo
            fuente.Play();
        }
    }

    // Muchos sonidos descargados traen silencio al principio y por eso parecían llegar tarde.
    // Busca la primera muestra que suene (más fuerte que 0,02) y devuelve ese segundo.
    float InicioDelSonido(AudioClip clip)
    {
        if (inicios.ContainsKey(clip)) return inicios[clip];
        inicios[clip] = BuscarInicio(clip);
        return inicios[clip];
    }

    float BuscarInicio(AudioClip clip)
    {
        float[] muestras = new float[clip.samples * clip.channels];
        if (!clip.GetData(muestras, 0)) return 0f;
        for (int i = 0; i < muestras.Length; i++)
        {
            if (Mathf.Abs(muestras[i]) > 0.02f)
                return (float)(i / clip.channels) / clip.frequency;
        }
        return 0f;
    }

    public void Recoger()
    {
        Tocar(recoger);
    }

    void Tocar(AudioClip clip)
    {
        if (clip != null) fuenteEfectos.PlayOneShot(clip, Ajustes.VolumenEfectos);
    }
}
