using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Muestra el inventario con la tecla Tab (RF-09): el protagonista a la izquierda y los 4 slots
// con sus armas y munición. Mientras está abierto, el juego se detiene. Se cierra con Tab.
// Se agrega al objeto "Canvas_Inventario" de la escena de juego.
public class MenuInventario : MonoBehaviour
{
    public GameObject panelInventario;
    public Text[] textosSlots;              // un texto por slot, en el mismo orden que el inventario
    public Inventario inventario;

    // Lo consulta MenuPausa para no abrir la pausa encima del inventario
    public static bool EstaAbierto { get; private set; }

    void Start()
    {
        if (inventario == null) inventario = FindAnyObjectByType<Inventario>();
        Cerrar(); // el juego empieza con el inventario cerrado
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.tabKey.wasPressedThisFrame) return;

        if (EstaAbierto) Cerrar();
        else if (Time.timeScale > 0f) Abrir(); // si el juego está en pausa, no se abre
    }

    void Abrir()
    {
        EstaAbierto = true;
        ActualizarTextos();
        panelInventario.SetActive(true);
        Time.timeScale = 0f;                       // detiene el juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Cerrar()
    {
        EstaAbierto = false;
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

    void ActualizarTextos()
    {
        for (int i = 0; i < textosSlots.Length && i < inventario.CantidadSlots; i++)
            textosSlots[i].text = Describir(inventario.ObtenerSlot(i));
    }

    // Texto de un slot, por ejemplo "Cargadores" y debajo "2 / 4"
    string Describir(Slot slot)
    {
        if (slot.Arma != null) return slot.Arma.name;
        if (slot.Reservado) return "(ocupado por el arma)";
        if (slot.TieneMunicion)
        {
            string nombre = slot.TipoMunicion == TipoMunicion.Cargador ? "Cargadores" : "Cartuchos";
            return nombre + "\n" + slot.Cantidad + " / " + Slot.Capacidad(slot.TipoMunicion);
        }
        return "Vacío";
    }
}
