using UnityEngine;

public abstract class Enemigo : MonoBehaviour
{
    [SerializeField] protected float salud = 50f;
    [SerializeField] protected float rangoDeteccion = 10f;
    [SerializeField] protected float rangoAtaque = 1.5f;
    [SerializeField] protected float danio = 10f;
    [SerializeField] protected float delayEntreAtaques = 0.5f;
    float contadorAtaque;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected Transform jugador;
    protected Salud saludJugador;



    protected virtual void Start()
    {
        saludJugador = FindAnyObjectByType<Salud>();

        if(saludJugador!=null) {
         
            jugador = saludJugador.transform;
        
        }

    }
    protected abstract void Patrullar();
    protected abstract void Perseguir();

    protected void Atacar()
    {
        contadorAtaque+=Time.deltaTime;

        if (contadorAtaque >= delayEntreAtaques)
        {
            contadorAtaque=0f;
            saludJugador.RecibirDanio(danio);
        }
    }

    protected virtual void Morir()
    {
        Destroy(gameObject);
    }
    public void RecibirDanio(float cantidad)
    {
     salud -= cantidad;
     if(salud<=0f) Morir();   
    }

    // Update is called once per frame
    protected virtual void Update()
{
    if (jugador == null || saludJugador == null) return;

    if (saludJugador.EstaMuerto)
    {
        Patrullar();
        return;
    }

    if (!veAlJugador())
    {
        Patrullar();
        return;
    }

    Perseguir();

    Perseguir();

    Vector3 alJugador = jugador.position - transform.position;
    alJugador.y = 0f;
    Debug.Log("distancia: " + alJugador.magnitude + " | rangoAtaque: " + rangoAtaque);
    if (alJugador.magnitude <= rangoAtaque)
        {
            Atacar();
        }
}

    protected bool veAlJugador()
    {
        if(saludJugador.EstaMuerto) return false;

        for (int i=0; i < 19;i++)
        {
            float angulo = -45f+i *5f;
            Vector3 direccion = Quaternion.Euler(0f,angulo,0f) * transform.forward;

            Debug.DrawRay(transform.position, direccion*rangoDeteccion,Color.red);


            RaycastHit hit;
            if(Physics.Raycast(transform.position,direccion, out hit, rangoDeteccion)&&hit.transform == jugador)
            {
                return true;
            }
        }
        return false;
    }



}
