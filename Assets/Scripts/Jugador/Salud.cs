using UnityEngine;

// Controla la salud del jugador: baja al recibir daño.
// Se agrega al objeto "Jugador".
public class Salud : MonoBehaviour
{
    public float saludMaxima = 100f;
    public float saludActual;             // pública solo para verla en el Inspector

    public bool EstaMuerto { get; private set; }

    void Start()
    {
        saludActual = saludMaxima;
    }

    // Lo llaman los enemigos al atacar
    public void RecibirDanio(float danio)
    {
        if (EstaMuerto) return;

        saludActual -= danio;
        if (saludActual <= 0f)
        {
            saludActual = 0f;
            EstaMuerto = true;
            Debug.Log("El jugador murio");
        }
    }
}
