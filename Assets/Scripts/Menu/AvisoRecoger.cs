using UnityEngine;
using UnityEngine.UI;

// Muestra un aviso corto abajo en la pantalla: "E  Recoger pistola" cuando hay algo cerca para recoger,
// o un mensaje del protagonista como "No hay espacio en el inventario".
// No muestra salud, munición ni estamina (el juego no tiene HUD, RF-08).
// Se agrega al objeto "Canvas_Avisos" de la escena de juego.
public class AvisoRecoger : MonoBehaviour
{
    public Protagonista protagonista;
    public Text texto;

    void Start()
    {
        if (protagonista == null) protagonista = FindAnyObjectByType<Protagonista>();
    }

    void Update()
    {
        // Con el juego en pausa o el inventario abierto no se muestra nada
        if (Time.timeScale == 0f)
        {
            texto.text = "";
            return;
        }

        if (protagonista.Mensaje != null)
            texto.text = protagonista.Mensaje;
        else if (protagonista.RecogibleCercano != null)
            texto.text = "E  " + protagonista.RecogibleCercano.Descripcion;
        else
            texto.text = "";
    }
}
