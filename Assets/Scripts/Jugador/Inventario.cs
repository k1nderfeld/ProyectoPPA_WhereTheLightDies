using UnityEngine;

// El inventario del protagonista: 4 slots para armas y munición (RF-09).
// Es la clase "Inventario" del diagrama de clases (el protagonista lo posee).
// Se agrega al objeto "Jugador", junto al Protagonista. En pantalla se ve con MenuInventario (tecla Tab).
public class Inventario : MonoBehaviour
{
    [SerializeField] private int cantidadSlots = 4;
    [SerializeField] private Slot[] slots;                   // se ven en el Inspector durante el juego

    [Header("Munición con la que parte el jugador")]
    [SerializeField] private int cargadoresIniciales = 2;
    [SerializeField] private int cartuchosIniciales = 6;

    public int CantidadSlots { get { return cantidadSlots; } }

    void Awake()
    {
        // Crea los slots vacíos
        slots = new Slot[cantidadSlots];
        for (int i = 0; i < cantidadSlots; i++)
            slots[i] = new Slot();
    }

    void Start()
    {
        AgregarMunicion(TipoMunicion.Cargador, cargadoresIniciales);
        AgregarMunicion(TipoMunicion.Cartucho, cartuchosIniciales);
    }

    public Slot ObtenerSlot(int indice)
    {
        return slots[indice];
    }

    // Guarda munición: primero completa los slots que ya tienen ese tipo y después usa slots vacíos.
    // Devuelve la cantidad que no cupo (0 si cupo todo).
    public int AgregarMunicion(TipoMunicion tipo, int cantidad)
    {
        foreach (Slot slot in slots)
        {
            if (cantidad > 0 && slot.TieneMunicion && slot.TipoMunicion == tipo)
                cantidad = slot.AgregarMunicion(tipo, cantidad);
        }
        foreach (Slot slot in slots)
        {
            if (cantidad > 0 && slot.EstaVacio)
                cantidad = slot.AgregarMunicion(tipo, cantidad);
        }
        return cantidad;
    }

    // Saca munición de un tipo (por ejemplo, al recargar). Devuelve cuánta se sacó.
    public int QuitarMunicion(TipoMunicion tipo, int cantidad)
    {
        int sacada = 0;
        // De atrás hacia adelante, para vaciar primero los últimos slots
        for (int i = slots.Length - 1; i >= 0 && sacada < cantidad; i--)
        {
            if (slots[i].TieneMunicion && slots[i].TipoMunicion == tipo)
                sacada += slots[i].QuitarMunicion(cantidad - sacada);
        }
        return sacada;
    }

    // Cuánta munición de un tipo hay en total
    public int ContarMunicion(TipoMunicion tipo)
    {
        int total = 0;
        foreach (Slot slot in slots)
        {
            if (slot.TieneMunicion && slot.TipoMunicion == tipo)
                total += slot.Cantidad;
        }
        return total;
    }
}
