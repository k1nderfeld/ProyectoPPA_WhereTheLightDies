using UnityEngine;

// La salida del sótano. Si el jugador entra cuando ya no quedan enemigos, gana el nivel.
// Si todavía quedan, se muestra un aviso en pantalla.
// Necesita un Collider marcado como "Is Trigger".
public class SalidaNivel : MonoBehaviour
{
    public PantallaFinal pantallaFinal;

    void OnTriggerEnter(Collider otro)
    {
        Protagonista protagonista = otro.GetComponent<Protagonista>();
        if (protagonista == null) return; // solo cuenta el jugador

        int enemigosVivos = 0;
        foreach (Enemigo enemigo in FindObjectsByType<Enemigo>())
            if (!enemigo.EstaMuerto) enemigosVivos++; // los que están cayendo muertos no cuentan
        if (enemigosVivos > 0)
        {
            protagonista.MostrarMensaje("Aún quedan " + enemigosVivos + " enemigos en el sótano");
            return;
        }

        pantallaFinal.Ganar();
    }
}
