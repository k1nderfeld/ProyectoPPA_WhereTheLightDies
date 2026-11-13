using UnityEngine;
using UnityEngine.InputSystem;

// El protagonista del juego: se mueve, corre y tiene salud y estamina.
// Es la clase "Protagonista" del diagrama de clases (docs/diagrama_clases.png) y reúne lo que
// antes eran los scripts MovimientoJugador, Estamina y Salud.
// Se agrega al objeto "Jugador". La cámara con el mouse va aparte, en CamaraJugador.
[RequireComponent(typeof(CharacterController))]
public class Protagonista : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField] private float saludMaxima = 100f;
    [SerializeField] private float saludActual;                       // se ve en el Inspector

    [Header("Estamina")]
    [SerializeField] private float estaminaMaxima = 100f;
    [SerializeField] private float estaminaActual;                    // se ve en el Inspector
    [SerializeField] private float gastoEstaminaPorSegundo = 20f;     // lo que baja mientras corre
    [SerializeField] private float recuperacionEstaminaPorSegundo = 15f; // lo que sube al descansar
    [SerializeField] private float esperaRecuperacion = 2f;           // segundos sin correr antes de recuperar
    [SerializeField] private float minimoParaCorrer = 0.2f;           // si se agota, necesita el 20 % para volver a correr

    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 3f;             // metros por segundo
    [SerializeField] private float velocidadCorrer = 5f;              // al menos 1,5 veces la de caminar (RF-03)
    [SerializeField] private float gravedad = -20f;                   // qué tan fuerte cae

    [Header("Armas (se completan en la issue #10)")]
    [SerializeField] private Inventario inventario;                   // el protagonista posee un inventario
    [SerializeField] private Arma armaEquipada;                       // y puede tener un arma equipada

    // Otros scripts (enemigos, efectos de pantalla) pueden leer estos datos, pero no cambiarlos
    public bool EstaCorriendo { get; private set; }
    public bool EstaMuerto { get; private set; }
    public float SaludActual { get { return saludActual; } }
    public float SaludMaxima { get { return saludMaxima; } }
    public float EstaminaActual { get { return estaminaActual; } }
    public float EstaminaMaxima { get { return estaminaMaxima; } }

    CharacterController controlador;
    Vector2 entradaMovimiento;      // W, A, S y D de este cuadro
    float velocidadVertical;        // velocidad de caída
    float tiempoSinCorrer;
    bool estaminaAgotada;           // true desde que llega a 0 hasta recuperar el mínimo

    void Awake()
    {
        controlador = GetComponent<CharacterController>();
    }

    void Start()
    {
        saludActual = saludMaxima;
        estaminaActual = estaminaMaxima;
    }

    void Update()
    {
        if (EstaMuerto) return;

        entradaMovimiento = LeerEntrada();
        Correr();
        Mover();
    }

    // Mueve al protagonista con W, A, S y D (RF-01), con gravedad.
    public void Mover()
    {
        // Hacia donde mira el jugador: adelante y a la derecha
        Vector3 direccion = transform.right * entradaMovimiento.x + transform.forward * entradaMovimiento.y;
        float velocidad = EstaCorriendo ? velocidadCorrer : velocidadCaminar;
        Vector3 movimiento = direccion * velocidad;

        // Gravedad: si está en el suelo se queda pegado; si no, cae cada vez más rápido
        if (controlador.isGrounded && velocidadVertical < 0f)
            velocidadVertical = -2f;
        velocidadVertical += gravedad * Time.deltaTime;
        movimiento.y = velocidadVertical;

        controlador.Move(movimiento * Time.deltaTime);
    }

    // Decide si el protagonista corre con Shift y gasta o recupera la estamina (RF-03 y RF-04).
    public void Correr()
    {
        bool quiereCorrer = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool seMueve = entradaMovimiento.sqrMagnitude > 0.01f;
        bool puedeCorrer = !estaminaAgotada && estaminaActual > 0f;
        EstaCorriendo = quiereCorrer && seMueve && puedeCorrer;

        if (EstaCorriendo)
        {
            // Corriendo: gasta estamina y reinicia la espera
            estaminaActual -= gastoEstaminaPorSegundo * Time.deltaTime;
            tiempoSinCorrer = 0f;

            if (estaminaActual <= 0f)
            {
                estaminaActual = 0f;
                estaminaAgotada = true;
            }
        }
        else
        {
            // Sin correr: cuenta el tiempo y, pasada la espera, recupera
            tiempoSinCorrer += Time.deltaTime;

            if (tiempoSinCorrer >= esperaRecuperacion)
            {
                estaminaActual += recuperacionEstaminaPorSegundo * Time.deltaTime;
                estaminaActual = Mathf.Min(estaminaActual, estaminaMaxima);
            }

            // Si estaba agotada, vuelve a poder correr al recuperar el mínimo
            if (estaminaAgotada && estaminaActual >= estaminaMaxima * minimoParaCorrer)
                estaminaAgotada = false;
        }
    }

    // Empuja a un enemigo u objeto cercano con la tecla F (RF-05). Pendiente.
    public void Empujar()
    {
    }

    // Lo llaman los enemigos al atacar (RF-06). Con la salud en 0 el protagonista muere.
    public void RecibirDanio(float danio)
    {
        if (EstaMuerto) return;

        saludActual -= danio;
        if (saludActual <= 0f)
        {
            saludActual = 0f;
            EstaMuerto = true;
            Debug.Log("El protagonista murió");
        }
    }

    // Guarda munición en el inventario al recogerla (RF-10). Pendiente: issue #10.
    public void RecogerMunicion(TipoMunicion tipo)
    {
    }

    // Recarga el arma equipada con la munición de reserva (RF-15). Pendiente: issue #10.
    public void RecargarArma()
    {
    }

    // Equipa el arma que está en la posición "indice" del inventario (RF-11). Pendiente: issue #10.
    public void EquiparArma(int indice)
    {
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