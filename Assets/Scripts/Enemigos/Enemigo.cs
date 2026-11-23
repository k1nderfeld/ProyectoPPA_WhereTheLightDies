using UnityEngine;

// Clase base de todos los enemigos (basada en la de Pablo).
// Ve al jugador con rayos en un cono, lo persigue, lo ataca cuando está cerca y muere al quedar sin salud.
// Cada tipo de enemigo (terrestre, volador) decide cómo patrulla y cómo persigue.
// Si el enemigo tiene un modelo con Animator (hijo), se le avisan las acciones:
//   trigger "Atacar", trigger "Golpeado", trigger "Morir" y el float "Velocidad".
public abstract class Enemigo : MonoBehaviour
{
    [SerializeField] protected float salud = 50f;
    [SerializeField] protected float rangoDeteccion = 10f;
    [SerializeField] protected float rangoAtaque = 1.5f;
    [SerializeField] protected float danio = 34f;              // 3 golpes matan al jugador (salud 100)
    [SerializeField] protected float tiempoEntreAtaques = 1f;
    [SerializeField] protected float tiempoMemoria = 3f;       // segundos que sigue persiguiendo después de perderlo de vista
    [SerializeField] protected float distanciaOido = 3f;       // a esta distancia lo nota aunque esté a su espalda
    [SerializeField] protected float distanciaOidoCorriendo = 8f; // si corre, lo oye desde más lejos
    [SerializeField] protected Color colorAtaque = Color.red;  // se pone de este color al golpear

    [Header("Drops al morir")]
    [SerializeField] [Range(0f, 1f)] private float probMunicion = 0.35f;  // probabilidad de soltar munición
    [SerializeField] [Range(0f, 1f)] private float probCuracion = 0.15f;  // probabilidad de soltar un botiquín

    public bool EstaMuerto { get; private set; }
    protected bool VeAlJugadorAhora { get; private set; }       // si lo ve en este cuadro
    protected Vector3 UltimaPosicionVista { get; private set; } // donde lo vio por última vez

    protected Transform jugador;
    protected Protagonista protagonista;
    protected Animator animador;   // del modelo 3D; puede no tener
    float contadorAtaque;
    float ultimaVezVisto = -100f;
    Renderer[] partes;             // las mallas del enemigo, para pintarlas al atacar
    Color[] coloresNormales;
    float finDestello;
    SonidosEnemigo sonidos;        // puede no tener (entonces es un enemigo mudo)
    bool persiguiendoAntes;
    float proximaRevision;         // cuándo vuelve a lanzar los rayos de visión
    bool resultadoVision;          // lo que vio en la última revisión
    bool cazador;                  // si es true, siempre sabe dónde está el jugador (enemigos de oleada)

    protected virtual void Start()
    {
        protagonista = FindAnyObjectByType<Protagonista>();
        sonidos = GetComponent<SonidosEnemigo>();
        animador = GetComponentInChildren<Animator>();
        if (protagonista != null)
            jugador = protagonista.transform;

        // Guarda el color original de cada parte para volver a él después del golpe
        partes = GetComponentsInChildren<Renderer>();
        coloresNormales = new Color[partes.Length];
        for (int i = 0; i < partes.Length; i++)
            coloresNormales[i] = partes[i].material.color;

        // Para que no ataquen todos al mismo tiempo apenas llegan
        contadorAtaque = tiempoEntreAtaques * 0.5f;
    }

    protected virtual void Update()
    {
        if (jugador == null || EstaMuerto) return;

        // Cuando termina el destello del golpe, vuelve a su color
        if (finDestello > 0f && Time.time >= finDestello)
        {
            finDestello = 0f;
            PintarPartes(false);
        }

        // Si lo ve, guarda el momento. Sigue persiguiendo un rato aunque lo pierda de vista
        // (así no se "olvida" del jugador cuando un rayo deja de tocarlo por un instante).
        // Optimización: los 19 rayos se lanzan solo cada 0,1 segundos (no en cada cuadro).
        // Para el jugador no se nota, y con muchos enemigos ahorra bastante.
        if (Time.time >= proximaRevision)
        {
            proximaRevision = Time.time + 0.1f;
            resultadoVision = VeAlJugador() || OyeAlJugador();
        }
        VeAlJugadorAhora = !protagonista.EstaMuerto && resultadoVision;
        if (VeAlJugadorAhora)
        {
            ultimaVezVisto = Time.time;
            UltimaPosicionVista = jugador.position;
        }
        // Los enemigos de las oleadas no patrullan: van directo a buscar al jugador
        if (cazador)
        {
            ultimaVezVisto = Time.time;
            UltimaPosicionVista = jugador.position;
        }
        bool persiguiendo = !protagonista.EstaMuerto && Time.time - ultimaVezVisto <= tiempoMemoria;

        if (sonidos != null)
        {
            if (persiguiendo && !persiguiendoAntes) sonidos.Alerta(); // recién lo encontró: ruge
            sonidos.Persiguiendo = persiguiendo;
        }
        persiguiendoAntes = persiguiendo;

        if (!persiguiendo)
        {
            Patrullar();
            return;
        }

        Perseguir();

        // Distancia sin contar la altura
        Vector3 alJugador = jugador.position - transform.position;
        alJugador.y = 0f;
        if (alJugador.magnitude <= rangoAtaque)
            Atacar();
    }

