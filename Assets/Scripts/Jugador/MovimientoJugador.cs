using UnityEngine;
using UnityEngine.InputSystem;

// Mueve al jugador con W, A, S y D. Se agrega al objeto "Jugador".
[RequireComponent(typeof(CharacterController), typeof(Estamina))]
public class MovimientoJugador : MonoBehaviour
{
    public float velocidadCaminar = 3f;   // metros por segundo
    public float velocidadCorrer = 5f; // al menos 1,5 que caminar
    public float gravedad = -20f;         // qué tan fuerte cae el jugador

    public bool EstaCorriendo{get; private set;} 
    CharacterController controlador;
    Estamina estamina;
    float velocidadVertical;              // velocidad de caída

    void Awake()
    {
        controlador = GetComponent<CharacterController>();
        estamina = GetComponent<Estamina>();
    }

    void Update()
    {
        Vector2 entrada = LeerEntrada();

        Vector3 direccion = transform.right * entrada.x + transform.forward * entrada.y;
        // Corre si mantiene Shift Y se está moviendo
        bool quiereCorrer = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool seMueve = entrada.sqrMagnitude > 0.01f;
        EstaCorriendo = quiereCorrer && seMueve && estamina.PuedeCorrer;
        float velocidad = EstaCorriendo ? velocidadCorrer : velocidadCaminar;
        Vector3 movimiento = direccion * velocidad;
        // Gravedad: si está en el suelo se queda pegado; si no, cae cada vez más rápido
        if (controlador.isGrounded && velocidadVertical < 0f)
            velocidadVertical = -2f;
        velocidadVertical += gravedad * Time.deltaTime;
        movimiento.y = velocidadVertical;

        controlador.Move(movimiento * Time.deltaTime);
    }

    // Lee las teclas y devuelve un valor entre -1 y 1 para cada eje
    Vector2 LeerEntrada()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return Vector2.zero;

        float x = 0f;
        float y = 0f;
        if (teclado.dKey.isPressed) x += 1f;
        if (teclado.aKey.isPressed) x -= 1f;
        if (teclado.wKey.isPressed) y += 1f;
        if (teclado.sKey.isPressed) y -= 1f;

        // Para que ir en diagonal no sea más rápido que ir recto
        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }
}