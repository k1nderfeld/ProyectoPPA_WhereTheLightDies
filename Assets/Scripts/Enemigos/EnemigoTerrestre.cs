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
    [SerializeField] private float distanciaPared = 1f;

    Rigidbody rb;
    Vector3 destino;
    float temporizador;

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

    protected override void Patrullar()
    {
        // Cada cierto tiempo elige un punto nuevo dentro de un círculo
        temporizador += Time.deltaTime;
        if (temporizador >= tiempoDireccion)
        {
            temporizador = 0f;
            Vector2 circulo = Random.insideUnitCircle * radioPatrulla;
            destino = transform.position + new Vector3(circulo.x, 0f, circulo.y);
        }

        // Si tiene una pared enfrente, se devuelve
        if (Physics.Raycast(transform.position, transform.forward, distanciaPared))
        {
            destino = transform.position - transform.forward * radioPatrulla;
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

        MoverHacia(jugador.position, velocidadPersecucion);
    }

    void MoverHacia(Vector3 punto, float velocidad)
    {
        Vector3 direccion = punto - transform.position;
        direccion.y = 0f;
        if (direccion.magnitude < 0.1f) return; // ya llegó

        transform.rotation = Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direccion), 180f*Time.deltaTime);
        Vector3 movimiento = direccion.normalized * velocidad;
        rb.linearVelocity = new Vector3(movimiento.x, rb.linearVelocity.y, movimiento.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        destino = transform.position - transform.forward * radioPatrulla;
        temporizador = 0f;
    }


}