    protected abstract void Patrullar();
    protected abstract void Perseguir();

    // Hace daño cada cierto tiempo mientras el jugador esté al alcance (RF-06)
    protected void Atacar()
    {
        contadorAtaque += Time.deltaTime;
        if (contadorAtaque >= tiempoEntreAtaques)
        {
            contadorAtaque = 0f;
            protagonista.RecibirDanio(danio);
            if (sonidos != null) sonidos.Golpe();
            if (animador != null) animador.SetTrigger("Atacar");

            // Se nota el golpe: el enemigo se pone rojo un instante
            PintarPartes(true);
            finDestello = Time.time + 0.2f;
        }
    }

    void PintarPartes(bool rojo)
    {
        for (int i = 0; i < partes.Length; i++)
        {
            if (partes[i] == null) continue;
            partes[i].material.color = rojo ? colorAtaque : coloresNormales[i];
        }
    }

    // Lo llaman las armas al acertar
    public void RecibirDanio(float cantidad)
    {
        if (EstaMuerto) return;

        salud -= cantidad;
        ultimaVezVisto = Time.time; // si le disparan, sabe dónde está el jugador
        if (jugador != null) UltimaPosicionVista = jugador.position;
        if (salud <= 0f)
            Morir();
        else if (animador != null)
            animador.SetTrigger("Golpeado");
    }

    // Lo llama el GestorOleadas al crear un enemigo: sale directo a cazar al jugador
    public void Alertar()
    {
        cazador = true;
    }

    // Al morir puede soltar munición o un botiquín (así el jugador sigue peleando)
    void SoltarObjeto()
    {
        Vector3 suelo = transform.position;
        float azar = Random.value;
        if (azar < probCuracion)
            Curacion.Crear(suelo);
        else if (azar < probCuracion + probMunicion)
        {
            TipoMunicion tipo = Random.value < 0.6f ? TipoMunicion.Cargador : TipoMunicion.Cartucho;
            Municion.CrearCaja(tipo, Random.Range(1, 3), suelo);
        }
    }

    protected virtual void Morir()
    {
        EstaMuerto = true;
        SoltarObjeto();
        Sangre.Muerte(transform.position);   // estallido de sangre y charco
        EfectosCamara.Congelar(0.06f);       // pausa mínima: cada muerte se siente
        if (sonidos != null)
        {
            sonidos.Muerte();
            sonidos.enabled = false; // ya no gruñe ni camina
        }

        // Sin modelo animado desaparece al tiro; con modelo, se cae y desaparece después
        if (animador == null)
        {
            Destroy(gameObject);
            return;
        }

        animador.SetTrigger("Morir");
        foreach (Collider colision in GetComponentsInChildren<Collider>())
            colision.enabled = false; // las balas y el jugador ya no chocan con el cuerpo
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; // se queda quieto en el suelo
        Destroy(gameObject, 6f);
    }

    // Lo "oye" si está muy cerca (aunque esté a su espalda) o si corre cerca.
    // Tiene que haber camino libre entre los dos: a través de una pared no lo oye.
    protected bool OyeAlJugador()
    {
        Vector3 alJugador = jugador.position - transform.position;
        alJugador.y = 0f;
        float distancia = alJugador.magnitude;
        float alcance = protagonista.EstaCorriendo ? distanciaOidoCorriendo : distanciaOido;
        if (distancia > alcance) return false;

        RaycastHit golpe;
        if (Physics.Raycast(transform.position, alJugador.normalized, out golpe, distancia + 0.5f, ~0, QueryTriggerInteraction.Ignore))
            return golpe.transform == jugador;
        return false;
    }

    // Se rinde: deja de perseguir (lo usa el enemigo cuando no puede llegar al último lugar visto)
    protected void OlvidarJugador()
    {
        ultimaVezVisto = -100f;
    }

    // Lanza 19 rayos de -45° a 45° (cada 5°). Si alguno toca al jugador, lo ve.
    // Los rayos ignoran los triggers (zonas invisibles como la salida o los objetos para recoger).
    protected bool VeAlJugador()
    {
        for (int i = 0; i < 19; i++)
        {
            float angulo = -45f + i * 5f;
            Vector3 direccion = Quaternion.Euler(0f, angulo, 0f) * transform.forward;

            RaycastHit golpe;
            if (Physics.Raycast(transform.position, direccion, out golpe, rangoDeteccion, ~0, QueryTriggerInteraction.Ignore)
                && golpe.transform == jugador)
                return true;
        }
        return false;
    }
}
