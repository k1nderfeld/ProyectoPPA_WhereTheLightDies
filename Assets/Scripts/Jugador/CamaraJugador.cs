using UnityEngine;
using UnityEngine.InputSystem;

// Mueve la vista con el mouse. Se agrega al objeto "Jugador".
public class CamaraJugador : MonoBehaviour
{
    public Transform camara;               // la cámara (hija del jugador)
    public float gradosPorPixel = 0.1f;    // se multiplica por la sensibilidad de las opciones
    public float limiteVertical = 85f;     // hasta dónde se puede mirar arriba y abajo

    float giroVertical;                    // cuánto está inclinada la cámara

    void Start()
    {
        // Oculta el cursor y lo deja fijo en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (Time.timeScale == 0f) return; // en pausa, la cámara no se mueve

        // Cuánto se movió el mouse en este cuadro
        Vector2 movimiento = Mouse.current.delta.ReadValue() * gradosPorPixel * Ajustes.Sensibilidad;

        // Izquierda y derecha: gira todo el cuerpo
        transform.Rotate(0f, movimiento.x, 0f);

        // Arriba y abajo: gira solo la cámara, sin pasar del límite
        giroVertical -= movimiento.y;
        giroVertical = Mathf.Clamp(giroVertical, -limiteVertical, limiteVertical);
        camara.localRotation = Quaternion.Euler(giroVertical, 0f, 0f);
    }
}