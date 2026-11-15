using System.Collections;
using UnityEngine;

// Clase base de todas las armas: la clase abstracta "Arma" del diagrama de clases.
// Cada arma (Pistola, Escopeta, CuerpoACuerpo) hereda de esta y solo cambia lo que la hace distinta.
// Se agrega al objeto del arma. El arma puede estar en el suelo (se recoge con E)
// o en la mano del protagonista (guardada en el inventario y, si está equipada, visible).
public abstract class Arma : MonoBehaviour, IRecogible
{
    [Header("Datos del arma (diagrama de clases)")]
    public float danioPorRayo = 10f;        // daño de cada rayo que acierta
    public float rango = 30f;               // distancia máxima del ataque, en metros
    public int cantidadRayosAtaque = 1;     // rayos por ataque (la escopeta lanza varios)
    public float cadencia = 2f;             // ataques por segundo
    public float tiempoRecarga = 1.5f;      // segundos que tarda en recargar
    public int municionPorCarga = 6;        // balas que caben en el arma (0 = no usa munición)
    public int municionActual = 6;          // balas que tiene ahora
    public int slotsInventario = 1;         // espacios que ocupa en el inventario
    public TipoMunicion tipoMunicion = TipoMunicion.Cargador;

    [Header("En la mano (posición respecto de la cámara)")]
    public Vector3 posicionEnMano = new Vector3(0.2f, -0.2f, 0.45f);
    public Vector3 rotacionEnMano = new Vector3(0f, -4f, 0f);

    [Header("Sonidos")]
    public AudioClip sonidoAtaque;
    public AudioClip sonidoSinMunicion;
    public AudioClip sonidoRecarga;
    public AudioClip sonidoEquipar;
    public AudioClip sonidoImpacto;         // suena donde pega el rayo

    [Header("Efectos")]
    public Light fogonazo;                  // luz breve en la boca del arma al disparar
    public Material materialMarca;          // marca oscura donde pega la bala

    public bool Recargando { get; private set; }
    public bool EnLaMano { get; private set; }

    // Las armas cuerpo a cuerpo no usan munición
    public bool UsaMunicion { get { return municionPorCarga > 0; } }

    public string Descripcion { get { return "Recoger " + name.ToLower(); } }

    float proximoAtaque;                    // momento desde el que se puede volver a atacar
    float finFogonazo;                      // momento en que se apaga el fogonazo
    AudioSource fuenteAudio;

    // Cada arma pone aquí sus valores (daño, rango, munición...). Lo usa el comando del editor.
    public abstract void ValoresIniciales();

    // Al agregar el componente a mano en el Inspector, también se cargan sus valores
    void Reset()
    {
        ValoresIniciales();
    }

