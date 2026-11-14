using UnityEngine;


public class EnemigoTerrestre : Enemigo
{
    [SerializeField] private float velocidadPatrulla = 2f;
    [SerializeField] private float velocidadPersecucion = 4f;
    [SerializeField] private float radioPatrulla = 10f;
    [SerializeField] private float tiempoDireccion = 4f;
    [SerializeField] private float distanciaPared=1f;
    [SerializeField] private float velocidadGiro= 5f;
    Rigidbody rb;
    Vector3 destino;
    float temporizador;



    protected override void Start()
    {
        base.Start();
        rb.freezeRotation = true;
        destino = transform.position;
        temporizador = tiempoDireccion;

    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void Patrullar()
    {
        temporizador+=Time.deltaTime;
        if(temporizador>=tiempoDireccion)
        {
            temporizador=0f;
            Vector2 circulo = Random.insideUnitCircle* radioPatrulla; 
            destino= transform.position + new Vector3(circulo.x,0f,circulo.y);
        }
        if (Physics.Raycast(transform.position, transform.forward, distanciaPared))
        {
            destino=transform.position-transform.forward * radioPatrulla;
            temporizador=0f;
        }
        Vector3 alDestino=destino - transform.position;
        alDestino.y =0f;

        transform.rotation=Quaternion.LookRotation(alDestino);
        Vector3 velocidad = alDestino.normalized*velocidadPatrulla;
        rb.linearVelocity = new Vector3(velocidad.x, rb.linearVelocity.y, velocidad.z);
    }
    
    
    protected override void Perseguir()
    {
        Vector3 alJugador = jugador.position - transform.position;
        alJugador.y = 0f;

        transform.rotation = Quaternion.LookRotation(alJugador);
        Vector3 velocidad = alJugador.normalized * velocidadPersecucion;
        rb.linearVelocity = new Vector3(velocidad.x, rb.linearVelocity.y, velocidad.z);
    }
}