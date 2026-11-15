using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Muestra el inventario con la tecla Tab (RF-09), con estilo de maletín (inspirado en Resident Evil 4):
// los 4 slots dentro del maletín, el protagonista a la derecha y abajo lo que hay en el slot elegido.
// El slot se elige con las flechas (o W, A, S, D) o haciendo clic. Clic en un arma (o Enter) la equipa (RF-11).
// Mientras está abierto, el juego se detiene.
// Se agrega al objeto "Canvas_Inventario" de la escena de juego.
public class MenuInventario : MonoBehaviour
{

    
    public GameObject panelInventario;
    public Inventario inventario;
    public Protagonista protagonista;

    [Header("Audio")]
    public AudioSource audioInventario;
    public AudioClip sonidoAbrir;
    public AudioClip sonidoCerrar;

    [Header("Slots (en el mismo orden que el inventario)")]
    public Text[] textosSlots;      // texto dentro de cada slot
    public Image[] bordesSlots;     // borde de cada slot; se ilumina el elegido
    public Image[] fondosSlots;     // relleno de cada slot
    public Button[] botonesSlots;   // para elegir un slot con el mouse

    [Header("Barra inferior")]
    public Text textoNombre;        // qué hay en el slot elegido
    public Text textoDetalle;       // cantidad o descripción

    const int Columnas = 2;         // los slots se ven en una grilla de 2 x 2

    static readonly Color BordeNormal = new Color(0.45f, 0.04f, 0.04f);
    static readonly Color BordeElegido = new Color(0.95f, 0.20f, 0.12f);
    static readonly Color FondoNormal = new Color(0f, 0f, 0f, 0.35f);
    static readonly Color FondoElegido = new Color(0.45f, 0.04f, 0.04f, 0.35f);

    // Lo consulta MenuPausa para no abrir la pausa encima del inventario
    public static bool EstaAbierto { get; private set; }

    int seleccionado;               // slot elegido (0 a 3)

    void Start()
    {
        if (inventario == null) inventario = FindAnyObjectByType<Inventario>();
        if (protagonista == null) protagonista = FindAnyObjectByType<Protagonista>();

        // Al hacer clic en un slot, se elige y, si es un arma, se equipa
        for (int i = 0; i < botonesSlots.Length; i++)
        {
            int indice = i; // copia para que cada botón recuerde su propio número
            botonesSlots[i].onClick.AddListener(() => AlHacerClic(indice));
        }

        Cerrar(); // el juego empieza con el inventario cerrado
    }

