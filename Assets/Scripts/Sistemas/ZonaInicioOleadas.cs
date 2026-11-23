using UnityEngine;

// Zona invisible (trigger) que hace empezar las oleadas cuando el jugador la cruza.
// Va en la puerta de la sala de armas: así el jugador primero recorre el pasillo y toma sus armas.
// Necesita un Collider marcado como "Is Trigger".
public class ZonaInicioOleadas : MonoBehaviour
{
    public GestorOleadas oleadas;

    void OnTriggerEnter(Collider otro)
    {
        if (otro.GetComponent<Protagonista>() == null) return; // solo cuenta el jugador

        oleadas.Activar();
        Destroy(gameObject); // ya cumplió su función
    }
}
