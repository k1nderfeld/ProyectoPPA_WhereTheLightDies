using UnityEngine;

// Controla la estamina del jugador: se gasta al correr y se recupera al descansar.
// Se agrega al objeto "Jugador".
public class Estamina : MonoBehaviour
{
    public float estaminaMaxima = 100f;
    public float gastoPorSegundo = 20f;          // lo que baja mientras corre
    public float recuperacionPorSegundo = 15f;   // lo que sube al descansar
    public float esperaRecuperacion = 2f;        // segundos sin correr antes de recuperar
    public float minimoParaCorrer = 0.2f;        // si se agota, necesita el 20 % para volver a correr

    public float estaminaActual;                 // pública solo para verla en el Inspector

    MovimientoJugador movimiento;
    float tiempoSinCorrer;
    bool agotada;                                // true desde que llega a 0 hasta recuperar el mínimo

    // Otros scripts preguntan si el jugador puede correr ahora
    public bool PuedeCorrer
    {
        get { return !agotada && estaminaActual > 0f; }
    }

    void Awake()
    {
        movimiento = GetComponent<MovimientoJugador>();
    }

    void Start()
    {
        estaminaActual = estaminaMaxima;
    }

    void Update()
    {
        if (movimiento.EstaCorriendo)
        {
            // Corriendo: gasta estamina y reinicia la espera
            estaminaActual -= gastoPorSegundo * Time.deltaTime;
            tiempoSinCorrer = 0f;

            if (estaminaActual <= 0f)
            {
                estaminaActual = 0f;
                agotada = true;
            }
        }
        else
        {
            // Sin correr: cuenta el tiempo y, pasada la espera, recupera
            tiempoSinCorrer += Time.deltaTime;

            if (tiempoSinCorrer >= esperaRecuperacion)
            {
                estaminaActual += recuperacionPorSegundo * Time.deltaTime;
                estaminaActual = Mathf.Min(estaminaActual, estaminaMaxima);
            }

            // Si estaba agotada, vuelve a poder correr al recuperar el mínimo
            if (agotada && estaminaActual >= estaminaMaxima * minimoParaCorrer)
                agotada = false;
        }
    }
}