    void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.tabKey.wasPressedThisFrame)
        {
            if (EstaAbierto) Cerrar();
            else if (Time.timeScale > 0f) Abrir(); // si el juego está en pausa, no se abre
            return;
        }

        if (!EstaAbierto) return;

        // Mover la selección por la grilla de slots
        if (teclado.rightArrowKey.wasPressedThisFrame || teclado.dKey.wasPressedThisFrame) MoverSeleccion(1);
        if (teclado.leftArrowKey.wasPressedThisFrame || teclado.aKey.wasPressedThisFrame) MoverSeleccion(-1);
        if (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame) MoverSeleccion(Columnas);
        if (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame) MoverSeleccion(-Columnas);
        if (teclado.enterKey.wasPressedThisFrame) EquiparSeleccionada();
        if (teclado.qKey.wasPressedThisFrame) BotarSeleccionado();
    }

    // Bota lo que hay en el slot elegido (con la tecla Q): queda en el suelo frente al jugador
    void BotarSeleccionado()
    {
        protagonista.BotarDelInventario(seleccionado);
        ActualizarTextos();
        Seleccionar(seleccionado);
    }

    // Clic en un slot: lo elige y, si tiene un arma, la equipa
    void AlHacerClic(int indice)
    {
        Seleccionar(indice);
        EquiparSeleccionada();
    }

    // Equipa el arma del slot elegido (RF-11) y actualiza lo que se ve
    void EquiparSeleccionada()
    {
        if (inventario.ObtenerSlot(seleccionado).Arma == null) return;
        protagonista.EquiparArma(seleccionado);
        ActualizarTextos();
        Seleccionar(seleccionado);
    }

    void Abrir()
    {
        EstaAbierto = true;
        ActualizarTextos();
        Seleccionar(seleccionado);
        panelInventario.SetActive(true);
        if (audioInventario != null && sonidoAbrir != null) audioInventario.PlayOneShot(sonidoAbrir);
        Time.timeScale = 0f;                       // detiene el juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Cerrar()
    {
        EstaAbierto = false;
        if (audioInventario != null && sonidoCerrar != null) audioInventario.PlayOneShot(sonidoCerrar);
        panelInventario.SetActive(false);
        Time.timeScale = 1f;                       // el juego sigue
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Al salir de la escena el inventario queda cerrado
    void OnDestroy()
    {
        EstaAbierto = false;
    }

    void MoverSeleccion(int salto)
    {
        int nuevo = seleccionado + salto;
        if (nuevo >= 0 && nuevo < textosSlots.Length) Seleccionar(nuevo);
    }

    // Ilumina el slot elegido y muestra su contenido en la barra inferior
    void Seleccionar(int indice)
    {
        seleccionado = indice;
        for (int i = 0; i < bordesSlots.Length; i++)
        {
            bool elegido = i == seleccionado;
            bordesSlots[i].color = elegido ? BordeElegido : BordeNormal;
            fondosSlots[i].color = elegido ? FondoElegido : FondoNormal;
        }

        Slot slot = inventario.ObtenerSlot(seleccionado);
        textoNombre.text = Nombre(slot);
        textoDetalle.text = Detalle(slot);
    }

    void ActualizarTextos()
    {
        for (int i = 0; i < textosSlots.Length && i < inventario.CantidadSlots; i++)
        {
            Slot slot = inventario.ObtenerSlot(i);
            if (slot.Arma != null)
            {
                string texto = Nombre(slot);
                if (slot.Arma.UsaMunicion) texto += "\n" + slot.Arma.municionActual + " / " + slot.Arma.municionPorCarga;
                if (slot.Arma == protagonista.ArmaEquipada) texto += "\nEQUIPADA";
                textosSlots[i].text = texto;
            }
            else if (slot.TieneMunicion)
                textosSlots[i].text = Nombre(slot) + "\n" + slot.Cantidad + " / " + Slot.Capacidad(slot.TipoMunicion);
            else
                textosSlots[i].text = Nombre(slot);
        }
    }

    // Nombre de lo que hay en un slot, por ejemplo "Cargadores"
    string Nombre(Slot slot)
    {
        if (slot.Arma != null) return slot.Arma.name;
        if (slot.Reservado) return "Espacio ocupado";
        if (slot.TieneMunicion) return slot.TipoMunicion == TipoMunicion.Cargador ? "Cargadores" : "Cartuchos";
        return "Vacío";
    }

    // Detalle para la barra inferior, por ejemplo "2 de 4 · para armas de cargador"
    string Detalle(Slot slot)
    {
        if (slot.Arma != null)
        {
            Arma arma = slot.Arma;
            string balas = "Cuerpo a cuerpo";
            if (arma.UsaMunicion)
            {
                string unidad = arma.tipoMunicion == TipoMunicion.Cargador ? " balas" : " cartuchos";
                balas = arma.municionActual + " de " + arma.municionPorCarga + unidad;
            }
            string estado = arma == protagonista.ArmaEquipada ? "Equipada" : "Clic: equipar";
            return balas + "  ·  " + estado + "  ·  Q: botar";
        }
        if (slot.Reservado) return "Lo ocupa un arma grande  ·  Q: botar";
        if (slot.TieneMunicion)
            return slot.Cantidad + " de " + Slot.Capacidad(slot.TipoMunicion) + "  ·  Q: botar";
        return "Espacio libre";
    }
}
