using UnityEngine;

// Clase base de todos los enemigos (basada en la de Pablo).
// Ve al jugador con rayos en un cono, lo persigue, lo ataca cuando está cerca y muere al quedar sin salud.
// Cada tipo de enemigo (terrestre, volador) decide cómo patrulla y cómo persigue.
public abstract class Enemigo : MonoBehaviour
{
    [SerializeField] protected float salud = 50f;
    [SerializeField] protected float rangoDeteccion = 10f;
    [SerializeField] protected float rangoAtaque = 1.5f;
    [SerializeField] protected float danio = 34f;              // 3 golpes matan al jugador (salud 100)
    [SerializeField] protected float tiempoEntreAtaques = 1f;
    [SerializeField] protected Color colorAtaque = Color.red;  // se pone de este color al golpear

    protected Transform jugador;
    protected Protagonista protagonista;
    float contadorAtaque;
    Renderer[] partes;      // las mallas del enemigo, para pintarlas al atacar
    Color[] coloresNormales;
    float finDestello;

    protected virtual void Start()
    {
        protagonista = FindAnyObjectByType<Protagonista>();
        if (protagonista != null)
            jugador = protagonista.transform;

        // Guarda el color original de cada parte para volver a él después del golpe
        partes = GetComponentsInChildren<Renderer>();
        coloresNormales = new Color[partes.Length];
        for (int i = 0; i < partes.Length; i++)
            coloresNormales[i] = partes[i].material.color;
    }

    protected virtual void Update()
    {
        if (jugador == null) return;

        // Cuando termina el destello del golpe, vuelve a su color
        if (finDestello > 0f && Time.time >= finDestello)
        {
            finDestello = 0f;
            PintarPartes(false);
        }

        // Si el jugador murió o no lo ve, sigue patrullando
        if (protagonista.EstaMuerto || !VeAlJugador())
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
        salud -= cantidad;
        if (salud <= 0f) Morir();
    }

    protected virtual void Morir()
    {
        Destroy(gameObject);
    }

    // Lanza 19 rayos de -45° a 45° (cada 5°). Si alguno toca al jugador, lo ve.
    protected bool VeAlJugador()
    {
        for (int i = 0; i < 19; i++)
        {
            float angulo = -45f + i * 5f;
            Vector3 direccion = Quaternion.Euler(0f, angulo, 0f) * transform.forward;
            Debug.DrawRay(transform.position, direccion * rangoDeteccion, Color.red);

            RaycastHit golpe;
            if (Physics.Raycast(transform.position, direccion, out golpe, rangoDeteccion) && golpe.transform == jugador)
                return true;
        }
        return false;
    }
}