    void Awake()
    {
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio != null) fuenteAudio.volume = Ajustes.VolumenEfectos; // volumen de las opciones
    }

    void Update()
    {
        // Después de atacar, el arma vuelve suave a su posición y a su giro (efecto de retroceso)
        if (EnLaMano)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, posicionEnMano, 12f * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(rotacionEnMano), 10f * Time.deltaTime);
        }

        // El fogonazo dura muy poco
        if (fogonazo != null && fogonazo.enabled && Time.time > finFogonazo)
            fogonazo.enabled = false;
    }

    // Ataca: lanza "cantidadRayosAtaque" rayos desde el centro de la cámara (RF-12, RF-13 y RF-14).
    // Si un rayo pega a un enemigo, el enemigo recibe el daño.
    public void usar()
    {
        if (!sePuedeUsar())
        {
            if (UsaMunicion && municionActual <= 0 && !Recargando) Sonar(sonidoSinMunicion);
            return;
        }

        proximoAtaque = Time.time + 1f / cadencia;
        if (UsaMunicion) municionActual--;
        Sonar(sonidoAtaque);
        Retroceso();

        if (fogonazo != null)
        {
            fogonazo.enabled = true;
            finFogonazo = Time.time + 0.05f;
        }

        Transform camara = Camera.main.transform;
        for (int i = 0; i < cantidadRayosAtaque; i++)
        {
            RaycastHit golpe;
            if (Physics.Raycast(camara.position, DireccionDelRayo(camara), out golpe, rango))
            {
                Enemigo enemigo = golpe.collider.GetComponentInParent<Enemigo>();
                if (enemigo != null)
                    enemigo.RecibirDanio(danioPorRayo);
                else if (UsaMunicion && !golpe.collider.isTrigger)
                    DejarMarca(golpe); // las balas marcan paredes y suelo; la palanca no

                if (sonidoImpacto != null) AudioSource.PlayClipAtPoint(sonidoImpacto, golpe.point, 0.6f * Ajustes.VolumenEfectos);
            }
        }
    }

    // Al disparar, el arma salta hacia atrás y hacia arriba; después vuelve sola (ver Update).
    // Es "virtual": la palanca la cambia por un golpe en arco.
    protected virtual void Retroceso()
    {
        transform.localPosition = posicionEnMano + new Vector3(0f, 0.015f, -0.06f);
        transform.localRotation = Quaternion.Euler(rotacionEnMano + new Vector3(-8f, 0f, 0f));
    }

    // Deja una marca oscura donde pegó la bala. Se borra sola a los 10 segundos.
    void DejarMarca(RaycastHit golpe)
    {
        if (materialMarca == null) return;

        GameObject marca = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(marca.GetComponent<Collider>());
        marca.transform.position = golpe.point + golpe.normal * 0.005f; // un poco separada de la pared
        marca.transform.rotation = Quaternion.LookRotation(-golpe.normal); // mirando hacia afuera de la pared
        marca.transform.localScale = Vector3.one * 0.06f;
        marca.GetComponent<Renderer>().sharedMaterial = materialMarca;
        Destroy(marca, 10f);
    }

    // ¿Puede atacar ahora? No está recargando, ya pasó el tiempo entre ataques y tiene munición (si la usa).
    public bool sePuedeUsar()
    {
        if (Recargando) return false;
        if (Time.time < proximoAtaque) return false;
        if (UsaMunicion && municionActual <= 0) return false;
        return true;
    }

    // Dirección de cada rayo: recto al centro de la pantalla. La escopeta la cambia para dispersar.
    protected virtual Vector3 DireccionDelRayo(Transform camara)
    {
        return camara.forward;
    }

    // Recarga con la munición del inventario (RF-15).
    // Armas de cargador: solo cuando se vacían, y usan un cargador completo.
    // Armas de cartuchos: cuando les falta alguno, y cargan solo los que faltan.
    public void Recargar(Inventario inventario)
    {
        if (!UsaMunicion || Recargando) return;
        if (tipoMunicion == TipoMunicion.Cargador && municionActual > 0) return;
        if (municionActual >= municionPorCarga) return;
        if (inventario.ContarMunicion(tipoMunicion) == 0) return; // sin reserva no recarga
        StartCoroutine(RecargarConEspera(inventario));
    }

    IEnumerator RecargarConEspera(Inventario inventario)
    {
        Recargando = true;
        Sonar(sonidoRecarga);
        yield return new WaitForSeconds(tiempoRecarga);

        if (tipoMunicion == TipoMunicion.Cargador)
        {
            if (inventario.QuitarMunicion(TipoMunicion.Cargador, 1) == 1)
                municionActual = municionPorCarga;
        }
        else
        {
            municionActual += inventario.QuitarMunicion(TipoMunicion.Cartucho, municionPorCarga - municionActual);
        }
        Recargando = false;
    }

    // Si el arma se guarda en medio de una recarga, la recarga se cancela
    void OnDisable()
    {
        StopAllCoroutines();
        Recargando = false;
    }

    // Recoger del suelo (E): va al inventario y queda en la mano, guardada hasta equiparla (RF-10).
    public bool Recoger(Protagonista protagonista)
    {
        if (!protagonista.Inventario.AgregarArma(this)) return false;

        var colision = GetComponent<Collider>();
        if (colision != null) colision.enabled = false;

        transform.SetParent(protagonista.Mano, false);
        transform.localPosition = posicionEnMano;
        transform.localRotation = Quaternion.Euler(rotacionEnMano);
        EnLaMano = true;
        gameObject.SetActive(false); // guardada; se ve al equiparla
        return true;
    }

    // Deja el arma en el suelo (al botarla del inventario). Se puede volver a recoger con E.
    public void Soltar(Vector3 posicion, Quaternion giro)
    {
        EnLaMano = false;
        transform.SetParent(null);
        transform.position = posicion + Vector3.up * 0.03f;
        transform.rotation = giro * Quaternion.Euler(0f, 0f, 90f); // acostada de lado

        var colision = GetComponent<Collider>();
        if (colision != null) colision.enabled = true;
        gameObject.SetActive(true);
    }

    // Saca el arma (al equiparla) o la guarda
    public void Mostrar(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible) Sonar(sonidoEquipar);
    }

    void Sonar(AudioClip clip)
    {
        if (clip == null || fuenteAudio == null) return;
        fuenteAudio.volume = Ajustes.VolumenEfectos; // por si se cambió en las opciones
        fuenteAudio.PlayOneShot(clip);
    }
}
