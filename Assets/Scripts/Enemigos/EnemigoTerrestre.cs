using UnityEngine;

// Enemigo que camina por el suelo (basado en el de Pablo).
// Patrulla eligiendo puntos al azar cerca de él y da la vuelta si choca con una pared.
// Se mueve con un Rigidbody, así respeta la gravedad y los choques.
public class EnemigoTerrestre : Enemigo
{
    [SerializeField] private float velocidadPatrulla = 1.5f;
    [SerializeField] private float velocidadPersecucion = 2.5f;
    [SerializeField] private float distanciaParaDetenerse = 1.2f; // menor que el rango de ataque
    [SerializeField] private float radioPatrulla = 10f;
    [SerializeField] private float tiempoDireccion = 4f;  // cada cuánto cambia de destino
    [SerializeField] private float distanciaPared = 1.5f;

    Rigidbody rb;
    Vector3 destino;
    float temporizador;
    float tiempoQuieto;

    void Awake()
    {
        // Se mueve con física: si el objeto no trae Rigidbody, se le agrega uno
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        // El NavMeshAgent ya no se usa y peleaba con el Rigidbody por mover al enemigo
        UnityEngine.AI.NavMeshAgent agente = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agente != null)
            agente.enabled = false;
    }

    protected override void Start()
    {
        base.Start();
        rb.freezeRotation = true; // que no se caiga al chocar
        destino = transform.position;
        temporizador = tiempoDireccion;
    }

    protected override void Update()
    {
        base.Update();

        // El modelo elige la animación según qué tan rápido se mueve:
        // casi 0 = quieto, velocidad de patrulla = caminar, velocidad de persecución = correr
        if (animador != null && !EstaMuerto)
        {
            Vector3 plano = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            animador.SetFloat("Velocidad", plano.magnitude);
        }
    }

    protected override void Patrullar()
    {
        // Cada cierto tiempo elige un punto nuevo dentro de un círculo
        temporizador += Time.deltaTime;
        if (temporizador >= tiempoDireccion)
        {
            temporizador = 0f;
            destino = ElegirDestino();
        }

        // Si está atascado (quiere caminar pero casi no avanza), elige otro destino
        if (EstaAtascado())
        {
            destino = ElegirDestino();
            temporizador = 0f;
        }

        MoverHacia(destino, velocidadPatrulla);
    }

    protected override void Perseguir()
    {
        Vector3 alJugador = jugador.position - transform.position;
        alJugador.y = 0f;

        // Si ya está cerca, se detiene a atacar en vez de pegarse al jugador
        if (alJugador.magnitude <= distanciaParaDetenerse)
        {
            transform.rotation = Quaternion.LookRotation(alJugador);
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        // Si lo perdió de vista y se quedó atascado, se rinde y vuelve a patrullar
        if (!VeAlJugadorAhora && EstaAtascado())
        {
            OlvidarJugador();
            return;
        }

        // Si lo ve va directo hacia él; si no, va al último lugar donde lo vio
        MoverHacia(VeAlJugadorAhora ? jugador.position : UltimaPosicionVista, velocidadPersecucion);
    }

    // Camina hacia un punto. Si hay algo en el camino, prueba girar hacia los lados
    // (45° y 90° a la derecha o a la izquierda) y toma la primera dirección libre.
    void MoverHacia(Vector3 punto, float velocidad)
    {
        Vector3 direccion = punto - transform.position;
        direccion.y = 0f;
        if (direccion.magnitude < 0.3f) // ya llegó
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }
        direccion.Normalize();

        if (HayObstaculo(direccion))
        {
            float[] giros = { 45f, -45f, 90f, -90f };
            foreach (float giro in giros)
            {
                Vector3 alternativa = Quaternion.Euler(0f, giro, 0f) * direccion;
                if (!HayObstaculo(alternativa))
                {
                    direccion = alternativa;
                    break;
                }
            }
        }

        // Gira de a poco (no de golpe) para que se vea natural
        Quaternion mirar = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, mirar, 8f * Time.deltaTime);
        Vector3 movimiento = direccion * velocidad;
        rb.linearVelocity = new Vector3(movimiento.x, rb.linearVelocity.y, movimiento.z);
    }

    // Busca un punto al azar al que pueda llegar caminando en línea recta (sin paredes en medio).
    // Prueba hasta 8 veces; si no encuentra, se da media vuelta.
    Vector3 ElegirDestino()
    {
        for (int intento = 0; intento < 8; intento++)
        {
            Vector2 circulo = Random.insideUnitCircle.normalized * Random.Range(3f, radioPatrulla);
            Vector3 punto = transform.position + new Vector3(circulo.x, 0f, circulo.y);
            Vector3 hacia = punto - transform.position;
            if (!Physics.Raycast(transform.position, hacia.normalized, hacia.magnitude + 0.6f, ~0, QueryTriggerInteraction.Ignore))
                return punto;
        }
        return transform.position - transform.forward * 3f;
    }

    // Revisa con 6 rayos si hay algo adelante: a la altura de las rodillas y del pecho,
    // y desde el centro y los dos costados del cuerpo (para no engancharse en esquinas y pilares).
    // El jugador y las zonas invisibles no cuentan como obstáculo.
    bool HayObstaculo(Vector3 direccion)
    {
        float[] alturas = { -0.6f, 0.3f };      // respecto al centro del enemigo
        float[] costados = { 0f, -0.45f, 0.45f }; // centro, izquierda y derecha
        Vector3 derecha = Vector3.Cross(Vector3.up, direccion);
        foreach (float altura in alturas)
        {
            foreach (float costado in costados)
            {
                Vector3 origen = transform.position + Vector3.up * altura + derecha * costado;
                RaycastHit golpe;
                if (Physics.Raycast(origen, direccion, out golpe, distanciaPared, ~0, QueryTriggerInteraction.Ignore)
                    && golpe.transform != jugador && !golpe.transform.IsChildOf(transform))
                    return true;
            }
        }
        return false;
    }

    // Atascado = lleva más de 1 segundo moviéndose a menos de 0,3 m/s
    bool EstaAtascado()
    {
        Vector3 plano = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (plano.magnitude < 0.3f) tiempoQuieto += Time.deltaTime;
        else tiempoQuieto = 0f;

        if (tiempoQuieto > 1f)
        {
            tiempoQuieto = 0f;
            return true;
        }
        return false;
    }
}
