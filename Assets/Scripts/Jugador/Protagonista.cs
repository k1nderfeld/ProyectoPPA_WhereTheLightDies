using UnityEngine;
using UnityEngine.InputSystem;

// El protagonista del juego: se mueve, corre, tiene salud y estamina, recoge cosas (E),
// equipa armas desde el inventario, ataca (clic izquierdo) y recarga (R).
// Es la clase "Protagonista" del diagrama de clases (docs/diagrama_clases.png).
// Se agrega al objeto "Jugador". La cámara con el mouse va aparte, en CamaraJugador.
[RequireComponent(typeof(CharacterController), typeof(Inventario))]
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

    [Header("Inventario y armas")]
    [SerializeField] private Inventario inventario;                   // el protagonista posee un inventario
    [SerializeField] private Arma armaEquipada;                       // y puede tener un arma equipada
    [SerializeField] private Transform mano;                          // donde se guardan las armas (hijo de la cámara)
    [SerializeField] private float distanciaRecoger = 2f;             // RF-10: se recoge a 2 m o menos

    [Header("Audio del Protagonista")]
    [SerializeField] private AudioSource audioJugador;
    [SerializeField] private AudioClip sonidoCansancio;
    [SerializeField] private AudioClip sonidoPaso;
    [SerializeField] private float intervaloCaminar = 0.5f;  // Cada cuánto suena al caminar
    [SerializeField] private float intervaloCorrer = 0.3f;   // Cada cuánto suena al correr (más rápido)
    [Range(0f, 1f)] [SerializeField] private float volumenPasos = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float volumenCansancio = 1f;
    private float temporizadorPasos;

    // Otros scripts (enemigos, efectos de pantalla, avisos) pueden leer estos datos, pero no cambiarlos
    public bool EstaCorriendo { get; private set; }
    public bool SeMueve { get; private set; }       // si está caminando o corriendo (lo usa AnimacionArma)
    public bool EstaMuerto { get; private set; }
    public float SaludActual { get { return saludActual; } }
    public float SaludMaxima { get { return saludMaxima; } }
    public float EstaminaActual { get { return estaminaActual; } }
    public float EstaminaMaxima { get { return estaminaMaxima; } }
    public Inventario Inventario { get { return inventario; } }
    public Transform Mano { get { return mano; } }
    public Arma ArmaEquipada { get { return armaEquipada; } }

    // Lo más cercano que se puede recoger (null si no hay nada); lo usa el aviso "E  Recoger..."
    public IRecogible RecogibleCercano { get; private set; }

    // Mensaje corto en pantalla, por ejemplo "No hay espacio en el inventario" (null si no hay)
    public string Mensaje { get { return Time.time < finMensaje ? mensaje : null; } }

    CharacterController controlador;
    Vector2 entradaMovimiento;      // W, A, S y D de este cuadro
    float velocidadVertical;        // velocidad de caída
    float tiempoSinCorrer;
    bool estaminaAgotada;           // true desde que llega a 0 hasta recuperar el mínimo
    bool juegoDetenidoAntes;        // si en el cuadro anterior el juego estaba en pausa o en el inventario
    string mensaje;
    float finMensaje;

    void Awake()
    {
        controlador = GetComponent<CharacterController>();
        if (inventario == null) inventario = GetComponent<Inventario>();
        saludActual = saludMaxima;
        estaminaActual = estaminaMaxima;
        estaminaAgotada = false;
    }

    void Start()
    {
        saludActual = saludMaxima;
        estaminaActual = estaminaMaxima;
        estaminaAgotada = false;
    }

    void Update()
    {
        if (EstaMuerto) return;

        entradaMovimiento = LeerEntrada();
        Correr();
        Mover();
        GestionarPasos();

        // En pausa o con el inventario abierto no se ataca ni se recoge. Tampoco en el cuadro en que
        // se reanuda, para que el clic en "Reanudar" no dispare el arma.
        bool detenido = Time.timeScale == 0f;
        bool recienReanudado = juegoDetenidoAntes && !detenido;
        juegoDetenidoAntes = detenido;
        if (detenido || recienReanudado) return;

        BuscarRecogibleCercano();

        var mouse = Mouse.current;
        var teclado = Keyboard.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && armaEquipada != null) armaEquipada.usar();
        if (teclado != null && teclado.rKey.wasPressedThisFrame) RecargarArma();
        if (teclado != null && teclado.eKey.wasPressedThisFrame) Recoger();
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
        SeMueve = seMueve;
        bool puedeCorrer = !estaminaAgotada && estaminaActual > 0f;
        EstaCorriendo = quiereCorrer && seMueve && puedeCorrer;

        if (EstaCorriendo)
        {
            estaminaActual -= gastoEstaminaPorSegundo * Time.deltaTime;
            tiempoSinCorrer = 0f;

            if (estaminaActual <= 0f)
            {
                estaminaActual = 0f;
                if (!estaminaAgotada)
                {
                    estaminaAgotada = true;
                    
                    // Solo suena si el jugador estaba corriendo de verdad
                    if (audioJugador != null && sonidoCansancio != null)
                    {
                        audioJugador.PlayOneShot(sonidoCansancio, volumenCansancio);
                    }
                }
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

    void GestionarPasos()
    {
        // Si el jugador se está moviendo con las teclas y está en el suelo
        if (entradaMovimiento.sqrMagnitude > 0.01f && controlador.isGrounded)
        {
            temporizadorPasos -= Time.deltaTime;
            if (temporizadorPasos <= 0f)
            {
                if (audioJugador != null && sonidoPaso != null)
                {
                    audioJugador.PlayOneShot(sonidoPaso, volumenPasos);
                }

                // Ajusta el intervalo dependiendo de si está corriendo o caminando
                temporizadorPasos = EstaCorriendo ? intervaloCorrer : intervaloCaminar;
            }
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

    // Guarda una unidad de munición en el inventario (RF-10). Devuelve true si cupo.
    public bool RecogerMunicion(TipoMunicion tipo)
    {
        return inventario.AgregarMunicion(tipo, 1) == 0;
    }

    // Recarga el arma equipada con la munición del inventario (RF-15), con la tecla R
    public void RecargarArma()
    {
        if (armaEquipada != null) armaEquipada.Recargar(inventario);
    }

    // Equipa el arma que está en la posición "indice" del inventario (RF-11): guarda la anterior y saca la nueva
    public void EquiparArma(int indice)
    {
        Arma arma = inventario.ObtenerSlot(indice).Arma;
        if (arma == null || arma == armaEquipada) return;

        if (armaEquipada != null) armaEquipada.Mostrar(false);
        armaEquipada = arma;
        armaEquipada.Mostrar(true);
    }

    // Bota lo que hay en el slot "indice" del inventario: queda en el suelo, frente al jugador,
    // y se puede volver a recoger con E. Si el slot es un espacio reservado, bota el arma que lo ocupa.
    public void BotarDelInventario(int indice)
    {
        while (indice > 0 && inventario.ObtenerSlot(indice).Reservado)
            indice--;

        Slot slot = inventario.ObtenerSlot(indice);
        Vector3 suelo = transform.position + transform.forward * 1f; // un metro adelante, a la altura de los pies

        if (slot.Arma != null)
        {
            Arma arma = slot.Arma;
            if (arma == armaEquipada) armaEquipada = null; // si estaba en la mano, queda sin arma
            inventario.SacarArma(arma);
            arma.Soltar(suelo, transform.rotation);
        }
        else if (slot.TieneMunicion)
        {
            Municion.CrearCaja(slot.TipoMunicion, slot.Cantidad, suelo);
            slot.Vaciar();
        }
    }

    // Recoge lo que esté cerca con la tecla E (RF-10)
    void Recoger()
    {
        if (RecogibleCercano == null) return;
        if (!RecogibleCercano.Recoger(this)) MostrarMensaje("No hay espacio en el inventario");
    }

    // Busca lo más cercano que se pueda recoger, dentro de la distancia permitida
    void BuscarRecogibleCercano()
    {
        RecogibleCercano = null;
        float menorDistancia = float.MaxValue;

        Collider[] cercanos = Physics.OverlapSphere(transform.position, distanciaRecoger);
        foreach (Collider cercano in cercanos)
        {
            IRecogible recogible = cercano.GetComponentInParent<IRecogible>();
            if (recogible == null) continue;

            float distancia = Vector3.Distance(transform.position, cercano.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                RecogibleCercano = recogible;
            }
        }
    }

    void MostrarMensaje(string texto)
    {
        mensaje = texto;
        finMensaje = Time.time + 2f; // se ve durante 2 segundos
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