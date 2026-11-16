using UnityEngine;

// Sonidos de un enemigo. El sonido es 3D: se oye más fuerte mientras más cerca está,
// y por el lado donde está (izquierda o derecha), así el jugador sabe de dónde viene.
// - Alerta: un rugido cuando ve al jugador.
// - Gruñidos: de vez en cuando mientras patrulla.
// - Pasos: mientras camina (más seguidos cuando persigue).
// - Golpe y muerte: los pide la clase Enemigo.
// Se agrega al mismo objeto que el enemigo.
public class SonidosEnemigo : MonoBehaviour
{
    public AudioClip alerta;
    public AudioClip grunido;
    public AudioClip golpe;
    public AudioClip muerte;
    public AudioClip paso;

    public float distanciaMaxima = 18f;     // más lejos de esto no se oye
    public float tiempoEntrePasos = 0.6f;   // caminando; persiguiendo es la mitad

    // Lo cambia Enemigo cada cuadro
    public bool Persiguiendo { get; set; }

    AudioSource fuente;      // para alerta, gruñido, golpe
    AudioSource fuentePasos; // aparte, para que los pasos no corten los gruñidos
    Rigidbody rb;
    float proximoGrunido;
    float proximoPaso;
    float ultimaAlerta = -10f;

    void Awake()
    {
        fuente = CrearFuente();
        fuentePasos = CrearFuente();
        rb = GetComponent<Rigidbody>();
        proximoGrunido = Time.time + Random.Range(2f, 6f);
    }

    AudioSource CrearFuente()
    {
        AudioSource nueva = gameObject.AddComponent<AudioSource>();
        nueva.playOnAwake = false;
        nueva.spatialBlend = 1f;                          // 1 = sonido 3D
        nueva.rolloffMode = AudioRolloffMode.Linear;      // baja parejo con la distancia
        nueva.minDistance = 2f;
        nueva.maxDistance = distanciaMaxima;
        return nueva;
    }

    void Update()
    {
        // Gruñidos de vez en cuando, solo si no está persiguiendo
        if (!Persiguiendo && Time.time >= proximoGrunido)
        {
            Tocar(fuente, grunido, Random.Range(0.85f, 1.1f), 0.8f);
            proximoGrunido = Time.time + Random.Range(5f, 10f);
        }

        // Pasos: solo si se está moviendo
        bool seMueve = rb != null && new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude > 0.3f;
        float espera = Persiguiendo ? tiempoEntrePasos / 2f : tiempoEntrePasos;
        if (seMueve && Time.time >= proximoPaso)
        {
            Tocar(fuentePasos, paso, Random.Range(0.8f, 1.1f), 0.6f);
            proximoPaso = Time.time + espera;
        }
    }

    public void Alerta()
    {
        // No ruge cada vez que lo pierde y lo vuelve a ver: espera 5 segundos
        if (Time.time - ultimaAlerta < 5f) return;
        ultimaAlerta = Time.time;
        Tocar(fuente, alerta, Random.Range(0.9f, 1.05f), 1f);
    }

    public void Golpe()
    {
        Tocar(fuente, golpe, Random.Range(0.9f, 1.1f), 1f);
    }

    // El enemigo se destruye al morir, así que el sonido se toca en su posición con un objeto aparte
    public void Muerte()
    {
        if (muerte != null)
            AudioSource.PlayClipAtPoint(muerte, transform.position, Ajustes.VolumenEfectos);
    }

    void Tocar(AudioSource fuente, AudioClip clip, float tono, float volumen)
    {
        if (clip == null) return;
        fuente.pitch = tono; // un poco distinto cada vez para que no suene repetido
        fuente.PlayOneShot(clip, volumen * Ajustes.VolumenEfectos);
    }
}
